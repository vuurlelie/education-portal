using EducationPortal.BusinessLogic.Abstractions;
using EducationPortal.BusinessLogic.DTOs.Courses;
using EducationPortal.BusinessLogic.DTOs.Skills;
using EducationPortal.Presentation.Mappers;
using EducationPortal.Presentation.Models;
using EducationPortal.Presentation.ViewModels.Skills;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EducationPortal.Presentation.Controllers;

public sealed class SkillsController : Controller
{
    private readonly ISkillService _skillService;
    private readonly ICourseService _courseService;

    public SkillsController(ISkillService skillService, ICourseService courseService)
    {
        _skillService = skillService;
        _courseService = courseService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var skillDtos = await _skillService.GetAllAsync(cancellationToken);
        var viewModel = SkillViewMapper.ToListItemViewModels(skillDtos);
        return View(viewModel);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Details(
        [FromRoute] IdRouteModel route,
        [FromServices] IValidator<IdRouteModel> routeValidator,
        CancellationToken cancellationToken)
    {
        var routeValidation = routeValidator.Validate(route);
        if (!routeValidation.IsValid)
        {
            return NotFound();
        }

        var skillDetails = await _skillService.GetDetailsAsync(route.Id, cancellationToken);
        if (skillDetails is null)
        {
            return NotFound();
        }

        var courses = await _courseService.GetAllAsync(cancellationToken);
        var courseNameById = courses.ToDictionary(course => course.Id, course => course.Name);

        var viewModel = SkillViewMapper.ToDetailsViewModel(skillDetails, courseNameById);
        return View(viewModel);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var courseOptions = await _courseService.GetAllAsync(cancellationToken);
        var viewModel = SkillViewMapper.ToCreateFormViewModel(courseOptions);
        return PartialView("Partials/_SkillForm", viewModel);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        SkillFormViewModel form,
        [FromServices] IValidator<SkillFormViewModel> formValidator,
        CancellationToken cancellationToken)
    {
        var validation = formValidator.Validate(form);
        if (!validation.IsValid)
        {
            var courseOptions = await _courseService.GetAllAsync(cancellationToken);
            form.CourseOptions = SelectListMapper.ToCourseOptions(courseOptions);
            ModelState.ApplyValidationResult(validation);
            return PartialView("Partials/_SkillForm", form);
        }

        var newSkillId = await _skillService.CreateAsync(new SkillCreateDto
        {
            Name = form.Name,
            Description = form.Description
        }, cancellationToken);

        await _skillService.UpdateSkillCoursesAsync(newSkillId, form.SelectedCourseIds, cancellationToken);

        return RedirectToAction(nameof(Details), new { id = newSkillId });
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Edit(
        [FromRoute] IdRouteModel route,
        [FromServices] IValidator<IdRouteModel> routeValidator,
        CancellationToken cancellationToken)
    {
        var routeValidation = routeValidator.Validate(route);
        if (!routeValidation.IsValid)
        {
            return NotFound();
        }

        var skillDetails = await _skillService.GetDetailsAsync(route.Id, cancellationToken);
        if (skillDetails is null)
        {
            return NotFound();
        }

        var courseOptions = await _courseService.GetAllAsync(cancellationToken);
        var selectedCourseIds = await _skillService.GetAssignedCourseIdsAsync(route.Id, cancellationToken);

        var viewModel = SkillViewMapper.ToEditFormViewModel(skillDetails, courseOptions, selectedCourseIds);
        return PartialView("Partials/_SkillForm", viewModel);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        [FromRoute] IdRouteModel route,
        SkillFormViewModel form,
        [FromServices] IValidator<IdRouteModel> routeValidator,
        [FromServices] IValidator<SkillFormViewModel> formValidator,
        CancellationToken cancellationToken)
    {
        var routeValidation = routeValidator.Validate(route);
        if (!routeValidation.IsValid)
        {
            return NotFound();
        }

        var validation = formValidator.Validate(form);
        if (!validation.IsValid)
        {
            var courseOptions = await _courseService.GetAllAsync(cancellationToken);
            form.CourseOptions = SelectListMapper.ToCourseOptions(courseOptions);
            ModelState.ApplyValidationResult(validation);
            return PartialView("Partials/_SkillForm", form);
        }

        await _skillService.UpdateAsync(route.Id, new SkillEditDto
        {
            Name = form.Name,
            Description = form.Description
        }, cancellationToken);

        await _skillService.UpdateSkillCoursesAsync(route.Id, form.SelectedCourseIds, cancellationToken);

        return RedirectToAction(nameof(Details), new { id = route.Id });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(
    [FromRoute] IdRouteModel route,
    [FromServices] IValidator<IdRouteModel> routeValidator,
    CancellationToken cancellationToken)
    {
        var validationResult = await routeValidator.ValidateAsync(route, cancellationToken);
        if (!validationResult.IsValid)
        {
            return NotFound();
        }

        try
        {
            var deleted = await _skillService.DeleteAsync(route.Id, cancellationToken);
            if (!deleted)
            {
                return NotFound();
            }

            TempData["Success"] = "Skill has been deleted.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException exception)
        {
            TempData["Error"] = exception.Message;
            return RedirectToAction(nameof(Details), new { id = route.Id });
        }
    }
}