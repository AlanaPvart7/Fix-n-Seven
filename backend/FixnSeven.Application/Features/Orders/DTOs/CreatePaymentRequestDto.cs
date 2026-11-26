namespace FixnSeven.Application.Features.Orders.DTOs;

public record CreatePaymentRequestDto(
    decimal Amount,      // mayor que 0 y no mayor que el saldo pendiente
    string Method,       // Efectivo, Tarjeta o Transferencia
    string? Reference    // numero de transferencia o voucher
);
