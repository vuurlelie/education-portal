using EducationPortal.Presentation.Constants;
using EducationPortal.Presentation.ViewModels.Courses;
using FluentValidation;

namespace EducationPortal.Presentation.Validators.Courses;

public sealed class CourseFormViewModelValidator : AbstractValidator<CourseFormViewModel>
{
    public CourseFormViewModelValidator()
    {
        RuleFor(viewModel => viewModel.Name)
            .Cascade(CascadeMode.Stop)
            .Must(name => !string.IsNullOrWhiteSpace(name))
                .WithMessage("Name is required.")
            .Must(name => name!.Trim().Length <= UiValidationLimits.CourseNameMaxLength)
                .WithMessage($"Name must be at most {UiValidationLimits.CourseNameMaxLength} characters long.");

        RuleFor(viewModel => viewModel.Description)
            .Must(description => description is null || description.Trim().Length <= UiValidationLimits.CourseDescriptionMaxLength)
                .WithMessage($"Description must be at most {UiValidationLimits.CourseDescriptionMaxLength} characters long.");

        RuleFor(viewModel => viewModel.SelectedMaterialIds)
            .NotNull().WithMessage("Selected materials collection is required.")
            .Must(materialIds => materialIds.All(id => id > 0))
                .WithMessage("Material ids must be positive.")
            .Must(materialIds => materialIds.Distinct().Count() == materialIds.Length)
                .WithMessage("Duplicate materials are not allowed.");

        RuleFor(viewModel => viewModel.SelectedSkillIds)
            .NotNull().WithMessage("Selected skills collection is required.")
            .Must(skillIds => skillIds.All(id => id > 0))
                .WithMessage("Skill ids must be positive.")
            .Must(skillIds => skillIds.Distinct().Count() == skillIds.Length)
                .WithMessage("Duplicate skills are not allowed.");
    }
}