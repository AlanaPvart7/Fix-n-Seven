namespace FixnSeven.Application.Features.Equipment.DTOs;

public record EquipmentResponseDto(
    Guid Id,
    Guid? CustomerId,
    string? CustomerName,
    Guid CategoryId,
    string CategoryName,
    string IdentifierLabel,
    string Identifier,
    string Brand,
    string Model,
    int? CurrentUsage
);
