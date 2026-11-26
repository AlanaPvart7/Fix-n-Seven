using FixnSeven.Application.Features.Reports.DTOs;
using FixnSeven.Domain.Entities;
namespace FixnSeven.Application.Features.Reports;

public interface IReportRepository
{
    // ---- Publico (sin token): ignoran el filtro por empresa ----
    Task<Certificate?> GetCertificateByCodeAsync(string verificationCode);
    Task<int> CountByEmailSinceAsync(string email, DateTime sinceUtc);
    Task<bool> TrackingCodeExistsAsync(string trackingCode);
    Task<Report?> GetByTrackingCodeAsync(string trackingCode);
    Task<int> CountAttachmentsAsync(Guid reportId);
    Task AddAsync(Report report);
    Task AddAttachmentAsync(ReportAttachment attachment);

    // ---- Privado (empresa del token) ----
    Task<Report?> GetEntityAsync(Guid id);
    Task UpdateAsync(Report report);
    Task<(List<ReportSummaryResponseDto> Items, int Total)> GetPagedAsync(
        string? status, Guid? certificateId, int page, int pageSize);
    Task<List<AttachmentResponseDto>> GetAttachmentsAsync(Guid reportId);
    Task<ReportAttachment?> GetAttachmentAsync(Guid reportId, Guid attachmentId);
    Task<string?> GetEmployeeNameAsync(Guid employeeId);
}
