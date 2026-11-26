namespace FixnSeven.Application.Features.Reports.DTOs;

public record ReportSummaryResponseDto(
    Guid Id,
    Guid CertificateId,
    string VerificationCode,
    string Reason,
    string ReporterType,
    string ReporterName,
    string Status,
    int AttachmentCount,
    DateTime CreatedAt
);
