namespace FixnSeven.Application.Features.Equipment.DTOs;

public record UpdateEquipmentCategoryRequestDto(
    string Name,
    string IdentifierLabel,
    string UsageUnit     // Fechas u Horas
);
