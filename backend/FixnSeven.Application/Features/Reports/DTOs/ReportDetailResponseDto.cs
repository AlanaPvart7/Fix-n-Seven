using FixnSeven.Application.Features.Certificates.DTOs;
namespace FixnSeven.Application.Features.Reports.DTOs;

public record ReportDetailResponseDto(
    Guid Id,
    string TrackingCode,
    CertificateResponseDto Certificate,
    string Reason,
    string Description,
    string ReporterType,
    string ReporterName,
    string ReporterEmail,
    string? ReporterPhone,
    string Status,
    string? Resolution,
    string? ResolvedByName,
    List<AttachmentResponseDto> Attachments,
    DateTime CreatedAt,
    DateTime? ResolvedAt
);
