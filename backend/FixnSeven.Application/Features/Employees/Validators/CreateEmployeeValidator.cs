using FluentValidation;
using FixnSeven.Application.Features.Employees.DTOs;
namespace FixnSeven.Application.Features.Employees.Validators;

public class CreateEmployeeValidator : AbstractValidator<CreateEmployeeRequestDto>
{
    private static readonly string[] Roles = { "Admin", "Reception", "Technician" };

    public CreateEmployeeValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(120).WithMessage("El nombre no puede pasar de 120 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo es obligatorio.")
            .EmailAddress().WithMessage("El correo no es válido.")
            .MaximumLength(254);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.")
            .Matches("[A-Z]").WithMessage("La contraseña debe tener al menos una mayúscula.")
            .Matches("[0-9]").WithMessage("La contraseña debe tener al menos un número.");

        RuleFor(x => x.Role)
            .Must(r => Roles.Contains(r))
            .WithMessage("El rol debe ser Admin, Reception o Technician.");
    }
}
