using Microsoft.EntityFrameworkCore;
using FixnSeven.Application.Features.Equipment;
using FixnSeven.Application.Features.Orders;
using FixnSeven.Application.Features.Orders.DTOs;
using FixnSeven.Domain.Common;
using FixnSeven.Domain.Entities;
using FixnSeven.Infrastructure.Data;
namespace FixnSeven.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly FixnSevenDbContext _context;
    private readonly IEquipmentRepository _equipment;

    public OrderRepository(FixnSevenDbContext context, IEquipmentRepository equipment)
    {
        _context = context;
        _equipment = equipment;
    }

    public async Task<(bool Exists, Guid? CustomerId)> GetEquipmentOwnerAsync(Guid equipmentId)
    {
        var row = await _context.Equipment.Where(e => e.Id == equipmentId)
            .Select(e => new { e.CustomerId }).FirstOrDefaultAsync();
        return row is null ? (false, null) : (true, row.CustomerId);
    }

    public Task<bool> TechnicianExistsAsync(Guid employeeId) =>
        _context.Employees.AnyAsync(e => e.Id == employeeId && e.RoleEmployeeId == SeedIds.RoleTechnician);

    public Task<int> CountOrdersSinceAsync(DateTime sinceUtc) =>
        _context.ServiceOrders.CountAsync(o => o.CreatedAt >= sinceUtc);

    public Task<int?> GetPlanMaxMonthlyOrdersAsync(Guid enterpriseId) =>
        (from e in _context.Enterprises
         join p in _context.Plans on e.PlanId equals p.Id
         where e.Id == enterpriseId
         select p.MaxMonthlyOrders).FirstOrDefaultAsync();

    public async Task<string> GetEnterpriseNameAsync(Guid enterpriseId) =>
        await _context.Enterprises.Where(e => e.Id == enterpriseId)
            .Select(e => e.Name).FirstOrDefaultAsync() ?? string.Empty;

    public async Task AddAsync(ServiceOrder order)
    {
        await _context.ServiceOrders.AddAsync(order);
        await _context.SaveChangesAsync();
    }

    public Task<ServiceOrder?> GetEntityAsync(Guid id) =>
        _context.ServiceOrders.FirstOrDefaultAsync(o => o.Id == id);

    public async Task UpdateAsync(ServiceOrder order)
    {
        _context.ServiceOrders.Update(order);
        await _context.SaveChangesAsync();
    }

    public async Task AddItemAsync(ServiceItem item)
    {
        await _context.ServiceItems.AddAsync(item);
        await _context.SaveChangesAsync();
    }

    // Resumen: orden + equipo + cliente + tecnico + total calculado (sin guardar el total)
    private IQueryable<OrderSummaryResponseDto> Summary(IQueryable<ServiceOrder> source) =>
        from o in source
        join q in _context.Equipment on o.EquipmentId equals q.Id
        join cu in _context.Customers on o.CustomerId equals (Guid?)cu.Id into customers
        from cu in customers.DefaultIfEmpty()
        join t in _context.Employees on o.TechnicianId equals (Guid?)t.Id into techs
        from t in techs.DefaultIfEmpty()
        select new OrderSummaryResponseDto(
            o.Id, o.Status, q.Identifier, q.Brand + " " + q.Model,
            cu != null ? cu.FullName : null,
            t != null ? t.FullName : null,
            Math.Round(_context.ServiceItems.Where(i => i.ServiceOrderId == o.Id).Sum(i => (decimal?)(i.Quantity * i.UnitPrice)) ?? 0m, 2),
            o.CreatedAt);

    public async Task<(List<OrderSummaryResponseDto> Items, int Total)> GetPagedAsync(
        string? status, Guid? technicianId, DateTime? fromUtc, DateTime? toUtc, int page, int pageSize)
    {
        var source = _context.ServiceOrders.AsQueryable();
        if (!string.IsNullOrWhiteSpace(status)) source = source.Where(o => o.Status == status);
        if (technicianId is not null) source = source.Where(o => o.TechnicianId == technicianId);
        if (fromUtc is not null) source = source.Where(o => o.CreatedAt >= fromUtc);
        if (toUtc is not null) source = source.Where(o => o.CreatedAt <= toUtc);

        // Contar, ordenar y paginar sobre la ENTIDAD; despues armar el DTO
        var total = await source.CountAsync();
        var pageSource = source.OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize);

        var items = (await Summary(pageSource).ToListAsync())
            .OrderByDescending(x => x.CreatedAt).ToList();
        return (items, total);
    }

    public Task<OrderSummaryResponseDto?> GetSummaryAsync(Guid id) =>
        Summary(_context.ServiceOrders.Where(o => o.Id == id)).FirstOrDefaultAsync();

    public async Task<List<OrderSummaryResponseDto>> GetByEquipmentAsync(Guid equipmentId) =>
        (await Summary(_context.ServiceOrders.Where(o => o.EquipmentId == equipmentId)).ToListAsync())
            .OrderByDescending(x => x.CreatedAt).ToList();

    public async Task<OrderDetailResponseDto?> GetDetailAsync(Guid id)
    {
        var order = await _context.ServiceOrders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id);
        if (order is null) return null;

        var equipment = await _equipment.GetByIdAsync(order.EquipmentId);
        var customerName = order.CustomerId is null ? null : await _context.Customers
            .Where(c => c.Id == order.CustomerId).Select(c => c.FullName).FirstOrDefaultAsync();
        var technicianName = order.TechnicianId is null ? null : await _context.Employees
            .Where(e => e.Id == order.TechnicianId).Select(e => e.FullName).FirstOrDefaultAsync();

        var items = await _context.ServiceItems
            .Where(i => i.ServiceOrderId == id)
            .OrderBy(i => i.Kind).ThenBy(i => i.Description)
            .Select(i => new { i.Id, i.Kind, i.Description, i.Quantity, i.UnitPrice })
            .ToListAsync();
        var itemDtos = items.Select(i => new ServiceItemResponseDto(
            i.Id, i.Kind, i.Description, i.Quantity, i.UnitPrice, i.Quantity * i.UnitPrice)).ToList();

        return new OrderDetailResponseDto(
            order.Id, order.Status, equipment!, customerName, order.TechnicianId, technicianName,
            order.ReportedIssue, order.ReceivedCondition, order.WorkDone, order.Notes,
            itemDtos, itemDtos.Sum(i => i.Subtotal), order.CreatedAt, order.ClosedAt);
    }
}
