using EducationPortal.BusinessLogic.DTOs.Materials;
using EducationPortal.Presentation.Constants;
using EducationPortal.Presentation.ViewModels.Materials;
using FluentValidation;

namespace EducationPortal.Presentation.Validators;

public sealed class MaterialFormViewModelValidator : AbstractValidator<MaterialFormViewModel>
{
    public MaterialFormViewModelValidator()
    {
        RuleFor(materialForm => materialForm.Title)
            .Cascade(CascadeMode.Stop)
            .Must(title => !string.IsNullOrWhiteSpace(title))
                .WithMessage("Title is required.")
            .Must(title => title!.Trim().Length <= UiValidationLimits.MaterialTitleMaxLength)
                .WithMessage($"Title must be at most {UiValidationLimits.MaterialTitleMaxLength} characters long.");

        RuleFor(materialForm => materialForm.Description)
            .Must(description => description is null || description.Trim().Length <= UiValidationLimits.MaterialDescriptionMaxLength)
                .WithMessage($"Description must be at most {UiValidationLimits.MaterialDescriptionMaxLength} characters long.");

        RuleFor(materialForm => materialForm.Authors)
            .Must(authors => authors is null || authors.Trim().Length <= UiValidationLimits.MaterialAuthorsMaxLength)
                .WithMessage($"Authors must be at most {UiValidationLimits.MaterialAuthorsMaxLength} characters long.");

        RuleFor(materialForm => materialForm.Type)
            .Cascade(CascadeMode.Stop)
            .Must(typeText => !string.IsNullOrWhiteSpace(typeText))
                .WithMessage("Type is required.")
            .Must(IsKnownMaterialType)
                .WithMessage("Unknown material type.");

        When(IsVideo, () =>
        {
            RuleFor(materialForm => materialForm.DurationSeconds)
                .NotNull().WithMessage("Duration is required for videos.")
                .Must(value => value > 0).WithMessage("Duration must be positive.");

            RuleFor(materialForm => materialForm.HeightPx)
                .NotNull().WithMessage("Height is required for videos.")
                .Must(value => value > 0).WithMessage("Height must be positive.");

            RuleFor(materialForm => materialForm.WidthPx)
                .NotNull().WithMessage("Width is required for videos.")
                .Must(value => value > 0).WithMessage("Width must be positive.");
        });

        When(IsBook, () =>
        {
            RuleFor(materialForm => materialForm.Pages)
                .NotNull().WithMessage("Pages is required for books.")
                .Must(value => value > 0).WithMessage("Pages must be positive.");

            RuleFor(materialForm => materialForm.FormatId)
                .NotNull().WithMessage("Format is required for books.")
                .Must(value => value > 0).WithMessage("Format id must be positive.");

            RuleFor(materialForm => materialForm.PublicationYear)
                .NotNull().WithMessage("Publication year is required for books.")
                .Must(year => year is not null && year.Value >= UiValidationLimits.PublicationYearMin)
                    .WithMessage($"Publication year must be at least {UiValidationLimits.PublicationYearMin}.")
                .Must(year => year is not null && year.Value <= DateTime.UtcNow.Year + 1)
                    .WithMessage($"Publication year cannot be greater than {DateTime.UtcNow.Year + 1}.");
        });

        When(IsArticle, () =>
        {
            RuleFor(materialForm => materialForm.PublishedAt)
                .NotNull().WithMessage("Published date is required for articles.");

            RuleFor(materialForm => materialForm.SourceUrl)
                .Must(url => string.IsNullOrWhiteSpace(url) || url!.Length <= UiValidationLimits.MaterialSourceUrlMaxLength)
                    .WithMessage($"URL must be at most {UiValidationLimits.MaterialSourceUrlMaxLength} characters long.")
                .Must(url => string.IsNullOrWhiteSpace(url) || Uri.TryCreate(url, UriKind.Absolute, out _))
                    .WithMessage("Invalid URL.");
        });

        RuleFor(materialForm => materialForm.SelectedCourseIds)
            .NotNull().WithMessage("Selected courses collection is required.")
            .Must(courseIds => courseIds.All(id => id > 0))
                .WithMessage("Course ids must be positive.")
            .Must(courseIds => courseIds.Distinct().Count() == courseIds.Length)
                .WithMessage("Duplicate course selection is not allowed.");
    }

    private static bool IsKnownMaterialType(string typeText)
        => Enum.TryParse<MaterialType>(typeText, ignoreCase: true, out _);

    private static bool IsVideo(MaterialFormViewModel materialForm)
        => Enum.TryParse<MaterialType>(materialForm.Type, true, out var parsed) && parsed == MaterialType.Video;

    private static bool IsBook(MaterialFormViewModel materialForm)
        => Enum.TryParse<MaterialType>(materialForm.Type, true, out var parsed) && parsed == MaterialType.Book;

    private static bool IsArticle(MaterialFormViewModel materialForm)
        => Enum.TryParse<MaterialType>(materialForm.Type, true, out var parsed) && parsed == MaterialType.Article;
}