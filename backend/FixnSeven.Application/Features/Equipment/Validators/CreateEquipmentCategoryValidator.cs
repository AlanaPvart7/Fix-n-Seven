using FluentValidation;
using FixnSeven.Application.Features.Equipment.DTOs;
namespace FixnSeven.Application.Features.Equipment.Validators;

public class CreateEquipmentCategoryValidator : AbstractValidator<CreateEquipmentCategoryRequestDto>
{
    public CreateEquipmentCategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre de la categoría es obligatorio.")
            .MaximumLength(60).WithMessage("El nombre no puede pasar de 60 caracteres.");
        RuleFor(x => x.IdentifierLabel)
            .NotEmpty().WithMessage("Indique cómo se identifica el equipo (IMEI, N° de serie...).")
            .MaximumLength(40);
        RuleFor(x => x.UsageUnit)
            .Must(u => u == "Fechas" || u == "Horas")
            .WithMessage("La medida de uso debe ser Fechas u Horas.");
    }
}
