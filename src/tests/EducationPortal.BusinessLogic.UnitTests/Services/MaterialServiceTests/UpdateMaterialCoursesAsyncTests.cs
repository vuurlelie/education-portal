using EducationPortal.BusinessLogic.Abstractions;
using EducationPortal.BusinessLogic.Services;
using EducationPortal.DataAccess.Abstractions;
using EducationPortal.DataAccess.Entities;
using Moq;

namespace EducationPortal.BusinessLogic.UnitTests.Services.MaterialServiceTests;

public sealed class UpdateMaterialCoursesAsyncTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMaterialRepository> _materialRepositoryMock;
    private readonly IMaterialService _materialService;

    public UpdateMaterialCoursesAsyncTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
        _materialRepositoryMock = new Mock<IMaterialRepository>(MockBehavior.Strict);

        _unitOfWorkMock.SetupGet(unitOfWork => unitOfWork.MaterialRepository)
                      .Returns(_materialRepositoryMock.Object);
        _unitOfWorkMock.Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(1);

        _materialService = new MaterialService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task UpdateMaterialCoursesAsync_NotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        _materialRepositoryMock.Setup(materialRepository => materialRepository.GetWithDetailsByIdAsync(10, It.IsAny<CancellationToken>()))
                              .ReturnsAsync((Material?)null);

        // Act + Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _materialService.UpdateMaterialCoursesAsync(10, new[] { 1, 2 }, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateMaterialCoursesAsync_TogglesExisting_AddsMissing_AndMarksRemovedDeleted()
    {
        // Arrange
        var material = new VideoMaterial
        {
            Id = 10,
            Title = "Title",
            CourseMaterials = new List<CourseMaterial>
            {
                new() { MaterialId = 10, CourseId = 1, RecordStatus = RecordStatus.Active },
                new() { MaterialId = 10, CourseId = 2, RecordStatus = RecordStatus.Deleted }
            }
        };

        _materialRepositoryMock.Setup(materialRepository => materialRepository.GetWithDetailsByIdAsync(10, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(material);

        var desiredCourseIds = new[] { 2, 3, 3 };

        // Act
        await _materialService.UpdateMaterialCoursesAsync(10, desiredCourseIds, CancellationToken.None);

        // Assert
        Assert.Contains(material.CourseMaterials, link => link.CourseId == 2 && link.RecordStatus == RecordStatus.Active);
        Assert.Contains(material.CourseMaterials, link => link.CourseId == 3 && link.RecordStatus == RecordStatus.Active);
        Assert.Contains(material.CourseMaterials, link => link.CourseId == 1 && link.RecordStatus == RecordStatus.Deleted);
        Assert.Equal(1, material.CourseMaterials.Count(link => link.CourseId == 3));

        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}