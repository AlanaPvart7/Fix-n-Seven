using FixnSeven.Application.Common;
using FixnSeven.Application.Common.Interfaces;
using FixnSeven.Application.Features.Certificates;
using FixnSeven.Application.Features.Certificates.DTOs;
using FixnSeven.Application.Features.Reports.DTOs;
using FixnSeven.Domain.Entities;
namespace FixnSeven.Application.Features.Reports;

// Fase 2: reportes de certificados incorrectos (reglas 9, 10 y 11 del modelo)
public class ReportService
{
    private const int MaxReportsPerEmailPerDay = 3;

    private readonly IReportRepository _repository;
    private readonly ICertificateRepository _certificates;
    private readonly IFileStorage _storage;
    private readonly ICurrentTenant _tenant;

    public ReportService(IReportRepository repository, ICertificateRepository certificates,
        IFileStorage storage, ICurrentTenant tenant)
    {
        _repository = repository;
        _certificates = certificates;
        _storage = storage;
        _tenant = tenant;
    }

    private bool IsTechnician => _tenant.Role == "Technician";

    // ---------- Publico: levantar un reporte desde el enlace del QR ----------
    public async Task<ServiceResult<ReportResponseDto>> CreateAsync(string verificationCode, CreateReportRequestDto request)
    {
        var certificate = await _repository.GetCertificateByCodeAsync(verificationCode.Trim().ToUpperInvariant());
        if (certificate is null) return ServiceResult<ReportResponseDto>.Fail(404, "El certificado no existe.");

        var email = request.ReporterEmail.Trim().ToLowerInvariant();
        if (await _repository.CountByEmailSinceAsync(email, DateTime.UtcNow.AddDays(-1)) >= MaxReportsPerEmailPerDay)
            return ServiceResult<ReportResponseDto>.Fail(429,
                $"Un mismo correo puede enviar como máximo {MaxReportsPerEmailPerDay} reportes por día.");

        // Si quien reporta inicio sesion con su empresa, es un Distribuidor (sale del token)
        var reporterEnterpriseId = _tenant.EnterpriseId;

        string trackingCode;
        do trackingCode = "RPT-" + CodeGenerator.Create(8);
        while (await _repository.TrackingCodeExistsAsync(trackingCode));

        var report = new Report
        {
            EnterpriseId = certificate.EnterpriseId,     // la empresa que EMITIO el certificado
            CertificateId = certificate.Id,
            ReporterType = reporterEnterpriseId is null ? ReporterTypes.Customer : ReporterTypes.Distributor,
            ReporterEnterpriseId = reporterEnterpriseId,
            ReporterName = request.ReporterName.Trim(),
            ReporterEmail = email,
            ReporterPhone = string.IsNullOrWhiteSpace(request.ReporterPhone) ? null : request.ReporterPhone.Trim(),
            Reason = request.Reason,
            Description = request.Description.Trim(),
            TrackingCode = trackingCode
        };
        await _repository.AddAsync(report);

        return ServiceResult<ReportResponseDto>.Ok(
            new ReportResponseDto(report.Id, report.TrackingCode, report.Status, report.CreatedAt), 201);
    }

    // ---------- Publico: adjuntar pruebas con el codigo de seguimiento ----------
    public async Task<ServiceResult<AttachmentResponseDto>> AddAttachmentAsync(
        string trackingCode, string fileName, long length, Stream content)
    {
        var report = await _repository.GetByTrackingCodeAsync(trackingCode.Trim().ToUpperInvariant());
        if (report is null) return ServiceResult<AttachmentResponseDto>.Fail(404, "El reporte no existe.");
        if (report.Status is ReportStatus.Accepted or ReportStatus.Rejected)
            return ServiceResult<AttachmentResponseDto>.Fail(409, "El reporte ya fue resuelto; no se pueden agregar pruebas.");
        if (await _repository.CountAttachmentsAsync(report.Id) >= FileInspector.MaxFilesPerReport)
            return ServiceResult<AttachmentResponseDto>.Fail(409,
                $"Un reporte admite como máximo {FileInspector.MaxFilesPerReport} archivos.");
        if (length <= 0) return ServiceResult<AttachmentResponseDto>.Fail(400, "El archivo está vacío.");

        // Se leen los primeros bytes para saber el tipo REAL
        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer);
        var bytes = buffer.ToArray();
        var kind = FileInspector.Inspect(bytes.Take(1024).ToArray(), fileName);
        if (kind is null)
            return ServiceResult<AttachmentResponseDto>.Fail(400,
                "Tipo de archivo no permitido o no coincide con su extensión. Se aceptan JPG, PNG, WEBP, HEIC, PDF, MP4, MOV, DOCX, XLSX y TXT.");
        if (bytes.LongLength > kind.MaxBytes)
            return ServiceResult<AttachmentResponseDto>.Fail(400,
                $"El archivo pasa del máximo permitido para {kind.FileKind} ({kind.MaxBytes / (1024 * 1024)} MB).");

        var attachment = new ReportAttachment
        {
            EnterpriseId = report.EnterpriseId,
            ReportId = report.Id,
            FileName = Path.GetFileName(fileName),
            FileKind = kind.FileKind,
            ContentType = kind.ContentType,
            SizeBytes = bytes.LongLength
        };
        // La ruta no usa el nombre que manda el usuario: evita rutas maliciosas
        attachment.StorageKey =
            $"reports/{report.EnterpriseId}/{report.Id}/{attachment.Id}{Path.GetExtension(fileName).ToLowerInvariant()}";

        buffer.Position = 0;
        await _storage.SaveAsync(attachment.StorageKey, buffer);
        await _repository.AddAttachmentAsync(attachment);

        return ServiceResult<AttachmentResponseDto>.Ok(new AttachmentResponseDto(attachment.Id, attachment.FileName,
            attachment.FileKind, attachment.ContentType, attachment.SizeBytes, attachment.UploadedAt), 201);
    }

    // ---------- Publico: estado del reporte ----------
    public async Task<PublicReportStatusResponseDto?> GetPublicStatusAsync(string trackingCode)
    {
        var report = await _repository.GetByTrackingCodeAsync(trackingCode.Trim().ToUpperInvariant());
        if (report is null) return null;
        return new PublicReportStatusResponseDto(report.TrackingCode, report.Status, report.Resolution,
            await _repository.CountAttachmentsAsync(report.Id), report.CreatedAt, report.ResolvedAt);
    }

    // ---------- Empresa: consultar ----------
    public Task<(List<ReportSummaryResponseDto> Items, int Total)> GetPagedAsync(
        string? status, Guid? certificateId, int page, int pageSize) =>
        _repository.GetPagedAsync(status, certificateId, page, pageSize);

    // El tecnico solo ve reportes de los certificados que EL emitio
    private async Task<(Report Report, CertificateResponseDto Certificate)?> GetVisibleAsync(Guid id)
    {
        var report = await _repository.GetEntityAsync(id);
        if (report is null) return null;
        var certificate = await _certificates.GetByIdAsync(report.CertificateId);
        if (certificate is null) return null;
        if (IsTechnician && certificate.TechnicianId != _tenant.EmployeeId) return null;
        return (report, certificate);
    }

    public async Task<ReportDetailResponseDto?> GetDetailAsync(Guid id)
    {
        var visible = await GetVisibleAsync(id);
        if (visible is null) return null;
        var (r, certificate) = visible.Value;

        var resolvedBy = r.ResolvedByEmployeeId is null ? null : await _repository.GetEmployeeNameAsync(r.ResolvedByEmployeeId.Value);
        return new ReportDetailResponseDto(r.Id, r.TrackingCode, certificate, r.Reason, r.Description,
            r.ReporterType, r.ReporterName, r.ReporterEmail, r.ReporterPhone, r.Status, r.Resolution, resolvedBy,
            await _repository.GetAttachmentsAsync(r.Id), r.CreatedAt, r.ResolvedAt);
    }

    // Descarga de una prueba: solo usuarios de la empresa duena del reporte (regla 11)
    public async Task<(Stream Content, string ContentType, string FileName)?> DownloadAsync(Guid id, Guid attachmentId)
    {
        if (await GetVisibleAsync(id) is null) return null;
        var attachment = await _repository.GetAttachmentAsync(id, attachmentId);
        if (attachment is null) return null;
        var stream = await _storage.OpenReadAsync(attachment.StorageKey);
        return stream is null ? null : (stream, attachment.ContentType, attachment.FileName);
    }

    // ---------- Empresa: Admin lo pasa a En revision ----------
    public async Task<ServiceResult<bool>> ReviewAsync(Guid id)
    {
        var report = await _repository.GetEntityAsync(id);
        if (report is null) return ServiceResult<bool>.Fail(404, "El reporte no existe.");
        if (report.Status != ReportStatus.Received)
            return ServiceResult<bool>.Fail(409, "Solo un reporte Recibido puede pasar a En revisión.");

        report.Status = ReportStatus.InReview;
        await _repository.UpdateAsync(report);
        return ServiceResult<bool>.Ok(true, 204);
    }

    // ---------- Empresa: Admin acepta o rechaza (regla 10) ----------
    public async Task<ServiceResult<bool>> ResolveAsync(Guid id, ResolveReportRequestDto request)
    {
        var employeeId = _tenant.EmployeeId ?? throw new InvalidOperationException("No hay EmployeeId en el token.");

        var report = await _repository.GetEntityAsync(id);
        if (report is null) return ServiceResult<bool>.Fail(404, "El reporte no existe.");
        if (report.Status is ReportStatus.Accepted or ReportStatus.Rejected)
            return ServiceResult<bool>.Fail(409, "El reporte ya fue resuelto.");

        var certificate = await _certificates.GetEntityAsync(report.CertificateId);
        if (certificate is null) return ServiceResult<bool>.Fail(404, "El certificado no existe.");
        if (certificate.TechnicianId == employeeId)
            return ServiceResult<bool>.Fail(403, "Quien emitió el certificado no puede resolver su reporte.");

        // Un reporte nunca cambia el certificado solo: se revoca SOLO si el Admin lo pide al aceptar
        if (request.Decision == ReportStatus.Accepted && request.RevokeCertificate
            && certificate.Status == CertificateStatus.Active)
        {
            certificate.Status = CertificateStatus.Revoked;
            certificate.RevokedAt = DateTime.UtcNow;
            certificate.RevocationReason = $"Reporte {report.TrackingCode} aceptado: {request.Resolution.Trim()}";
            if (certificate.RevocationReason.Length > 500) certificate.RevocationReason = certificate.RevocationReason[..500];
            await _certificates.UpdateAsync(certificate);
        }

        report.Status = request.Decision;
        report.Resolution = request.Resolution.Trim();
        report.ResolvedByEmployeeId = employeeId;
        report.ResolvedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(report);
        return ServiceResult<bool>.Ok(true, 204);
    }
}
