using FixnSeven.Domain.Common;
namespace FixnSeven.Domain.Entities;

public static class PaymentMethods
{
    public const string Cash = "Efectivo";
    public const string Card = "Tarjeta";
    public const string Transfer = "Transferencia";
    public static readonly string[] All = { Cash, Card, Transfer };
}

// HU-13: cobro de una orden. Solo se REGISTRA, no hay pasarela de pago.
// El saldo no se guarda: total de la orden - suma de pagos (3FN)
public class Payment : TenantEntity
{
    public Guid ServiceOrderId { get; set; }
    public Guid ReceivedByEmployeeId { get; set; }   // sale del JWT
    public decimal Amount { get; set; }
    public string Method { get; set; } = PaymentMethods.Cash;
    public string? Reference { get; set; }           // numero de transferencia o voucher
    public DateTime PaidAt { get; set; } = DateTime.UtcNow;
}
