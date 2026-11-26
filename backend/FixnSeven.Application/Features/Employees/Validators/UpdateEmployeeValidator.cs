using FluentValidation;
using FixnSeven.Application.Features.Employees.DTOs;
namespace FixnSeven.Application.Features.Employees.Validators;

public class UpdateEmployeeValidator : AbstractValidator<UpdateEmployeeRequestDto>
{
    private static readonly string[] Roles = { "Admin", "Reception", "Technician" };

    public UpdateEmployeeValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(120).WithMessage("El nombre no puede pasar de 120 caracteres.");
        RuleFor(x => x.Role)
            .Must(r => Roles.Contains(r))
            .WithMessage("El rol debe ser Admin, Reception o Technician.");
    }
}
