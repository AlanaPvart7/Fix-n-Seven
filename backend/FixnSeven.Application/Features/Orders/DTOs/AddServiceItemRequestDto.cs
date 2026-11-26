namespace FixnSeven.Application.Features.Orders.DTOs;

public record AddServiceItemRequestDto(
    string Kind,          // Trabajo o Repuesto
    string Description,
    int Quantity,
    decimal UnitPrice
);
