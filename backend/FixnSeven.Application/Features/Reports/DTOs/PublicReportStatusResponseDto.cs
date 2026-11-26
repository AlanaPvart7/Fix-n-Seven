namespace FixnSeven.Application.Features.Reports.DTOs;

// Consulta sin cuenta con el codigo de seguimiento: solo el estado y la explicacion de ESE reporte
public record PublicReportStatusResponseDto(
    string TrackingCode,
    string Status,
    string? Resolution,
    int AttachmentCount,
    DateTime CreatedAt,
    DateTime? ResolvedAt
);
