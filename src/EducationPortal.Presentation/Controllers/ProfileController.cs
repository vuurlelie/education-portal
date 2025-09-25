using EducationPortal.BusinessLogic.Abstractions;
using EducationPortal.BusinessLogic.DTOs.Profile;
using EducationPortal.Presentation.Extensions;
using EducationPortal.Presentation.Mappers;
using EducationPortal.Presentation.Models;
using EducationPortal.Presentation.ViewModels.Profile;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EducationPortal.Presentation.Controllers;

[Authorize]
public sealed class ProfileController : Controller
{
    private readonly IProfileService _profileService;
    private readonly ICourseService _courseService;
    private readonly IEnrollmentService _enrollmentService;

    public ProfileController(
        IProfileService profileService,
        ICourseService courseService,
        IEnrollmentService enrollmentService)
    {
        _profileService = profileService;
        _courseService = courseService;
        _enrollmentService = enrollmentService;
    }

    [HttpGet]
    public async Task<IActionResult> MyProfile(CancellationToken cancellationToken)
    {
        var userId = User.GetUserIdOrThrow();

        UserProfileDto profileDto = await _profileService.GetProfileAsync(userId, cancellationToken);
        var viewModel = profileDto.ToViewModel();

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Index() => RedirectToAction(nameof(MyProfile));

    [HttpGet]
    public async Task<IActionResult> MyCourses(
        [FromQuery] MyCoursesQueryModel query,
        [FromServices] IValidator<MyCoursesQueryModel> queryValidator,
        CancellationToken cancellationToken)
    {
        var validationResult = queryValidator.Validate(query);
        if (!validationResult.IsValid)
        {
            foreach (var failure in validationResult.Errors)
            {
                ModelState.AddModelError(failure.PropertyName, failure.ErrorMessage);
            }

            return View(new MyCoursesViewModel());
        }

        var userId = User.GetUserIdOrThrow();

        var inProgressDtos = await _profileService.GetCoursesInProgressAsync(userId, cancellationToken);
        var completedDtos = await _profileService.GetCompletedCoursesAsync(userId, cancellationToken);

        var inProgressDetailed = new System.Collections.Generic.List<CourseInProgressViewModel>();
        if (inProgressDtos.Count > 0)
        {
            var completedMaterialIds = await _enrollmentService.GetUserCompletedMaterialIdsAsync(userId, cancellationToken);
            var completedMaterialIdSet = completedMaterialIds.ToHashSet();

            foreach (var courseItem in inProgressDtos.OrderBy(item => item.CourseName))
            {
                var courseDetails = await _courseService.GetDetailsAsync(courseItem.CourseId, cancellationToken);
                if (courseDetails is null)
                {
                    continue;
                }

                var detailedView = courseItem.ToCourseInProgressViewModel(
                    courseDetails.Materials,
                    completedMaterialIdSet);

                inProgressDetailed.Add(detailedView);
            }
        }

        var viewModel = new MyCoursesViewModel
        {
            InProgress = inProgressDtos.ToUserCourseItemViewModels(),
            InProgressDetailed = inProgressDetailed,
            Completed = completedDtos.ToUserCourseItemViewModels()
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> MySkills(CancellationToken cancellationToken)
    {
        var userId = User.GetUserIdOrThrow();

        var userSkills = await _profileService.GetSkillsAsync(userId, cancellationToken);

        var viewModel = userSkills
            .Select(skill => new UserSkillItemViewModel
            {
                SkillId = skill.SkillId,
                SkillName = skill.SkillName,
                Level = skill.Level
            })
            .ToList();

        return View(viewModel);
    }
}