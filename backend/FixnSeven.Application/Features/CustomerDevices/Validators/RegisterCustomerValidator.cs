using FluentValidation;
using FixnSeven.Application.Features.CustomerDevices.DTOs;
namespace FixnSeven.Application.Features.CustomerDevices.Validators;

public class RegisterCustomerValidator : AbstractValidator<RegisterCustomerRequestDto>
{
    public RegisterCustomerValidator()
    {
        RuleFor(x => x.Email).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("El correo es obligatorio.")
            .EmailAddress().WithMessage("El correo no es válido.")
            .MaximumLength(254);
        RuleFor(x => x.Password).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.")
            .Matches("[A-Z]").WithMessage("La contraseña debe tener al menos una mayúscula.")
            .Matches("[0-9]").WithMessage("La contraseña debe tener al menos un número.");
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(120);
        RuleFor(x => x.Phone).MaximumLength(30);
    }
}
