using FluentValidation;
using FixnSeven.Application.Features.Reports.DTOs;
using FixnSeven.Domain.Entities;
namespace FixnSeven.Application.Features.Reports.Validators;

public class CreateReportValidator : AbstractValidator<CreateReportRequestDto>
{
    public CreateReportValidator()
    {
        RuleFor(x => x.Reason)
            .Must(r => ReportReasons.All.Contains(r))
            .WithMessage("El motivo debe ser EstadoDistinto, NivelIncorrecto, DatosErroneos, PosibleFalsificacion u Otro.");
        RuleFor(x => x.Description).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Describa qué está mal en el certificado.")
            .MinimumLength(20).WithMessage("La descripción debe tener al menos 20 caracteres.")
            .MaximumLength(2000);
        RuleFor(x => x.ReporterName)
            .NotEmpty().WithMessage("Su nombre es obligatorio.")
            .MaximumLength(120);
        RuleFor(x => x.ReporterEmail).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Su correo es obligatorio para avisarle el resultado.")
            .EmailAddress().WithMessage("El correo no es válido.")
            .MaximumLength(254);
        RuleFor(x => x.ReporterPhone).MaximumLength(30);
    }
}
