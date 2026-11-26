namespace FixnSeven.Application.Features.Reminders.DTOs;

public record ReminderResponseDto(
    Guid Id,
    Guid EquipmentId,
    string EquipmentDescription,   // "Samsung Galaxy A54 5G"
    string Identifier,
    string? CustomerName,          // a quien llamar (null si el equipo es propio)
    string? CustomerPhone,
    string Reason,
    DateOnly? DueDate,
    int? DueUsage,
    int? CurrentUsage,
    int? DaysLeft,                 // negativo = ya vencio (se calcula, no se guarda)
    bool IsOverdue
);
