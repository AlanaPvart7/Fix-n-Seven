using Microsoft.EntityFrameworkCore;
using FixnSeven.Application.Features.Reports;
using FixnSeven.Application.Features.Reports.DTOs;
using FixnSeven.Domain.Entities;
using FixnSeven.Infrastructure.Data;
namespace FixnSeven.Infrastructure.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly FixnSevenDbContext _context;
    public ReportRepository(FixnSevenDbContext context) => _context = context;

    // ---- Publico: quien reporta no tiene token de esa empresa, por eso IgnoreQueryFilters ----
    public Task<Certificate?> GetCertificateByCodeAsync(string verificationCode) =>
        _context.Certificates.IgnoreQueryFilters().AsNoTracking()
            .FirstOrDefaultAsync(c => c.VerificationCode == verificationCode);

    public Task<int> CountByEmailSinceAsync(string email, DateTime sinceUtc) =>
        _context.Reports.IgnoreQueryFilters().CountAsync(r => r.ReporterEmail == email && r.CreatedAt >= sinceUtc);

    public Task<bool> TrackingCodeExistsAsync(string trackingCode) =>
        _context.Reports.IgnoreQueryFilters().AnyAsync(r => r.TrackingCode == trackingCode);

    public Task<Report?> GetByTrackingCodeAsync(string trackingCode) =>
        _context.Reports.IgnoreQueryFilters().AsNoTracking().FirstOrDefaultAsync(r => r.TrackingCode == trackingCode);

    public Task<int> CountAttachmentsAsync(Guid reportId) =>
        _context.ReportAttachments.IgnoreQueryFilters().CountAsync(a => a.ReportId == reportId);

    public async Task AddAsync(Report report)
    {
        await _context.Reports.AddAsync(report);
        await _context.SaveChangesAsync();
    }

    public async Task AddAttachmentAsync(ReportAttachment attachment)
    {
        await _context.ReportAttachments.AddAsync(attachment);
        await _context.SaveChangesAsync();
    }

    // ---- Privado: el filtro global deja ver solo los reportes de MI empresa ----
    public Task<Report?> GetEntityAsync(Guid id) =>
        _context.Reports.FirstOrDefaultAsync(r => r.Id == id);

    public async Task UpdateAsync(Report report)
    {
        _context.Reports.Update(report);
        await _context.SaveChangesAsync();
    }

    public async Task<(List<ReportSummaryResponseDto> Items, int Total)> GetPagedAsync(
        string? status, Guid? certificateId, int page, int pageSize)
    {
        var source = _context.Reports.AsQueryable();
        if (!string.IsNullOrWhiteSpace(status)) source = source.Where(r => r.Status == status);
        if (certificateId is not null) source = source.Where(r => r.CertificateId == certificateId);

        var total = await source.CountAsync();
        var pageSource = source.OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize);

        var items = (await (
                from r in pageSource
                join c in _context.Certificates on r.CertificateId equals c.Id
                select new ReportSummaryResponseDto(
                    r.Id, r.CertificateId, c.VerificationCode, r.Reason, r.ReporterType, r.ReporterName, r.Status,
                    _context.ReportAttachments.Count(a => a.ReportId == r.Id), r.CreatedAt))
            .ToListAsync())
            .OrderByDescending(x => x.CreatedAt).ToList();
        return (items, total);
    }

    public Task<List<AttachmentResponseDto>> GetAttachmentsAsync(Guid reportId) =>
        _context.ReportAttachments.Where(a => a.ReportId == reportId)
            .OrderBy(a => a.UploadedAt)
            .Select(a => new AttachmentResponseDto(a.Id, a.FileName, a.FileKind, a.ContentType, a.SizeBytes, a.UploadedAt))
            .ToListAsync();

    public Task<ReportAttachment?> GetAttachmentAsync(Guid reportId, Guid attachmentId) =>
        _context.ReportAttachments.AsNoTracking()
            .FirstOrDefaultAsync(a => a.ReportId == reportId && a.Id == attachmentId);

    public Task<string?> GetEmployeeNameAsync(Guid employeeId) =>
        _context.Employees.Where(e => e.Id == employeeId).Select(e => (string?)e.FullName).FirstOrDefaultAsync();
}
