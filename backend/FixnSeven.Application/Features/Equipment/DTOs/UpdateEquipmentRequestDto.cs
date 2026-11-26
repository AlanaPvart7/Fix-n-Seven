namespace FixnSeven.Application.Features.Equipment.DTOs;

public record UpdateEquipmentRequestDto(
    Guid? CustomerId,
    Guid CategoryId,
    string Identifier,
    string Brand,
    string Model,
    int? CurrentUsage
);
