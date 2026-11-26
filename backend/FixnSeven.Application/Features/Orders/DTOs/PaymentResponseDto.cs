namespace FixnSeven.Application.Features.Orders.DTOs;

public record PaymentResponseDto(
    Guid Id,
    Guid ServiceOrderId,
    decimal Amount,
    string Method,
    string? Reference,
    string ReceivedByName,
    DateTime PaidAt,
    decimal BalanceAfter   // saldo de la orden despues de este pago (se calcula)
);
