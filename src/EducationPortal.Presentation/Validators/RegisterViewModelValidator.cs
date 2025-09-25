using EducationPortal.Presentation.Constants;
using EducationPortal.Presentation.ViewModels.Account;
using FluentValidation;

namespace EducationPortal.Presentation.Validators;

public sealed class RegisterViewModelValidator : AbstractValidator<RegisterViewModel>
{
    public RegisterViewModelValidator()
    {
        RuleFor(viewModel => viewModel.FullName)
            .Must(fullName => string.IsNullOrWhiteSpace(fullName) || fullName.Trim().Length <= UiValidationLimits.FullNameMaxLength)
            .WithMessage($"Full name must be at most {UiValidationLimits.FullNameMaxLength} characters.");

        RuleFor(viewModel => viewModel.Email)
            .Cascade(CascadeMode.Stop)
            .Must(email => !string.IsNullOrWhiteSpace(email))
                .WithMessage("Email is required.")
            .EmailAddress()
                .WithMessage("Please enter a valid email address.");

        RuleFor(viewModel => viewModel.Password)
            .Cascade(CascadeMode.Stop)
            .Must(password => !string.IsNullOrWhiteSpace(password))
                .WithMessage("Password is required.")
            .MinimumLength(UiValidationLimits.PasswordMinLength)
                .WithMessage($"Password must be at least {UiValidationLimits.PasswordMinLength} characters long.");

        RuleFor(viewModel => viewModel.ConfirmPassword)
            .Equal(viewModel => viewModel.Password)
            .WithMessage("Passwords do not match.");
    }
}