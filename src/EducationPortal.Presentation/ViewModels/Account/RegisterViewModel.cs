using System.ComponentModel.DataAnnotations;

namespace EducationPortal.Presentation.ViewModels.Account;

public sealed class RegisterViewModel
{
    [Display(Name = "Full name")]
    public string? FullName { get; set; }

    [DataType(DataType.EmailAddress)]
    public string Email { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password), Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}