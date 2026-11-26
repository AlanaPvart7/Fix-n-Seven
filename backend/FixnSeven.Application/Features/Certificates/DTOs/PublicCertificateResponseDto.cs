namespace FixnSeven.Application.Features.Certificates.DTOs;

// Consulta publica por QR: NUNCA lleva dueno, cliente, contacto, notas internas ni el identificador completo
public record PublicCertificateResponseDto(
    string VerificationCode,
    string WorkshopName,
    string TechnicianName,
    string EquipmentType,
    string Brand,
    string Model,
    string MaskedIdentifier,  // solo los ultimos 4 caracteres
    string Grade,
    int Score,
    string Status,            // Vigente, Vencido o Revocado
    DateTime IssuedAt,
    DateTime ExpiresAt,
    DateTime? RevokedAt,
    string? PublicSummary,
    PublicOwnerInfoResponseDto? OwnerInfo,   // Fase 2 (cuenta del comprador)
    string Disclaimer
);
