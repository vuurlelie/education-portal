using EducationPortal.Presentation.Models;
using FluentValidation;

namespace EducationPortal.Presentation.Validators;

public sealed class MyCoursesQueryModelValidator : AbstractValidator<MyCoursesQueryModel>
{
    private static readonly HashSet<string> AllowedTabs = new(StringComparer.OrdinalIgnoreCase)
    {
        "inprogress",
        "completed"
    };

    public MyCoursesQueryModelValidator()
    {
        RuleFor(model => model.Tab)
            .Must(tab => tab is null || AllowedTabs.Contains(tab))
            .WithMessage("Unsupported tab value. Allowed: inprogress, completed.");
    }
}