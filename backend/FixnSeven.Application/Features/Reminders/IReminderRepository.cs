using FixnSeven.Application.Features.Reminders.DTOs;
using FixnSeven.Domain.Entities;
namespace FixnSeven.Application.Features.Reminders;

public interface IReminderRepository
{
    Task<bool> EquipmentExistsAsync(Guid equipmentId);
    Task AddAsync(ServiceReminder reminder);
    Task<ServiceReminder?> GetEntityAsync(Guid id);
    Task DeleteAsync(ServiceReminder reminder);
    Task<ReminderResponseDto?> GetByIdAsync(Guid id, DateOnly today);
    Task<List<ReminderResponseDto>> GetUpcomingAsync(DateOnly today, DateOnly until, double usageThreshold);
}
