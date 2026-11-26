namespace FixnSeven.Application.Features.Income;

public record ClosedOrderRow(Guid? TechnicianId, string? TechnicianName, DateTime ClosedAt, decimal Total);
public record PaymentRow(DateTime PaidAt, decimal Amount);

public interface IIncomeRepository
{
    Task<List<ClosedOrderRow>> GetClosedOrdersAsync(DateTime fromUtc, DateTime toUtcExclusive);
    Task<List<PaymentRow>> GetPaymentsAsync(DateTime fromUtc, DateTime toUtcExclusive);
}
