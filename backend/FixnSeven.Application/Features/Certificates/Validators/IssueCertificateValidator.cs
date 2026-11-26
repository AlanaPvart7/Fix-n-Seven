using FluentValidation;
using FixnSeven.Application.Features.Certificates.DTOs;
using FixnSeven.Domain.Entities;
namespace FixnSeven.Application.Features.Certificates.Validators;

public class IssueCertificateValidator : AbstractValidator<IssueCertificateRequestDto>
{
    public IssueCertificateValidator()
    {
        RuleFor(x => x.EquipmentId).NotEmpty().WithMessage("Indique el equipo.");
        RuleFor(x => x.ServiceOrderId)
            .Must(id => id != Guid.Empty).When(x => x.ServiceOrderId is not null)
            .WithMessage("La orden no es valida.");

        RuleFor(x => x.Grade)
            .Must(CertificateGrades.IsValid)
            .WithMessage("El nivel debe ser Excelente, Bueno, Regular o Reprobado.");

        RuleFor(x => x.Score)
            .InclusiveBetween(0, 100).WithMessage("El puntaje va de 0 a 100.");

        // El puntaje tiene que caer en el rango del nivel (Bueno = 75 a 89, etc.)
        RuleFor(x => x.Score)
            .Must((dto, score) => CertificateGrades.Fits(dto.Grade, score))
            .When(x => CertificateGrades.IsValid(x.Grade) && x.Score is >= 0 and <= 100)
            .WithMessage(dto =>
            {
                var (min, max) = CertificateGrades.Range(dto.Grade);
                return $"El nivel {dto.Grade} exige un puntaje de {min} a {max}.";
            });

        RuleFor(x => x.Observations).MaximumLength(2000);
        RuleFor(x => x.PublicSummary).MaximumLength(500);
    }
}
