using FluentValidation;
using FixnSeven.Application.Features.Reminders.DTOs;
namespace FixnSeven.Application.Features.Reminders.Validators;

public class CreateReminderValidator : AbstractValidator<CreateReminderRequestDto>
{
    public CreateReminderValidator()
    {
        RuleFor(x => x.EquipmentId).NotEmpty().WithMessage("Indique el equipo.");
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Indique el motivo del recordatorio.")
            .MaximumLength(200);

        RuleFor(x => x)
            .Must(x => x.DueDate is not null || x.DueUsage is not null)
            .WithName("DueDate")
            .WithMessage("Indique una fecha (DueDate) o unas horas de uso (DueUsage).");

        // Margen de 1 dia: en Honduras (UTC-6) despues de las 6 p. m. en UTC ya es "manana"
        RuleFor(x => x.DueDate)
            .Must(d => d >= DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1))
            .When(x => x.DueDate is not null)
            .WithMessage("La fecha del recordatorio no puede estar en el pasado.");

        RuleFor(x => x.DueUsage)
            .GreaterThan(0).When(x => x.DueUsage is not null)
            .WithMessage("Las horas de uso deben ser mayores que 0.");
    }
}
