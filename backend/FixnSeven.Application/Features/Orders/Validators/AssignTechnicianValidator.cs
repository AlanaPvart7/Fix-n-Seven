using FluentValidation;
using FixnSeven.Application.Features.Orders.DTOs;
namespace FixnSeven.Application.Features.Orders.Validators;

public class AssignTechnicianValidator : AbstractValidator<AssignTechnicianRequestDto>
{
    public AssignTechnicianValidator()
    {
        RuleFor(x => x.TechnicianId).NotEmpty().WithMessage("Elija el técnico.");
    }
}
