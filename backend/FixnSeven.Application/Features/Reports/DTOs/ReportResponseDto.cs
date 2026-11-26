namespace FixnSeven.Application.Features.Reports.DTOs;

// Lo que recibe quien reporta: guarde el TrackingCode para consultar el resultado
public record ReportResponseDto(
    Guid Id,
    string TrackingCode,
    string Status,
    DateTime CreatedAt
);
