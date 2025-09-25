using EducationPortal.DataAccess.Entities;
using EducationPortal.Presentation.Extensions;
using EducationPortal.Presentation.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EducationPortal.Presentation.Controllers;

public sealed class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly TimeProvider _timeProvider;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        TimeProvider timeProvider)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _timeProvider = timeProvider;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.IsAuthenticated())
        {
            return RedirectToAction("MyProfile", "Profile");
        }

        var viewModel = new LoginViewModel { ReturnUrl = returnUrl };
        return View(viewModel);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var signInResult = await _signInManager.PasswordSignInAsync(
            userName: viewModel.Email,
            password: viewModel.Password,
            isPersistent: viewModel.RememberMe,
            lockoutOnFailure: false);

        if (signInResult.Succeeded)
        {
            if (!string.IsNullOrWhiteSpace(viewModel.ReturnUrl) && Url.IsLocalUrl(viewModel.ReturnUrl))
            {
                return Redirect(viewModel.ReturnUrl);
            }

            return RedirectToAction("MyProfile", "Profile");
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return View(viewModel);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register(string? returnUrl = null)
    {
        if (User.IsAuthenticated())
        {
            return RedirectToAction("MyProfile", "Profile");
        }

        var viewModel = new RegisterViewModel { ReturnUrl = returnUrl };
        return View(viewModel);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var existingUser = await _userManager.FindByEmailAsync(viewModel.Email);
        if (existingUser is not null)
        {
            ModelState.AddModelError(string.Empty, "An account with this email already exists. Please sign in.");
            return View(viewModel);
        }

        var newUser = new ApplicationUser
        {
            UserName = viewModel.Email,
            Email = viewModel.Email,
            FullName = string.IsNullOrWhiteSpace(viewModel.FullName) ? null : viewModel.FullName.Trim(),
            CreatedAt = _timeProvider.GetLocalNow().DateTime
        };

        var createResult = await _userManager.CreateAsync(newUser, viewModel.Password);
        if (createResult.Succeeded)
        {
            TempData["Success"] = "Registration successful. You can now sign in.";

            ModelState.Clear();
            var clearedForm = new RegisterViewModel { ReturnUrl = viewModel.ReturnUrl };
            return View(clearedForm);
        }

        foreach (var error in createResult.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login", "Account");
    }
}
