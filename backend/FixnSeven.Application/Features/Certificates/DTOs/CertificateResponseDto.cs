namespace FixnSeven.Application.Features.Certificates.DTOs;

public record CertificateResponseDto(
    Guid Id,
    Guid EquipmentId,
    Guid? ServiceOrderId,
    string Grade,
    int Score,
    string Status,            // Vigente, Vencido o Revocado (Vencido se calcula)
    string VerificationCode,
    Guid TechnicianId,
    string TechnicianName,
    DateTime IssuedAt,
    DateTime ExpiresAt,
    DateTime? RevokedAt,
    string? RevocationReason,
    string? Observations,
    string? PublicSummary,
    string? OwnerCode,        // codigo privado del dueno: SOLO en la respuesta de POST; despues null
    List<string>? Warnings
);
