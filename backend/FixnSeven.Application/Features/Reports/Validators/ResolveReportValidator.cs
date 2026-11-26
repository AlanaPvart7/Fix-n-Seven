using FluentValidation;
using FixnSeven.Application.Features.Reports.DTOs;
using FixnSeven.Domain.Entities;
namespace FixnSeven.Application.Features.Reports.Validators;

public class ResolveReportValidator : AbstractValidator<ResolveReportRequestDto>
{
    public ResolveReportValidator()
    {
        RuleFor(x => x.Decision)
            .Must(d => d == ReportStatus.Accepted || d == ReportStatus.Rejected)
            .WithMessage("La decisión debe ser Aceptado o Rechazado.");
        RuleFor(x => x.Resolution).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Explique la decisión.")
            .MinimumLength(10).WithMessage("La explicación debe tener al menos 10 caracteres.")
            .MaximumLength(2000);
        RuleFor(x => x.RevokeCertificate)
            .Equal(false).When(x => x.Decision == ReportStatus.Rejected)
            .WithMessage("Si rechaza el reporte, no puede revocar el certificado.");
    }
}
