using FluentValidation;
using FixnSeven.Application.Features.Settings.DTOs;
using FixnSeven.Domain.Entities;
namespace FixnSeven.Application.Features.Settings.Validators;

public class UpdateCertificateSettingsValidator : AbstractValidator<UpdateCertificateSettingsRequestDto>
{
    // De menor a mayor: Reprobado < Regular < Bueno < Excelente
    private static readonly string[] Order =
        { CertificateGrades.Failed, CertificateGrades.Fair, CertificateGrades.Good, CertificateGrades.Excellent };

    public UpdateCertificateSettingsValidator()
    {
        RuleFor(x => x.ValidityDays)
            .InclusiveBetween(1, 365).WithMessage("La vigencia debe ser de 1 a 365 días.");

        RuleFor(x => x.GradeScale)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Envíe la escala de calidad.")
            .Must(s => s.Count == 4 && Order.All(g => s.Count(i => i.Grade == g) == 1))
            .WithMessage("La escala debe tener los 4 niveles, una vez cada uno: Excelente, Bueno, Regular y Reprobado.")
            .Must(CoversZeroToHundred)
            .WithMessage("Los rangos deben ir de 0 a 100, sin huecos ni traslapes, y en orden: Reprobado < Regular < Bueno < Excelente.");
    }

    private static bool CoversZeroToHundred(List<GradeScaleItemDto> scale)
    {
        var ordered = Order.Select(g => scale.First(i => i.Grade == g)).ToList();
        if (ordered[0].MinScore != 0 || ordered[^1].MaxScore != 100) return false;
        for (var i = 0; i < ordered.Count; i++)
        {
            if (ordered[i].MinScore > ordered[i].MaxScore) return false;
            if (i > 0 && ordered[i].MinScore != ordered[i - 1].MaxScore + 1) return false;
        }
        return true;
    }
}
