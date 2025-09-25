using EducationPortal.BusinessLogic.Abstractions;
using EducationPortal.BusinessLogic.DTOs.Courses;
using EducationPortal.BusinessLogic.DTOs.Materials;
using EducationPortal.Presentation.Constants;
using EducationPortal.Presentation.ViewModels.Materials;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EducationPortal.Presentation.Mappers;

public static class MaterialViewMapper
{
    public static IReadOnlyList<MaterialListItemViewModel> ToListItems(IEnumerable<MaterialListItemDto> materialDtos)
        => materialDtos
            .Select(materialDto => new MaterialListItemViewModel
            {
                Id = materialDto.Id,
                Title = materialDto.Title,
                Type = materialDto.Type.ToString(),
                FormatName = materialDto.FormatName
            })
            .ToList();

    public static MaterialDetailsViewModel ToDetails(MaterialDetailsDto detailsDto)
        => new MaterialDetailsViewModel
        {
            Id = detailsDto.Id,
            Title = detailsDto.Title,
            Description = detailsDto.Description,
            Type = detailsDto.Type.ToString(),
            DurationSeconds = detailsDto.DurationSeconds,
            HeightPx = detailsDto.HeightPx,
            WidthPx = detailsDto.WidthPx,
            Authors = detailsDto.Authors,
            Pages = detailsDto.Pages,
            FormatId = detailsDto.FormatId,
            FormatName = detailsDto.FormatName,
            PublicationYear = detailsDto.PublicationYear,
            PublishedAt = detailsDto.PublishedAt,
            SourceUrl = detailsDto.SourceUrl
        };

    public static void PopulateLookups(
        MaterialFormViewModel form,
        IReadOnlyList<(int Id, string Name)> formatOptions,
        IReadOnlyList<CourseListItemDto> courseOptions,
        DateOnly today)
    {
        form.FormatOptions = formatOptions
            .Select(format => new SelectListItem { Value = format.Id.ToString(), Text = format.Name })
            .OrderBy(option => option.Text)
            .ToList();

        form.CourseOptions = courseOptions
            .Select(course => new SelectListItem { Value = course.Id.ToString(), Text = course.Name })
            .OrderBy(option => option.Text)
            .ToList();

        if (!form.PublishedAt.HasValue)
        {
            form.PublishedAt = today;
        }

        if (string.IsNullOrWhiteSpace(form.Type))
        {
            form.Type = MaterialUiDefaults.DefaultType;
        }
    }

    public static MaterialFormViewModel ToCreateForm(
        IReadOnlyList<(int Id, string Name)> formatOptions,
        IReadOnlyList<CourseListItemDto> courseOptions,
        DateOnly today)
    {
        var form = new MaterialFormViewModel
        {
            Type = MaterialUiDefaults.DefaultType,
            PublishedAt = today
        };

        PopulateLookups(form, formatOptions, courseOptions, today);
        return form;
    }

    public static MaterialFormViewModel ToEditForm(
        MaterialDetailsDto detailsDto,
        IReadOnlyList<int> selectedCourseIds,
        IReadOnlyList<(int Id, string Name)> formatOptions,
        IReadOnlyList<CourseListItemDto> courseOptions,
        DateOnly today)
    {
        var form = new MaterialFormViewModel
        {
            Id = detailsDto.Id,
            Title = detailsDto.Title,
            Description = detailsDto.Description,
            Type = detailsDto.Type.ToString(),
            DurationSeconds = detailsDto.DurationSeconds,
            HeightPx = detailsDto.HeightPx,
            WidthPx = detailsDto.WidthPx,
            Authors = detailsDto.Authors,
            Pages = detailsDto.Pages,
            FormatId = detailsDto.FormatId,
            PublicationYear = detailsDto.PublicationYear,
            PublishedAt = detailsDto.PublishedAt ?? today,
            SourceUrl = detailsDto.SourceUrl,
            SelectedCourseIds = selectedCourseIds.ToArray()
        };

        PopulateLookups(form, formatOptions, courseOptions, today);
        return form;
    }

    public static async Task<int> DispatchCreateAsync(
        IMaterialService materialService,
        MaterialType materialType,
        MaterialFormViewModel form,
        CancellationToken cancellationToken)
    {
        return materialType switch
        {
            MaterialType.Video => await materialService.CreateVideoAsync(
                new VideoMaterialCreateDto
                {
                    Title = form.Title,
                    Description = form.Description,
                    DurationSeconds = form.DurationSeconds!.Value,
                    HeightPx = form.HeightPx!.Value,
                    WidthPx = form.WidthPx!.Value
                },
                cancellationToken),

            MaterialType.Book => await materialService.CreateBookAsync(
                new BookMaterialCreateDto
                {
                    Title = form.Title,
                    Description = form.Description,
                    Authors = form.Authors ?? string.Empty,
                    Pages = form.Pages!.Value,
                    FormatId = form.FormatId!.Value,
                    PublicationYear = form.PublicationYear!.Value
                },
                cancellationToken),

            MaterialType.Article => await materialService.CreateArticleAsync(
                new ArticleMaterialCreateDto
                {
                    Title = form.Title,
                    Description = form.Description,
                    PublishedAt = form.PublishedAt!.Value,
                    SourceUrl = form.SourceUrl ?? string.Empty
                },
                cancellationToken),

            _ => throw new InvalidOperationException("Unsupported material type")
        };
    }

    public static async Task DispatchUpdateAsync(
        IMaterialService materialService,
        int materialId,
        MaterialType materialType,
        MaterialFormViewModel form,
        CancellationToken cancellationToken)
    {
        switch (materialType)
        {
            case MaterialType.Video:
                await materialService.UpdateVideoAsync(
                    materialId,
                    new VideoMaterialEditDto
                    {
                        Title = form.Title,
                        Description = form.Description,
                        DurationSeconds = form.DurationSeconds,
                        HeightPx = form.HeightPx,
                        WidthPx = form.WidthPx
                    },
                    cancellationToken);
                break;

            case MaterialType.Book:
                await materialService.UpdateBookAsync(
                    materialId,
                    new BookMaterialEditDto
                    {
                        Title = form.Title,
                        Description = form.Description,
                        Authors = form.Authors,
                        Pages = form.Pages,
                        FormatId = form.FormatId,
                        PublicationYear = form.PublicationYear
                    },
                    cancellationToken);
                break;

            case MaterialType.Article:
                await materialService.UpdateArticleAsync(
                    materialId,
                    new ArticleMaterialEditDto
                    {
                        Title = form.Title,
                        Description = form.Description,
                        PublishedAt = form.PublishedAt,
                        SourceUrl = form.SourceUrl
                    },
                    cancellationToken);
                break;

            default:
                throw new InvalidOperationException("Unsupported material type");
        }
    }
}