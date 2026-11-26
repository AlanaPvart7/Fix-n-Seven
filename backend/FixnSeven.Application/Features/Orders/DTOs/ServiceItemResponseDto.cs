namespace FixnSeven.Application.Features.Orders.DTOs;

public record ServiceItemResponseDto(
    Guid Id,
    string Kind,
    string Description,
    int Quantity,
    decimal UnitPrice,
    decimal Subtotal     // Quantity * UnitPrice, calculado
);
