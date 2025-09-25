using EducationPortal.DataAccess.Entities;
using EducationPortal.Presentation.Constants;
using EducationPortal.Presentation.ViewModels.Home;

namespace EducationPortal.Presentation.Mappers;

public static class HomeViewMapper
{
    public static IReadOnlyList<HomeCourseSummaryViewModel> ToHomeCourseSummaries(
        IEnumerable<Course> courses,
        int maxItems = HomePageConstants.FeaturedCoursesCount)
    {
        if (courses is null)
        {
            return Array.Empty<HomeCourseSummaryViewModel>();
        }

        return courses
            .Select(course => new HomeCourseSummaryViewModel
            {
                Id = course.Id,
                Name = course.Name,
                Description = string.IsNullOrWhiteSpace(course.Description) ? "-" : course.Description!
            })
            .Take(maxItems)
            .ToList();
    }
}