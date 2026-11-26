namespace FixnSeven.Application.Features.Equipment.DTOs;

public record CreateEquipmentCategoryRequestDto(
    string Name,             // Celular, Laptop...
    string IdentifierLabel,  // IMEI, N° de serie...
    string UsageUnit         // Fechas u Horas
);
