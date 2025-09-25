using EducationPortal.BusinessLogic.Abstractions;
using EducationPortal.BusinessLogic.Services;
using EducationPortal.DataAccess.Abstractions;
using EducationPortal.DataAccess.Entities;
using Moq;

namespace EducationPortal.BusinessLogic.UnitTests.Services.CourseServiceTests;

public sealed class UpdateCourseMaterialsAsyncTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new(MockBehavior.Strict);
    private readonly Mock<ICourseRepository> _courseRepositoryMock = new(MockBehavior.Strict);
    private readonly ICourseService _courseService;

    public UpdateCourseMaterialsAsyncTests()
    {
        _unitOfWorkMock.SetupGet(unitOfWork => unitOfWork.CourseRepository).Returns(_courseRepositoryMock.Object);
        _unitOfWorkMock.Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(1);

        _courseService = new CourseService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task UpdateCourseMaterialsAsync_NotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        _courseRepositoryMock.Setup(courseRepository => courseRepository.GetWithDetailsByIdAsync(10, It.IsAny<CancellationToken>()))
                             .ReturnsAsync((Course?)null);

        // Act + Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _courseService.UpdateCourseMaterialsAsync(10, new[] { 1, 2 }, CancellationToken.None));

        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateCourseMaterialsAsync_TogglesExistingAndAddsMissing_NoDuplicates()
    {
        // Arrange
        var course = new Course
        {
            Id = 10,
            Name = "Course",
            CourseMaterials = new List<CourseMaterial>
            {
                new() { CourseId = 10, MaterialId = 1, RecordStatus = RecordStatus.Active },
                new() { CourseId = 10, MaterialId = 2, RecordStatus = RecordStatus.Deleted }
            }
        };

        _courseRepositoryMock.Setup(courseRepository => courseRepository.GetWithDetailsByIdAsync(10, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(course);

        var desiredIds = new[] { 1, 2, 2, 3 };

        // Act
        await _courseService.UpdateCourseMaterialsAsync(10, desiredIds, CancellationToken.None);

        // Assert
        Assert.Contains(course.CourseMaterials, courseMaterial => courseMaterial.MaterialId == 1 && courseMaterial.RecordStatus == RecordStatus.Active);
        Assert.Contains(course.CourseMaterials, courseMaterial => courseMaterial.MaterialId == 2 && courseMaterial.RecordStatus == RecordStatus.Active);
        Assert.Contains(course.CourseMaterials, courseMaterial => courseMaterial.MaterialId == 3 && courseMaterial.RecordStatus == RecordStatus.Active);
        Assert.Equal(1, course.CourseMaterials.Count(courseMaterial => courseMaterial.MaterialId == 3));

        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateCourseMaterialsAsync_ByMarkingDeleted_RemovesNowMissingIds()
    {
        // Arrange
        var course = new Course
        {
            Id = 10,
            Name = "Course",
            CourseMaterials = new List<CourseMaterial>
            {
                new() { CourseId = 10, MaterialId = 1, RecordStatus = RecordStatus.Active },
                new() { CourseId = 10, MaterialId = 2, RecordStatus = RecordStatus.Active }
            }
        };

        _courseRepositoryMock.Setup(courseRepository => courseRepository.GetWithDetailsByIdAsync(10, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(course);

        // Act
        await _courseService.UpdateCourseMaterialsAsync(10, new[] { 2 }, CancellationToken.None);

        // Assert
        Assert.Contains(course.CourseMaterials, courseMaterial => courseMaterial.MaterialId == 1 && courseMaterial.RecordStatus == RecordStatus.Deleted);
        Assert.Contains(course.CourseMaterials, courseMaterial => courseMaterial.MaterialId == 2 && courseMaterial.RecordStatus == RecordStatus.Active);

        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}