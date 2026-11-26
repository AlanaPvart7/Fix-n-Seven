using FluentValidation;
using FixnSeven.Application.Features.CustomerDevices.DTOs;
namespace FixnSeven.Application.Features.CustomerDevices.Validators;

public class UpdateCustomerDeviceValidator : AbstractValidator<UpdateCustomerDeviceRequestDto>
{
    public UpdateCustomerDeviceValidator()
    {
        RuleFor(x => x.Alias).MaximumLength(60);
        RuleFor(x => x.Accessories).MaximumLength(300);
        RuleFor(x => x.PublicDescription)
            .MaximumLength(500).WithMessage("La descripción pública admite hasta 500 caracteres.");
        RuleFor(x => x.PrivateNotes).MaximumLength(2000);
        RuleFor(x => x.PurchaseDate)
            .Must(d => d <= DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1))
            .When(x => x.PurchaseDate is not null)
            .WithMessage("La fecha de compra no puede ser futura.");
    }
}
