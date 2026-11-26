using Microsoft.EntityFrameworkCore;
using FixnSeven.Application.Features.Income;
using FixnSeven.Domain.Entities;
using FixnSeven.Infrastructure.Data;
namespace FixnSeven.Infrastructure.Repositories;

public class IncomeRepository : IIncomeRepository
{
    private readonly FixnSevenDbContext _context;
    public IncomeRepository(FixnSevenDbContext context) => _context = context;

    // Ordenes cerradas en el rango, con su total calculado (filtradas por empresa automaticamente)
    public async Task<List<ClosedOrderRow>> GetClosedOrdersAsync(DateTime fromUtc, DateTime toUtcExclusive)
    {
        var rows = await (
            from o in _context.ServiceOrders
            join t in _context.Employees on o.TechnicianId equals (Guid?)t.Id into techs
            from t in techs.DefaultIfEmpty()
            where o.Status == OrderStatus.Closed && o.ClosedAt >= fromUtc && o.ClosedAt < toUtcExclusive
            select new
            {
                o.TechnicianId,
                TechnicianName = t != null ? t.FullName : null,
                ClosedAt = o.ClosedAt!.Value,
                Total = _context.ServiceItems.Where(i => i.ServiceOrderId == o.Id)
                    .Sum(i => (decimal?)(i.Quantity * i.UnitPrice)) ?? 0m
            }).ToListAsync();

        return rows.Select(r => new ClosedOrderRow(r.TechnicianId, r.TechnicianName, r.ClosedAt, Math.Round(r.Total, 2)))
            .ToList();
    }

    public async Task<List<PaymentRow>> GetPaymentsAsync(DateTime fromUtc, DateTime toUtcExclusive) =>
        (await _context.Payments
            .Where(p => p.PaidAt >= fromUtc && p.PaidAt < toUtcExclusive)
            .Select(p => new { p.PaidAt, p.Amount })
            .ToListAsync())
        .Select(p => new PaymentRow(p.PaidAt, p.Amount)).ToList();
}
