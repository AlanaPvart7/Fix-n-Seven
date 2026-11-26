using FluentValidation;
using FixnSeven.Application.Features.CustomerDevices.DTOs;
namespace FixnSeven.Application.Features.CustomerDevices.Validators;

public class LinkDeviceValidator : AbstractValidator<LinkDeviceRequestDto>
{
    public LinkDeviceValidator()
    {
        RuleFor(x => x.VerificationCode)
            .NotEmpty().WithMessage("Escriba el código de verificación del QR.")
            .MaximumLength(20);
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Escriba el código privado del dueño o el código de transferencia.")
            .MaximumLength(20);
    }
}
