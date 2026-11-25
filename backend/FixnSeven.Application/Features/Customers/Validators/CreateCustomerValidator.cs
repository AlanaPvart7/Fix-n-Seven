using FluentValidation;
using FixnSeven.Application.Features.Customers.DTOs;

namespace FixnSeven.Application.Features.Customers.Validators;

public class CreateCustomerValidator : AbstractValidator<CreateCustomerRequestDto>
{
    public CreateCustomerValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("El nombre del cliente es obligatorio.")
            .MaximumLength(120).WithMessage("El nombre no puede pasar de 120 caracteres.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("El teléfono es obligatorio.")
            .MaximumLength(20).WithMessage("El teléfono no puede pasar de 20 caracteres.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("El correo no es válido.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}