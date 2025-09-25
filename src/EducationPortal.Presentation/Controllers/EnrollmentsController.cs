using EducationPortal.BusinessLogic.Abstractions;
using EducationPortal.Presentation.Extensions;
using EducationPortal.Presentation.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EducationPortal.Presentation.Controllers;

[Authorize]
public sealed class EnrollmentsController : Controller
{
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentsController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enroll(
        [FromForm] IdRouteModel form,
        [FromServices] IValidator<IdRouteModel> formValidator,
        CancellationToken cancellationToken)
    {
        var validation = formValidator.Validate(form);
        if (!validation.IsValid)
        {
            return NotFound();
        }

        var userId = User.GetUserIdOrThrow();
        await _enrollmentService.EnrollAsync(userId, form.Id, cancellationToken);

        return RedirectToAction("MyCourses", "Profile", new { tab = "inprogress" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteCourse(
        [FromForm] IdRouteModel form,
        [FromServices] IValidator<IdRouteModel> formValidator,
        CancellationToken cancellationToken)
    {
        var validation = formValidator.Validate(form);
        if (!validation.IsValid)
        {
            return NotFound();
        }

        var userId = User.GetUserIdOrThrow();
        await _enrollmentService.CompleteCourseAsync(userId, form.Id, cancellationToken);

        return RedirectToAction("MyCourses", "Profile", new { tab = "completed" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkMaterialComplete(
        [FromForm] IdRouteModel form,
        [FromServices] IValidator<IdRouteModel> formValidator,
        CancellationToken cancellationToken)
    {
        var validation = formValidator.Validate(form);
        if (!validation.IsValid)
        {
            return NotFound();
        }

        var userId = User.GetUserIdOrThrow();
        await _enrollmentService.MarkMaterialCompleteAsync(userId, form.Id, cancellationToken);

        return RedirectToAction("MyCourses", "Profile", new { tab = "inprogress" });
    }
}