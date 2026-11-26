using FixnSeven.Application.Common;
using FixnSeven.Application.Common.Interfaces;
using FixnSeven.Application.Features.Certificates.DTOs;
using FixnSeven.Application.Features.Orders;
using FixnSeven.Domain.Entities;
namespace FixnSeven.Application.Features.Certificates;

// Reglas de negocio de HU-14 (emitir), HU-15 (revocar) y HU-16 (consulta publica)
public class CertificateService
{
    private readonly ICertificateRepository _repository;
    private readonly IOrderRepository _orders;
    private readonly IPasswordHasher _hasher;
    private readonly ICurrentTenant _tenant;

    public CertificateService(ICertificateRepository repository, IOrderRepository orders,
        IPasswordHasher hasher, ICurrentTenant tenant)
    {
        _repository = repository;
        _orders = orders;
        _hasher = hasher;
        _tenant = tenant;
    }

    private bool IsTechnician => _tenant.Role == "Technician";

    // ---------- HU-14: emitir ----------
    public async Task<ServiceResult<CertificateResponseDto>> IssueAsync(IssueCertificateRequestDto request)
    {
        var enterpriseId = _tenant.EnterpriseId ?? throw new InvalidOperationException("No hay EnterpriseId en el token.");
        var employeeId = _tenant.EmployeeId ?? throw new InvalidOperationException("No hay EmployeeId en el token.");

        if (!await _repository.EquipmentExistsAsync(request.EquipmentId))
            return ServiceResult<CertificateResponseDto>.Fail(400, "El equipo no existe.");

        // Regla 7: un tecnico solo certifica equipos de ordenes que tiene asignadas
        if (request.ServiceOrderId is null)
        {
            if (IsTechnician)
                return ServiceResult<CertificateResponseDto>.Fail(403,
                    "Como técnico, indique la orden asignada a usted con la que inspeccionó el equipo.");
        }
        else
        {
            var order = await _orders.GetEntityAsync(request.ServiceOrderId.Value);
            if (order is null)
                return ServiceResult<CertificateResponseDto>.Fail(400, "La orden no existe.");
            if (order.EquipmentId != request.EquipmentId)
                return ServiceResult<CertificateResponseDto>.Fail(400, "La orden no corresponde a ese equipo.");
            if (IsTechnician && order.TechnicianId != employeeId)
                return ServiceResult<CertificateResponseDto>.Fail(403, "Esta orden no está asignada a usted.");
        }

        // Regla 8: maximo un certificado vigente. Primero se revoca (HU-15) y despues se emite otro.
        var current = await _repository.GetActiveEntityByEquipmentAsync(request.EquipmentId);
        Certificate? expiredToClose = null;
        if (current is not null)
        {
            if (current.ExpiresAt >= DateTime.UtcNow)
                return ServiceResult<CertificateResponseDto>.Fail(409,
                    $"El equipo ya tiene un certificado vigente ({current.VerificationCode}). Revóquelo antes de emitir otro.");

            // Ya vencio: se cierra para liberar el indice unico y conservar el historial
            current.Status = CertificateStatus.Revoked;
            current.RevokedAt = DateTime.UtcNow;
            current.RevocationReason = "Reemplazado por un certificado nuevo (estaba vencido).";
            expiredToClose = current;
        }

        string verificationCode;
        do verificationCode = CodeGenerator.Create(10);
        while (await _repository.VerificationCodeExistsAsync(verificationCode));

        var ownerCode = CodeGenerator.CreateGrouped(3, 4);   // se muestra UNA sola vez
        var validityDays = await _repository.GetValidityDaysAsync(enterpriseId);
        var now = DateTime.UtcNow;

        var certificate = new Certificate
        {
            EquipmentId = request.EquipmentId,
            ServiceOrderId = request.ServiceOrderId,
            TechnicianId = employeeId,                   // sale del JWT
            Grade = request.Grade,
            Score = request.Score,
            Observations = request.Observations?.Trim(),
            PublicSummary = request.PublicSummary?.Trim(),
            Status = CertificateStatus.Active,
            VerificationCode = verificationCode,
            OwnerCodeHash = _hasher.Hash(ownerCode),     // solo la huella, nunca el codigo
            IssuedAt = now,
            ExpiresAt = now.AddDays(validityDays)
        };
        await _repository.IssueAsync(certificate, expiredToClose);

        List<string>? warnings = null;
        if (certificate.Grade == CertificateGrades.Failed)
            warnings = ["El nivel es Reprobado: el certificado queda registrado, pero el equipo NO queda apto."];

        var technicianName = await _repository.GetEmployeeNameAsync(employeeId);
        return ServiceResult<CertificateResponseDto>.Ok(
            CertificateMapper.ToDto(certificate, technicianName, ownerCode, warnings), 201);
    }

    // ---------- HU-15: revocar ----------
    public async Task<ServiceResult<bool>> RevokeAsync(Guid id, RevokeCertificateRequestDto request)
    {
        var certificate = await _repository.GetEntityAsync(id);
        if (certificate is null) return ServiceResult<bool>.Fail(404, "El certificado no existe.");
        if (IsTechnician && certificate.TechnicianId != _tenant.EmployeeId)
            return ServiceResult<bool>.Fail(403, "Solo puede revocar los certificados que usted emitió.");
        if (certificate.Status == CertificateStatus.Revoked)
            return ServiceResult<bool>.Fail(409, "El certificado ya está revocado.");

        // No se edita: se revoca y queda el historial
        certificate.Status = CertificateStatus.Revoked;
        certificate.RevokedAt = DateTime.UtcNow;
        certificate.RevocationReason = request.Reason.Trim();
        await _repository.UpdateAsync(certificate);
        return ServiceResult<bool>.Ok(true, 204);
    }

    // ---------- Consultas ----------
    public Task<CertificateResponseDto?> GetByIdAsync(Guid id) => _repository.GetByIdAsync(id);

    // ---------- HU-16: consulta publica por QR ----------
    public Task<PublicCertificateResponseDto?> GetPublicAsync(string code) =>
        _repository.GetPublicAsync(code.Trim().ToUpperInvariant());
}
