using EducationPortal.BusinessLogic.Abstractions;
using EducationPortal.DataAccess.Enums;
using EducationPortal.Presentation.Extensions;
using EducationPortal.Presentation.Mappers;
using EducationPortal.Presentation.Models;
using EducationPortal.Presentation.ViewModels.Courses;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EducationPortal.Presentation.Controllers;

public sealed class CoursesController : Controller
{
    private readonly ICourseService _courseService;
    private readonly IMaterialService _materialService;
    private readonly ISkillService _skillService;
    private readonly IEnrollmentService _enrollmentService;

    public CoursesController(
        ICourseService courseService,
        IMaterialService materialService,
        ISkillService skillService,
        IEnrollmentService enrollmentService)
    {
        _courseService = courseService;
        _materialService = materialService;
        _skillService = skillService;
        _enrollmentService = enrollmentService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var courseDtos = await _courseService.GetAllAsync(cancellationToken);

        IReadOnlyDictionary<int, bool>? canStartByCourseId = null;
        if (User.IsAuthenticated())
        {
            var userId = User.GetUserIdOrThrow();
            var dictionary = new Dictionary<int, bool>(capacity: courseDtos.Count);

            foreach (var courseDto in courseDtos)
            {
                var state = await _enrollmentService.GetUserCourseStatusAsync(userId, courseDto.Id, cancellationToken);
                dictionary[courseDto.Id] = state == CourseEnrollmentState.NotEnrolled;
            }

            canStartByCourseId = dictionary;
        }

        var viewModels = CourseViewMapper.ToListItems(courseDtos, canStartByCourseId);
        return View(viewModels);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Details(
        [FromRoute] IdRouteModel route,
        [FromServices] IValidator<IdRouteModel> routeValidator,
        CancellationToken cancellationToken)
    {
        ValidationResult validation = await routeValidator.ValidateAsync(route, cancellationToken);
        if (!validation.IsValid)
        {
            return NotFound();
        }

        var courseDetailsDto = await _courseService.GetDetailsAsync(route.Id, cancellationToken);
        if (courseDetailsDto is null)
        {
            return NotFound();
        }

        var canStart = false;
        if (User.IsAuthenticated())
        {
            var userId = User.GetUserIdOrThrow();
            var state = await _enrollmentService.GetUserCourseStatusAsync(userId, route.Id, cancellationToken);
            canStart = state == CourseEnrollmentState.NotEnrolled;
        }

        var viewModel = CourseViewMapper.ToDetails(courseDetailsDto, canStart);
        return View(viewModel);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enroll(
        [FromRoute] IdRouteModel route,
        [FromServices] IValidator<IdRouteModel> routeValidator,
        CancellationToken cancellationToken)
    {
        ValidationResult validation = await routeValidator.ValidateAsync(route, cancellationToken);
        if (!validation.IsValid)
        {
            return NotFound();
        }

        var userId = User.GetUserIdOrThrow();
        await _enrollmentService.EnrollAsync(userId, route.Id, cancellationToken);

        return RedirectToAction(nameof(Details), new { id = route.Id });
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var materialDtos = await _materialService.GetAllAsync(cancellationToken);
        var skillDtos = await _skillService.GetAllAsync(cancellationToken);

        var form = CourseFormMapper.ToCreateForm(materialDtos, skillDtos);
        return PartialView("Partials/_CourseForm", form);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CourseFormViewModel form,
        [FromServices] IValidator<CourseFormViewModel> formValidator,
        CancellationToken cancellationToken)
    {
        var validation = await formValidator.ValidateAsync(form, cancellationToken);
        if (!validation.IsValid)
        {
            var materialDtos = await _materialService.GetAllAsync(cancellationToken);
            var skillDtos = await _skillService.GetAllAsync(cancellationToken);

            form.MaterialOptions = CourseFormMapper.ToCreateForm(materialDtos, skillDtos).MaterialOptions;
            form.SkillOptions = CourseFormMapper.ToCreateForm(materialDtos, skillDtos).SkillOptions;

            foreach (var error in validation.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }

            return PartialView("Partials/_CourseForm", form);
        }

        var newCourseId = await _courseService.CreateAsync(
            new BusinessLogic.DTOs.Courses.CourseCreateDto
            {
                Name = form.Name,
                Description = form.Description
            },
            cancellationToken);

        await _courseService.UpdateCourseMaterialsAsync(newCourseId, form.SelectedMaterialIds, cancellationToken);
        await _courseService.UpdateCourseSkillsAsync(newCourseId, form.SelectedSkillIds, cancellationToken);

        return RedirectToAction(nameof(Details), new { id = newCourseId });
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Edit(
        [FromRoute] IdRouteModel route,
        [FromServices] IValidator<IdRouteModel> routeValidator,
        CancellationToken cancellationToken)
    {
        var validation = await routeValidator.ValidateAsync(route, cancellationToken);
        if (!validation.IsValid)
        {
            return NotFound();
        }

        var courseDetailsDto = await _courseService.GetDetailsAsync(route.Id, cancellationToken);
        if (courseDetailsDto is null)
        {
            return NotFound();
        }

        var materialDtos = await _materialService.GetAllAsync(cancellationToken);
        var skillDtos = await _skillService.GetAllAsync(cancellationToken);

        var form = CourseFormMapper.ToEditForm(courseDetailsDto, materialDtos, skillDtos);
        return PartialView("Partials/_CourseForm", form);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        [FromRoute] IdRouteModel route,
        CourseFormViewModel form,
        [FromServices] IValidator<IdRouteModel> routeValidator,
        [FromServices] IValidator<CourseFormViewModel> formValidator,
        CancellationToken cancellationToken)
    {
        var routeValidation = await routeValidator.ValidateAsync(route, cancellationToken);
        if (!routeValidation.IsValid)
        {
            return NotFound();
        }

        var formValidation = await formValidator.ValidateAsync(form, cancellationToken);
        if (!formValidation.IsValid)
        {
            var materialDtos = await _materialService.GetAllAsync(cancellationToken);
            var skillDtos = await _skillService.GetAllAsync(cancellationToken);

            form.MaterialOptions = CourseFormMapper.ToCreateForm(materialDtos, skillDtos).MaterialOptions;
            form.SkillOptions = CourseFormMapper.ToCreateForm(materialDtos, skillDtos).SkillOptions;

            foreach (var error in formValidation.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }

            return PartialView("Partials/_CourseForm", form);
        }

        await _courseService.UpdateAsync(
            route.Id,
            new BusinessLogic.DTOs.Courses.CourseEditDto
            {
                Name = form.Name,
                Description = form.Description
            },
            cancellationToken);

        await _courseService.UpdateCourseMaterialsAsync(route.Id, form.SelectedMaterialIds, cancellationToken);
        await _courseService.UpdateCourseSkillsAsync(route.Id, form.SelectedSkillIds, cancellationToken);

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
            var deleted = await _courseService.DeleteAsync(route.Id, cancellationToken);
            if (!deleted)
            {
                return NotFound();
            }

            TempData["Success"] = "Course has been deleted.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException exception)
        {
            TempData["Error"] = exception.Message;
            return RedirectToAction(nameof(Details), new { id = route.Id });
        }
    }
}