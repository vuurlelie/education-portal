using EducationPortal.BusinessLogic.Abstractions;
using EducationPortal.BusinessLogic.Services;
using EducationPortal.DataAccess.Abstractions;
using EducationPortal.DataAccess.Entities;
using Moq;

namespace EducationPortal.BusinessLogic.UnitTests.Services.MaterialServiceTests;

public sealed class GetAssignedCourseIdsAsyncTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMaterialRepository> _materialRepositoryMock;
    private readonly IMaterialService _materialService;

    public GetAssignedCourseIdsAsyncTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
        _materialRepositoryMock = new Mock<IMaterialRepository>(MockBehavior.Strict);

        _unitOfWorkMock.SetupGet(unitOfWork => unitOfWork.MaterialRepository)
                      .Returns(_materialRepositoryMock.Object);

        _materialService = new MaterialService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetAssignedCourseIdsAsync_NotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        _materialRepositoryMock.Setup(materialRepository => materialRepository.GetWithDetailsByIdAsync(10, It.IsAny<CancellationToken>()))
                              .ReturnsAsync((Material?)null);

        // Act + Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _materialService.GetAssignedCourseIdsAsync(10, CancellationToken.None));
    }

    [Fact]
    public async Task GetAssignedCourseIdsAsync_ReturnsDistinctActiveIds()
    {
        // Arrange
        var materialWithLinks = new VideoMaterial
        {
            Id = 5,
            Title = "Title",
            CourseMaterials = new List<CourseMaterial>
            {
                new() { CourseId = 1, RecordStatus = RecordStatus.Active },
                new() { CourseId = 2, RecordStatus = RecordStatus.Active },
                new() { CourseId = 2, RecordStatus = RecordStatus.Active },
                new() { CourseId = 3, RecordStatus = RecordStatus.Deleted }
            }
        };

        _materialRepositoryMock.Setup(materialRepository => materialRepository.GetWithDetailsByIdAsync(5, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(materialWithLinks);

        // Act
        var ids = await _materialService.GetAssignedCourseIdsAsync(5, CancellationToken.None);

        // Assert
        Assert.Equal(new[] { 1, 2 }, ids.OrderBy(id => id));
    }
}