using FluentValidation;
using FixnSeven.Application.Features.Orders.DTOs;
namespace FixnSeven.Application.Features.Orders.Validators;

public class CreateOrderValidator : AbstractValidator<CreateOrderRequestDto>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.EquipmentId).NotEmpty().WithMessage("Elija el equipo.");
        RuleFor(x => x.ReportedIssue)
            .NotEmpty().WithMessage("Describa el problema que reporta el cliente.")
            .MaximumLength(1000).WithMessage("El problema no puede pasar de 1000 caracteres.");
        RuleFor(x => x.ReceivedCondition).MaximumLength(1000);
    }
}
