using EducationPortal.BusinessLogic.Abstractions;
using EducationPortal.BusinessLogic.Services;
using EducationPortal.DataAccess.Abstractions;
using EducationPortal.DataAccess.Entities;
using Moq;

namespace EducationPortal.BusinessLogic.UnitTests.Services.CourseServiceTests;

public sealed class GetAllAsyncTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new(MockBehavior.Strict);
    private readonly Mock<ICourseRepository> _courseRepositoryMock = new(MockBehavior.Strict);
    private readonly ICourseService _courseService;

    public GetAllAsyncTests()
    {
        _unitOfWorkMock.SetupGet(unitOfWork => unitOfWork.CourseRepository).Returns(_courseRepositoryMock.Object);
        _courseService = new CourseService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_NoCourses_ReturnsEmptyList()
    {
        // Arrange
        _courseRepositoryMock.Setup(courseRepository => courseRepository.GetAllAsync(It.IsAny<CancellationToken>()))
                             .ReturnsAsync(new List<Course>());

        // Act
        var result = await _courseService.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_MapsIdAndName_ForEachCourse()
    {
        // Arrange
        var courses = new List<Course>
        {
            new() { Id = 1, Name = ".NET Basics" },
            new() { Id = 2, Name = "Advanced C#" }
        };

        _courseRepositoryMock.Setup(courseRepository => courseRepository.GetAllAsync(It.IsAny<CancellationToken>()))
                             .ReturnsAsync(courses);

        // Act
        var items = await _courseService.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.Collection(items,
            first =>
            {
                Assert.Equal(1, first.Id);
                Assert.Equal(".NET Basics", first.Name);
            },
            second =>
            {
                Assert.Equal(2, second.Id);
                Assert.Equal("Advanced C#", second.Name);
            }
        );
    }
}