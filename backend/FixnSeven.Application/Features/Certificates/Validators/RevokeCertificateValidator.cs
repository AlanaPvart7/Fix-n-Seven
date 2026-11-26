using FluentValidation;
using FixnSeven.Application.Features.Certificates.DTOs;
namespace FixnSeven.Application.Features.Certificates.Validators;

public class RevokeCertificateValidator : AbstractValidator<RevokeCertificateRequestDto>
{
    public RevokeCertificateValidator()
    {
        RuleFor(x => x.Reason).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Explique por que se revoca el certificado.")
            .MinimumLength(5).WithMessage("El motivo debe tener al menos 5 caracteres.")
            .MaximumLength(500);
    }
}
