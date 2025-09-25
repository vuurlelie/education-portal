using EducationPortal.Presentation.ViewModels.Account;
using FluentValidation;

namespace EducationPortal.Presentation.Validators;

public sealed class LoginViewModelValidator : AbstractValidator<LoginViewModel>
{
    public LoginViewModelValidator()
    {
        RuleFor(viewModel => viewModel.Email)
            .Cascade(CascadeMode.Stop)
            .Must(email => !string.IsNullOrWhiteSpace(email))
                .WithMessage("Email is required.")
            .EmailAddress()
                .WithMessage("Please enter a valid email address.");

        RuleFor(viewModel => viewModel.Password)
            .Cascade(CascadeMode.Stop)
            .Must(password => !string.IsNullOrWhiteSpace(password))
                .WithMessage("Password is required.");
    }
}