using EducationPortal.Presentation.Models;
using FluentValidation;

namespace EducationPortal.Presentation.Validators;

public sealed class IdRouteModelValidator : AbstractValidator<IdRouteModel>
{
    public IdRouteModelValidator()
    {
        RuleFor(model => model.Id).GreaterThan(0);
    }
}