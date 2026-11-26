using FixnSeven.Application.Common;
using FixnSeven.Application.Features.Reminders.DTOs;
using FixnSeven.Domain.Entities;
namespace FixnSeven.Application.Features.Reminders;

// HU-17: recordatorios de proximo servicio para llamar al cliente
public class ReminderService
{
    // Un recordatorio por horas aparece cuando el equipo llega al 90 % de las horas
    public const double UsageThreshold = 0.9;

    private readonly IReminderRepository _repository;
    public ReminderService(IReminderRepository repository) => _repository = repository;

    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    public async Task<ServiceResult<ReminderResponseDto>> CreateAsync(CreateReminderRequestDto request)
    {
        if (!await _repository.EquipmentExistsAsync(request.EquipmentId))
            return ServiceResult<ReminderResponseDto>.Fail(400, "El equipo no existe.");

        var reminder = new ServiceReminder
        {
            EquipmentId = request.EquipmentId,
            Reason = request.Reason.Trim(),
            DueDate = request.DueDate,
            DueUsage = request.DueUsage
        };
        await _repository.AddAsync(reminder);

        var dto = await _repository.GetByIdAsync(reminder.Id, Today);
        return ServiceResult<ReminderResponseDto>.Ok(dto!, 201);
    }

    public Task<ReminderResponseDto?> GetByIdAsync(Guid id) => _repository.GetByIdAsync(id, Today);

    // Vencidos + los que vencen en los proximos "days" dias (por fecha o por horas de uso)
    public Task<List<ReminderResponseDto>> GetUpcomingAsync(int days)
    {
        days = Math.Clamp(days, 1, 365);
        return _repository.GetUpcomingAsync(Today, Today.AddDays(days), UsageThreshold);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var reminder = await _repository.GetEntityAsync(id);
        if (reminder is null) return false;
        await _repository.DeleteAsync(reminder);
        return true;
    }
}
