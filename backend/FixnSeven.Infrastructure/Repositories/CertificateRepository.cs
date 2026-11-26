using Microsoft.EntityFrameworkCore;
using FixnSeven.Application.Features.Certificates;
using FixnSeven.Application.Features.Certificates.DTOs;
using FixnSeven.Domain.Entities;
using FixnSeven.Infrastructure.Data;
namespace FixnSeven.Infrastructure.Repositories;

public class CertificateRepository : ICertificateRepository
{
    private readonly FixnSevenDbContext _context;
    public CertificateRepository(FixnSevenDbContext context) => _context = context;

    public Task<bool> EquipmentExistsAsync(Guid equipmentId) =>
        _context.Equipment.AnyAsync(e => e.Id == equipmentId);

    // Unico en todo el sistema: se ignora el filtro por empresa
    public Task<bool> VerificationCodeExistsAsync(string code) =>
        _context.Certificates.IgnoreQueryFilters().AnyAsync(c => c.VerificationCode == code);

    public Task<int> GetValidityDaysAsync(Guid enterpriseId) =>
        _context.Enterprises.Where(e => e.Id == enterpriseId)
            .Select(e => e.CertificateValidityDays).FirstAsync();

    public async Task<string> GetEmployeeNameAsync(Guid employeeId) =>
        await _context.Employees.Where(e => e.Id == employeeId)
            .Select(e => e.FullName).FirstOrDefaultAsync() ?? string.Empty;

    public Task<Certificate?> GetEntityAsync(Guid id) =>
        _context.Certificates.FirstOrDefaultAsync(c => c.Id == id);

    public Task<Certificate?> GetActiveEntityByEquipmentAsync(Guid equipmentId) =>
        _context.Certificates.FirstOrDefaultAsync(c =>
            c.EquipmentId == equipmentId && c.Status == CertificateStatus.Active);

    // Si hay uno vencido, se cierra ANTES de insertar el nuevo (indice unico filtrado)
    public async Task IssueAsync(Certificate certificate, Certificate? expiredToClose)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        if (expiredToClose is not null)
        {
            _context.Certificates.Update(expiredToClose);
            await _context.SaveChangesAsync();
        }
        await _context.Certificates.AddAsync(certificate);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task UpdateAsync(Certificate certificate)
    {
        _context.Certificates.Update(certificate);
        await _context.SaveChangesAsync();
    }

    // Certificado + nombre del tecnico; el estado (Vencido) se calcula en memoria
    private async Task<List<CertificateResponseDto>> ListAsync(IQueryable<Certificate> source)
    {
        var rows = await (
            from c in source.AsNoTracking()
            join t in _context.Employees on c.TechnicianId equals t.Id
            select new { Certificate = c, TechnicianName = t.FullName }).ToListAsync();

        return rows
            .OrderByDescending(r => r.Certificate.IssuedAt)
            .Select(r => CertificateMapper.ToDto(r.Certificate, r.TechnicianName))
            .ToList();
    }

    public async Task<CertificateResponseDto?> GetByIdAsync(Guid id) =>
        (await ListAsync(_context.Certificates.Where(c => c.Id == id))).FirstOrDefault();

    public Task<List<CertificateResponseDto>> GetByEquipmentAsync(Guid equipmentId) =>
        ListAsync(_context.Certificates.Where(c => c.EquipmentId == equipmentId));

    public async Task<CertificateResponseDto?> GetActiveByEquipmentAsync(Guid equipmentId) =>
        (await ListAsync(_context.Certificates.Where(c =>
            c.EquipmentId == equipmentId && c.Status == CertificateStatus.Active))).FirstOrDefault();

    public async Task<CertificateResponseDto?> GetLatestByOrderAsync(Guid orderId) =>
        (await ListAsync(_context.Certificates.Where(c => c.ServiceOrderId == orderId))).FirstOrDefault();

    // HU-16: sin token. Se ignoran los filtros por empresa y SOLO se seleccionan datos publicos
    public async Task<PublicCertificateResponseDto?> GetPublicAsync(string verificationCode)
    {
        var row = await (
            from c in _context.Certificates.IgnoreQueryFilters()
            join q in _context.Equipment.IgnoreQueryFilters() on c.EquipmentId equals q.Id
            join cat in _context.EquipmentCategories.IgnoreQueryFilters() on q.CategoryId equals cat.Id
            join t in _context.Employees.IgnoreQueryFilters() on c.TechnicianId equals t.Id
            join en in _context.Enterprises on c.EnterpriseId equals en.Id
            where c.VerificationCode == verificationCode
            select new
            {
                c.VerificationCode, c.Grade, c.Score, c.Status, c.IssuedAt, c.ExpiresAt, c.RevokedAt, c.PublicSummary,
                WorkshopName = en.Name, TechnicianName = t.FullName,
                EquipmentType = cat.Name, q.Brand, q.Model, q.Identifier, EquipmentId = q.Id
            }).AsNoTracking().FirstOrDefaultAsync();

        if (row is null) return null;

        var status = row.Status == CertificateStatus.Revoked ? CertificateStatus.Revoked
            : row.ExpiresAt < DateTime.UtcNow ? CertificateStatus.Expired
            : CertificateStatus.Active;

        // Informacion que el dueno decidio compartir (Fase 2). Nunca su nombre ni contacto.
        PublicOwnerInfoResponseDto? ownerInfo = null;
        var owner = await _context.CustomerDevices.AsNoTracking()
            .FirstOrDefaultAsync(d => d.EquipmentId == row.EquipmentId && d.Status == DeviceStatus.Active && d.ShowInPublicView);
        if (owner is not null)
        {
            var photoIds = await _context.CustomerDeviceFiles
                .Where(f => f.CustomerDeviceId == owner.Id && f.IsPublic && f.FileKind == FileKinds.Photo)
                .OrderBy(f => f.UploadedAt).Select(f => f.Id).ToListAsync();
            ownerInfo = new PublicOwnerInfoResponseDto(owner.PurchaseDate, owner.Accessories, owner.PublicDescription,
                photoIds.Select(id => $"/api/public/device-files/{id}").ToList());
        }

        return new PublicCertificateResponseDto(
            row.VerificationCode, row.WorkshopName, row.TechnicianName, row.EquipmentType,
            row.Brand, row.Model, CertificateMapper.Mask(row.Identifier),
            row.Grade, row.Score, status, row.IssuedAt, row.ExpiresAt, row.RevokedAt,
            row.PublicSummary, ownerInfo, CertificateMapper.Disclaimer);
    }
}
