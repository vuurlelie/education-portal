using EducationPortal.BusinessLogic.Abstractions;
using EducationPortal.BusinessLogic.DTOs.Materials;
using EducationPortal.BusinessLogic.Services;
using EducationPortal.DataAccess.Abstractions;
using EducationPortal.DataAccess.Entities;
using Moq;

namespace EducationPortal.BusinessLogic.UnitTests.Services.CourseServiceTests;

public sealed class GetDetailsAsyncTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new(MockBehavior.Strict);
    private readonly Mock<ICourseRepository> _courseRepositoryMock = new(MockBehavior.Strict);
    private readonly ICourseService _courseService;

    public GetDetailsAsyncTests()
    {
        _unitOfWorkMock.SetupGet(unitOfWork => unitOfWork.CourseRepository).Returns(_courseRepositoryMock.Object);
        _courseService = new CourseService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetDetailsAsync_NotFound_ReturnsNull()
    {
        // Arrange
        _courseRepositoryMock.Setup(courseRepository => courseRepository.GetWithDetailsByIdAsync(999, It.IsAny<CancellationToken>()))
                             .ReturnsAsync((Course?)null);

        // Act
        var details = await _courseService.GetDetailsAsync(999, CancellationToken.None);

        // Assert
        Assert.Null(details);
    }

    [Fact]
    public async Task GetDetailsAsync_FiltersInactiveLinks_AndMapsSortedMaterialsAndSkills()
    {
        // Arrange
        var course = new Course
        {
            Id = 10,
            Name = "Test",
            Description = "Description",
            CourseMaterials = new List<CourseMaterial>
            {
                new() { Material = new VideoMaterial { Id = 1, Title = "B-Video" }, MaterialId = 1, RecordStatus = RecordStatus.Active },
                new() { Material = new BookMaterial  { Id = 2, Title = "A-Book", Authors = "Tom" }, MaterialId = 2, RecordStatus = RecordStatus.Active },
                new() { Material = new ArticleMaterial { Id = 3, Title = "C-Article", SourceUrl = "url", PublishedAt = DateOnly.MinValue }, MaterialId = 3, RecordStatus = RecordStatus.Active },
                new() { Material = new VideoMaterial { Id = 4, Title = "Z-Unknown" }, MaterialId = 4, RecordStatus = RecordStatus.Deleted }
            },
            CourseSkills = new List<CourseSkill>
            {
                new() { Skill = new Skill { Id = 7, Name = "LINQ" }, SkillId = 7, RecordStatus = RecordStatus.Active },
                new() { Skill = new Skill { Id = 8, Name = "EF Core" }, SkillId = 8, RecordStatus = RecordStatus.Active },
                new() { Skill = new Skill { Id = 9, Name = "Old" }, SkillId = 9, RecordStatus = RecordStatus.Deleted }
            }
        };

        _courseRepositoryMock.Setup(courseRepository => courseRepository.GetWithDetailsByIdAsync(10, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(course);

        // Act
        var details = await _courseService.GetDetailsAsync(10, CancellationToken.None);

        // Assert
        Assert.NotNull(details);
        Assert.Equal(10, details!.Id);
        Assert.Equal("Test", details.Name);
        Assert.Equal("Description", details.Description);

        Assert.Collection(details.Materials,
            first =>
            {
                Assert.Equal(2, first.Id);
                Assert.Equal("A-Book", first.Title);
                Assert.Equal(MaterialType.Book, first.Type);
            },
            second =>
            {
                Assert.Equal(1, second.Id);
                Assert.Equal("B-Video", second.Title);
                Assert.Equal(MaterialType.Video, second.Type);
            },
            third =>
            {
                Assert.Equal(3, third.Id);
                Assert.Equal("C-Article", third.Title);
                Assert.Equal(MaterialType.Article, third.Type == MaterialType.Book ? MaterialType.Article : MaterialType.Article); 
            }
        );
    }
}