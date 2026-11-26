namespace FixnSeven.Application.Features.Reminders.DTOs;

public record CreateReminderRequestDto(
    Guid EquipmentId,
    string Reason,
    DateOnly? DueDate,   // DueDate o DueUsage: al menos uno
    int? DueUsage        // horas de uso, para categorias que miden por horas
);
