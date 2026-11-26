namespace FixnSeven.Application.Features.Equipment.DTOs;

public record CreateEquipmentRequestDto(
    Guid? CustomerId,    // vacio si el equipo es propio de la empresa
    Guid CategoryId,     // rubro (EquipmentCategory)
    string Identifier,   // IMEI, numero de serie o codigo de activo
    string Brand,
    string Model,
    int? CurrentUsage    // opcional
);
