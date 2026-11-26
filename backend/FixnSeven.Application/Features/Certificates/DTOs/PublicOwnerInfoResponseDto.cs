namespace FixnSeven.Application.Features.Certificates.DTOs;

// Fase 2: informacion que el dueno decide compartir desde su cuenta
public record PublicOwnerInfoResponseDto(
    DateOnly? PurchaseDate,
    string? Accessories,
    string? Description,
    List<string> PhotoUrls
);
