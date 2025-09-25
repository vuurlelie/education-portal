using EducationPortal.DataAccess.Abstractions;
using EducationPortal.Presentation.Constants;
using EducationPortal.Presentation.Mappers;
using Microsoft.AspNetCore.Mvc;

namespace EducationPortal.Presentation.Controllers;

public sealed class HomeController : Controller
{
    private readonly ICourseRepository _courseRepository;

    public HomeController(ICourseRepository courseRepository)
    {
        _courseRepository = courseRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var courses = await _courseRepository.GetAllAsync(cancellationToken);

        var viewModel = HomeViewMapper.ToHomeCourseSummaries(
            courses,
            HomePageConstants.FeaturedCoursesCount);

        return View(viewModel);
    }
}