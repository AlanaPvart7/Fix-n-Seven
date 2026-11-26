using Microsoft.EntityFrameworkCore;
using FixnSeven.Application.Features.Reminders;
using FixnSeven.Application.Features.Reminders.DTOs;
using FixnSeven.Domain.Entities;
using FixnSeven.Infrastructure.Data;
namespace FixnSeven.Infrastructure.Repositories;

public class ReminderRepository : IReminderRepository
{
    private readonly FixnSevenDbContext _context;
    public ReminderRepository(FixnSevenDbContext context) => _context = context;

    public Task<bool> EquipmentExistsAsync(Guid equipmentId) =>
        _context.Equipment.AnyAsync(e => e.Id == equipmentId);

    public async Task AddAsync(ServiceReminder reminder)
    {
        await _context.ServiceReminders.AddAsync(reminder);
        await _context.SaveChangesAsync();
    }

    public Task<ServiceReminder?> GetEntityAsync(Guid id) =>
        _context.ServiceReminders.FirstOrDefaultAsync(r => r.Id == id);

    public async Task DeleteAsync(ServiceReminder reminder)
    {
        _context.ServiceReminders.Remove(reminder);
        await _context.SaveChangesAsync();
    }

    // Recordatorio + equipo + cliente (a quien llamar). Dias restantes y vencido se calculan en memoria.
    private async Task<List<ReminderResponseDto>> ListAsync(IQueryable<ServiceReminder> source, DateOnly today)
    {
        var rows = await (
            from r in source.AsNoTracking()
            join q in _context.Equipment on r.EquipmentId equals q.Id
            join cu in _context.Customers on q.CustomerId equals (Guid?)cu.Id into customers
            from cu in customers.DefaultIfEmpty()
            select new
            {
                r.Id, r.EquipmentId, Description = q.Brand + " " + q.Model, q.Identifier,
                CustomerName = cu != null ? cu.FullName : null,
                CustomerPhone = cu != null ? cu.Phone : null,
                r.Reason, r.DueDate, r.DueUsage, q.CurrentUsage
            }).ToListAsync();

        return rows
            .Select(x =>
            {
                int? daysLeft = x.DueDate is null ? null : x.DueDate.Value.DayNumber - today.DayNumber;
                var overdue = daysLeft < 0 || (x.DueUsage is not null && x.CurrentUsage >= x.DueUsage);
                return new ReminderResponseDto(x.Id, x.EquipmentId, x.Description, x.Identifier,
                    x.CustomerName, x.CustomerPhone, x.Reason, x.DueDate, x.DueUsage, x.CurrentUsage,
                    daysLeft, overdue);
            })
            .OrderByDescending(x => x.IsOverdue)
            .ThenBy(x => x.DaysLeft ?? int.MaxValue)
            .ToList();
    }

    public async Task<ReminderResponseDto?> GetByIdAsync(Guid id, DateOnly today) =>
        (await ListAsync(_context.ServiceReminders.Where(r => r.Id == id), today)).FirstOrDefault();

    public Task<List<ReminderResponseDto>> GetUpcomingAsync(DateOnly today, DateOnly until, double usageThreshold) =>
        ListAsync(
            from r in _context.ServiceReminders
            join q in _context.Equipment on r.EquipmentId equals q.Id
            where (r.DueDate != null && r.DueDate <= until)
               || (r.DueUsage != null && q.CurrentUsage != null && q.CurrentUsage >= r.DueUsage * usageThreshold)
            select r,
            today);
}
