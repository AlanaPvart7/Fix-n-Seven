using FluentValidation;
using FixnSeven.Application.Features.Enterprises.DTOs;
namespace FixnSeven.Application.Features.Enterprises.Validators;

public class RegisterEnterpriseValidator : AbstractValidator<RegisterEnterpriseRequestDto>
{
    public RegisterEnterpriseValidator()
    {
        RuleFor(x => x.EnterpriseName).NotEmpty().WithMessage("El nombre de la empresa es obligatorio.")
                                      .MaximumLength(120);
        RuleFor(x => x.EnterpriseTypeId).NotEmpty().WithMessage("Elija el tipo de empresa.");
        RuleFor(x => x.PlanId).NotEmpty().WithMessage("Elija un plan.");
        RuleFor(x => x.AdminFullName).NotEmpty().WithMessage("El nombre del administrador es obligatorio.")
                                     .MaximumLength(120);
        RuleFor(x => x.AdminEmail).NotEmpty().WithMessage("El correo es obligatorio.")
                                  .EmailAddress().WithMessage("El correo no es válido.")
                                  .MaximumLength(254);
        RuleFor(x => x.Password).NotEmpty().WithMessage("La contraseña es obligatoria.")
                                .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.")
                                .Matches("[A-Z]").WithMessage("La contraseña debe tener al menos una mayúscula.")
                                .Matches("[0-9]").WithMessage("La contraseña debe tener al menos un número.");
    }
}
