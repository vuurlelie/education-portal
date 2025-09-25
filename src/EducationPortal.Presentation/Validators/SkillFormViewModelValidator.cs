using System.Linq;
using EducationPortal.Presentation.Constants;
using EducationPortal.Presentation.ViewModels.Skills;
using FluentValidation;

namespace EducationPortal.Presentation.Validators;

public sealed class SkillFormViewModelValidator : AbstractValidator<SkillFormViewModel>
{
    public SkillFormViewModelValidator()
    {
        RuleFor(viewModel => viewModel.Name)
            .Cascade(CascadeMode.Stop)
            .Must(name => !string.IsNullOrWhiteSpace(name))
                .WithMessage("Name is required.")
            .Must(name => name!.Trim().Length <= UiValidationLimits.SkillNameMaxLength)
                .WithMessage($"Name must be at most {UiValidationLimits.SkillNameMaxLength} characters long.");

        RuleFor(viewModel => viewModel.Description)
            .Must(description => description is null || description.Trim().Length <= UiValidationLimits.SkillDescriptionMaxLength)
                .WithMessage($"Description must be at most {UiValidationLimits.SkillDescriptionMaxLength} characters long.");

        RuleFor(viewModel => viewModel.SelectedCourseIds)
            .NotNull().WithMessage("Selected courses collection is required.")
            .Must(courseIds => courseIds.All(id => id > 0))
                .WithMessage("Course ids must be positive.")
            .Must(courseIds => courseIds.Distinct().Count() == courseIds.Length)
                .WithMessage("Duplicate course selection is not allowed.");
    }
}