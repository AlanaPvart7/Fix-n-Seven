namespace FixnSeven.Application.Features.Orders.DTOs;

public record OrderSummaryResponseDto(
    Guid Id,
    string Status,
    string EquipmentIdentifier,
    string EquipmentDescription,   // marca + modelo
    string? CustomerName,
    string? TechnicianName,
    decimal Total,
    DateTime CreatedAt
);
