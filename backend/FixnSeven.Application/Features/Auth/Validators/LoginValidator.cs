using FluentValidation;
using FixnSeven.Application.Features.Auth.DTOs;
namespace FixnSeven.Application.Features.Auth.Validators;

public class LoginValidator : AbstractValidator<LoginRequestDto>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage("El correo es obligatorio.")
                             .EmailAddress().WithMessage("El correo no es válido.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("La contraseña es obligatoria.");
    }
}
