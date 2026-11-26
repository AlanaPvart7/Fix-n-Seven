namespace FixnSeven.Application.Features.Certificates.DTOs;

public record IssueCertificateRequestDto(
    Guid EquipmentId,
    Guid? ServiceOrderId,   // obligatorio para el tecnico: solo certifica equipos de SUS ordenes
    string Grade,           // Excelente, Bueno, Regular o Reprobado
    int Score,              // 0 a 100, dentro del rango del nivel
    string? Observations,   // notas internas
    string? PublicSummary   // lo que vera el comprador en el QR
);
