using Microsoft.EntityFrameworkCore;
using FixnSeven.Application.Features.Orders;
using FixnSeven.Application.Features.Orders.DTOs;
using FixnSeven.Domain.Entities;
using FixnSeven.Infrastructure.Data;
namespace FixnSeven.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly FixnSevenDbContext _context;
    public PaymentRepository(FixnSevenDbContext context) => _context = context;

    public async Task<decimal> GetOrderTotalAsync(Guid orderId) =>
        Math.Round(await _context.ServiceItems.Where(i => i.ServiceOrderId == orderId)
            .SumAsync(i => (decimal?)(i.Quantity * i.UnitPrice)) ?? 0m, 2);

    public async Task<decimal> GetOrderPaidAsync(Guid orderId) =>
        await _context.Payments.Where(p => p.ServiceOrderId == orderId)
            .SumAsync(p => (decimal?)p.Amount) ?? 0m;

    public async Task AddAsync(Payment payment)
    {
        await _context.Payments.AddAsync(payment);
        await _context.SaveChangesAsync();
    }

    // Pagos de la orden en orden cronologico; el saldo despues de cada pago se calcula en memoria
    public async Task<List<PaymentResponseDto>> GetByOrderAsync(Guid orderId)
    {
        var total = await GetOrderTotalAsync(orderId);
        var rows = await (
            from p in _context.Payments.AsNoTracking()
            join e in _context.Employees on p.ReceivedByEmployeeId equals e.Id
            where p.ServiceOrderId == orderId
            select new { Payment = p, ReceivedBy = e.FullName }).ToListAsync();

        var paid = 0m;
        return rows.OrderBy(r => r.Payment.PaidAt).Select(r =>
        {
            paid += r.Payment.Amount;
            var p = r.Payment;
            return new PaymentResponseDto(p.Id, p.ServiceOrderId, p.Amount, p.Method, p.Reference,
                r.ReceivedBy, p.PaidAt, total - paid);
        }).ToList();
    }

    public async Task<PaymentResponseDto?> GetByIdAsync(Guid id)
    {
        var orderId = await _context.Payments.Where(p => p.Id == id)
            .Select(p => (Guid?)p.ServiceOrderId).FirstOrDefaultAsync();
        if (orderId is null) return null;
        return (await GetByOrderAsync(orderId.Value)).FirstOrDefault(p => p.Id == id);
    }
}
