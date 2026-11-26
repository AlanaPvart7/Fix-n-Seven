using FixnSeven.Application.Features.Certificates.DTOs;
namespace FixnSeven.Application.Features.CustomerDevices.DTOs;

public record CustomerDeviceResponseDto(
    Guid Id,
    string? Alias,
    string EquipmentType,
    string Brand,
    string Model,
    PublicCertificateResponseDto? Certificate,   // el certificado se obtiene desde el equipo, no se guarda
    DateOnly? PurchaseDate,
    string? Accessories,
    string? PublicDescription,
    string? PrivateNotes,
    bool ShowInPublicView,
    List<CustomerDeviceFileResponseDto> Files,
    List<string>? Warnings
);
