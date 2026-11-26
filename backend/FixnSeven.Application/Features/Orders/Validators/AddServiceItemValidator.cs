using FluentValidation;
using FixnSeven.Application.Features.Orders.DTOs;
namespace FixnSeven.Application.Features.Orders.Validators;

public class AddServiceItemValidator : AbstractValidator<AddServiceItemRequestDto>
{
    public AddServiceItemValidator()
    {
        RuleFor(x => x.Kind)
            .Must(k => k == "Trabajo" || k == "Repuesto")
            .WithMessage("El tipo debe ser Trabajo o Repuesto.");
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("La descripción es obligatoria.")
            .MaximumLength(300);
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("La cantidad debe ser mayor que 0.");
        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("El precio no puede ser negativo.");
    }
}
