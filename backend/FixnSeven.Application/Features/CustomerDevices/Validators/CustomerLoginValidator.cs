using FluentValidation;
using FixnSeven.Application.Features.CustomerDevices.DTOs;
namespace FixnSeven.Application.Features.CustomerDevices.Validators;

public class CustomerLoginValidator : AbstractValidator<CustomerLoginRequestDto>
{
    public CustomerLoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo es obligatorio.")
            .EmailAddress().WithMessage("El correo no es válido.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("La contraseña es obligatoria.");
    }
}
