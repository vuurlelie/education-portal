using System.ComponentModel.DataAnnotations;

namespace EducationPortal.Presentation.ViewModels.Account;

public sealed class LoginViewModel
{
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}