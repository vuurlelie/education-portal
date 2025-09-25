using EducationPortal.BusinessLogic.DTOs.Materials;
using EducationPortal.DataAccess.Entities;

namespace EducationPortal.BusinessLogic.Mappers;

public static class MaterialMapper
{
    public static IReadOnlyList<MaterialListItemDto> ToListItemDtos(this IEnumerable<Material> materials)
    {
        var items = new List<MaterialListItemDto>();
        foreach (var material in materials)
        {
            items.Add(material.ToListItemDto());
        }
        return items;
    }

    public static MaterialListItemDto ToListItemDto(this Material material)
    {
        return MapToListItem(material);
    }

    public static MaterialDetailsDto ToDetailsDto(this Material material)
    {
        return MapToDetails(material);
    }

    public static VideoMaterial ToEntity(this VideoMaterialCreateDto dto)
    {
        return new VideoMaterial
        {
            Title = dto.Title,
            Description = dto.Description,
            DurationSec = dto.DurationSeconds,
            HeightPx = dto.HeightPx,
            WidthPx = dto.WidthPx
        };
    }

    public static BookMaterial ToEntity(this BookMaterialCreateDto dto)
    {
        return new BookMaterial
        {
            Title = dto.Title,
            Description = dto.Description,
            Authors = dto.Authors,
            Pages = dto.Pages,
            FormatId = dto.FormatId,
            PublicationYear = dto.PublicationYear
        };
    }

    public static ArticleMaterial ToEntity(this ArticleMaterialCreateDto dto)
    {
        return new ArticleMaterial
        {
            Title = dto.Title,
            Description = dto.Description,
            PublishedAt = dto.PublishedAt,
            SourceUrl = dto.SourceUrl
        };
    }

    public static void ApplyChanges(this VideoMaterialEditDto changes, VideoMaterial entity)
    {
        entity.Title = changes.Title;
        entity.Description = changes.Description;

        if (changes.DurationSeconds.HasValue)
        {
            entity.DurationSec = changes.DurationSeconds.Value;
        }
        if (changes.HeightPx.HasValue)
        {
            entity.HeightPx = changes.HeightPx.Value;
        }
        if (changes.WidthPx.HasValue)
        {
            entity.WidthPx = changes.WidthPx.Value;
        }
    }

    public static void ApplyChanges(this BookMaterialEditDto changes, BookMaterial entity)
    {
        entity.Title = changes.Title;
        entity.Description = changes.Description;

        if (changes.Authors is not null)
        {
            entity.Authors = changes.Authors;
        }
        if (changes.Pages.HasValue)
        {
            entity.Pages = changes.Pages.Value;
        }
        if (changes.FormatId.HasValue)
        {
            entity.FormatId = changes.FormatId.Value;
        }
        if (changes.PublicationYear.HasValue)
        {
            entity.PublicationYear = changes.PublicationYear.Value;
        }
    }

    public static void ApplyChanges(this ArticleMaterialEditDto changes, ArticleMaterial entity)
    {
        entity.Title = changes.Title;
        entity.Description = changes.Description;

        if (changes.PublishedAt.HasValue)
        {
            entity.PublishedAt = changes.PublishedAt.Value;
        }
        if (changes.SourceUrl is not null)
        {
            entity.SourceUrl = changes.SourceUrl;
        }
    }

    private static MaterialListItemDto MapToListItem(Material material)
    {
        return material switch
        {
            VideoMaterial video => new MaterialListItemDto
            {
                Id = video.Id,
                Title = video.Title,
                Type = MaterialType.Video
            },
            BookMaterial book => new MaterialListItemDto
            {
                Id = book.Id,
                Title = book.Title,
                Type = MaterialType.Book,
                FormatId = book.FormatId,
                FormatName = book.Format?.Name
            },
            ArticleMaterial article => new MaterialListItemDto
            {
                Id = article.Id,
                Title = article.Title,
                Type = MaterialType.Article
            },
            _ => throw new InvalidOperationException("Unsupported material type")
        };
    }

    private static MaterialDetailsDto MapToDetails(Material material)
    {
        return material switch
        {
            VideoMaterial video => new MaterialDetailsDto
            {
                Id = video.Id,
                Title = video.Title,
                Description = video.Description,
                Type = MaterialType.Video,
                DurationSeconds = video.DurationSec,
                HeightPx = video.HeightPx,
                WidthPx = video.WidthPx
            },
            BookMaterial book => new MaterialDetailsDto
            {
                Id = book.Id,
                Title = book.Title,
                Description = book.Description,
                Type = MaterialType.Book,
                Authors = book.Authors,
                Pages = book.Pages,
                FormatId = book.FormatId,
                FormatName = book.Format?.Name,
                PublicationYear = book.PublicationYear
            },
            ArticleMaterial article => new MaterialDetailsDto
            {
                Id = article.Id,
                Title = article.Title,
                Description = article.Description,
                Type = MaterialType.Article,
                PublishedAt = article.PublishedAt,
                SourceUrl = article.SourceUrl
            },
            _ => throw new InvalidOperationException("Unsupported material type")
        };
    }
}