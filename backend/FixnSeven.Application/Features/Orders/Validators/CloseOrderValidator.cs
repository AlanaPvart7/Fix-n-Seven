using FluentValidation;
using FixnSeven.Application.Features.Orders.DTOs;
namespace FixnSeven.Application.Features.Orders.Validators;

public class CloseOrderValidator : AbstractValidator<CloseOrderRequestDto>
{
    public CloseOrderValidator()
    {
        RuleFor(x => x.WorkDone)
            .NotEmpty().WithMessage("Describa el trabajo realizado para cerrar la orden.")
            .MaximumLength(2000);
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}
