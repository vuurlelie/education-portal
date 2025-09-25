using EducationPortal.BusinessLogic.Abstractions;
using EducationPortal.BusinessLogic.DTOs.Materials;
using EducationPortal.Presentation.Mappers;
using EducationPortal.Presentation.Models;
using EducationPortal.Presentation.ViewModels.Materials;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EducationPortal.Presentation.Controllers;

public sealed class MaterialsController : Controller
{
    private readonly IMaterialService _materialService;
    private readonly ICourseService _courseService;
    private readonly TimeProvider _timeProvider;

    public MaterialsController(
        IMaterialService materialService,
        ICourseService courseService,
        TimeProvider timeProvider)
    {
        _materialService = materialService;
        _courseService = courseService;
        _timeProvider = timeProvider;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Index(string? type, CancellationToken cancellationToken)
    {
        var dtos = await _materialService.GetAllAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(type) &&
            Enum.TryParse<MaterialType>(type, true, out var filterType))
        {
            dtos = dtos.Where(dto => dto.Type == filterType).ToList();
        }

        var viewModel = MaterialViewMapper.ToListItems(dtos);
        return View(viewModel);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Details(
        IdRouteModel route,
        [FromServices] IValidator<IdRouteModel> routeValidator,
        CancellationToken cancellationToken)
    {
        var validation = await routeValidator.ValidateAsync(route, cancellationToken);
        if (!validation.IsValid) return NotFound();

        var detailsDto = await _materialService.GetDetailsAsync(route.Id, cancellationToken);
        if (detailsDto is null) return NotFound();

        var viewModel = MaterialViewMapper.ToDetails(detailsDto);
        return View(viewModel);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var formats = await _materialService.GetBookFormatsAsync(cancellationToken);
        var courses = await _courseService.GetAllAsync(cancellationToken);
        var today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);

        var viewModel = MaterialViewMapper.ToCreateForm(formats, courses, today);
        return PartialView("Partials/_MaterialForm", viewModel);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        MaterialFormViewModel form,
        [FromServices] IValidator<MaterialFormViewModel> formValidator,
        CancellationToken cancellationToken)
    {
        var validation = await formValidator.ValidateAsync(form, cancellationToken);
        if (!validation.IsValid)
        {
            validation.AddToModelState(ModelState, null);

            var formats = await _materialService.GetBookFormatsAsync(cancellationToken);
            var courses = await _courseService.GetAllAsync(cancellationToken);
            var today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);

            MaterialViewMapper.PopulateLookups(form, formats, courses, today);
            return PartialView("Partials/_MaterialForm", form);
        }

        if (!Enum.TryParse<MaterialType>(form.Type, true, out var materialType))
        {
            return BadRequest();
        }

        var newId = await MaterialViewMapper.DispatchCreateAsync(_materialService, materialType, form, cancellationToken);
        await _materialService.UpdateMaterialCoursesAsync(newId, form.SelectedCourseIds, cancellationToken);

        return RedirectToAction(nameof(Details), new { id = newId });
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Edit(
        IdRouteModel route,
        [FromServices] IValidator<IdRouteModel> routeValidator,
        CancellationToken cancellationToken)
    {
        var validation = await routeValidator.ValidateAsync(route, cancellationToken);
        if (!validation.IsValid) return NotFound();

        var detailsDto = await _materialService.GetDetailsAsync(route.Id, cancellationToken);
        if (detailsDto is null) return NotFound();

        var selectedCourseIds = await _materialService.GetAssignedCourseIdsAsync(route.Id, cancellationToken);
        var formats = await _materialService.GetBookFormatsAsync(cancellationToken);
        var courses = await _courseService.GetAllAsync(cancellationToken);
        var today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);

        var viewModel = MaterialViewMapper.ToEditForm(detailsDto, selectedCourseIds, formats, courses, today);
        return PartialView("Partials/_MaterialForm", viewModel);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        IdRouteModel route,
        MaterialFormViewModel form,
        [FromServices] IValidator<IdRouteModel> routeValidator,
        [FromServices] IValidator<MaterialFormViewModel> formValidator,
        CancellationToken cancellationToken)
    {
        var idValidation = await routeValidator.ValidateAsync(route, cancellationToken);
        if (!idValidation.IsValid) return NotFound();

        var formValidation = await formValidator.ValidateAsync(form, cancellationToken);
        if (!formValidation.IsValid)
        {
            formValidation.AddToModelState(ModelState, null);

            var formats = await _materialService.GetBookFormatsAsync(cancellationToken);
            var courses = await _courseService.GetAllAsync(cancellationToken);
            var today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);

            MaterialViewMapper.PopulateLookups(form, formats, courses, today);
            return PartialView("Partials/_MaterialForm", form);
        }

        if (!Enum.TryParse<MaterialType>(form.Type, true, out var materialType))
        {
            return BadRequest();
        }

        await MaterialViewMapper.DispatchUpdateAsync(_materialService, route.Id, materialType, form, cancellationToken);
        await _materialService.UpdateMaterialCoursesAsync(route.Id, form.SelectedCourseIds, cancellationToken);

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
            var deleted = await _materialService.DeleteAsync(route.Id, cancellationToken);
            if (!deleted)
            {
                return NotFound();
            }

            TempData["Success"] = "Material has been deleted.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException exception)
        {
            TempData["Error"] = exception.Message;
            return RedirectToAction(nameof(Details), new { id = route.Id });
        }
    }
}