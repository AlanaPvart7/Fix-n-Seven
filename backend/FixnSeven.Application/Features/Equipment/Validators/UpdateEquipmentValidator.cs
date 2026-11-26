using FluentValidation;
using FixnSeven.Application.Features.Equipment.DTOs;
namespace FixnSeven.Application.Features.Equipment.Validators;

public class UpdateEquipmentValidator : AbstractValidator<UpdateEquipmentRequestDto>
{
    public UpdateEquipmentValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty().WithMessage("Elija la categoría del equipo.");
        RuleFor(x => x.Identifier)
            .NotEmpty().WithMessage("El identificador (IMEI o N° de serie) es obligatorio.")
            .MaximumLength(60).WithMessage("El identificador no puede pasar de 60 caracteres.");
        RuleFor(x => x.Brand)
            .NotEmpty().WithMessage("La marca es obligatoria.")
            .MaximumLength(60);
        RuleFor(x => x.Model)
            .NotEmpty().WithMessage("El modelo es obligatorio.")
            .MaximumLength(60);
        RuleFor(x => x.CurrentUsage)
            .GreaterThanOrEqualTo(0).When(x => x.CurrentUsage is not null)
            .WithMessage("El uso actual no puede ser negativo.");
    }
}
