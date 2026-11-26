using FluentValidation;
using FixnSeven.Application.Features.Orders.DTOs;
using FixnSeven.Domain.Entities;
namespace FixnSeven.Application.Features.Orders.Validators;

public class CreatePaymentValidator : AbstractValidator<CreatePaymentRequestDto>
{
    public CreatePaymentValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("El monto debe ser mayor que 0.")
            .PrecisionScale(12, 2, true).WithMessage("El monto admite como máximo 2 decimales.");
        RuleFor(x => x.Method)
            .Must(m => PaymentMethods.All.Contains(m))
            .WithMessage("El método debe ser Efectivo, Tarjeta o Transferencia.");
        RuleFor(x => x.Reference).MaximumLength(60);
    }
}
