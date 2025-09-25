using EducationPortal.BusinessLogic.Abstractions;
using EducationPortal.BusinessLogic.DTOs.Courses;
using EducationPortal.BusinessLogic.Services;
using EducationPortal.DataAccess.Abstractions;
using EducationPortal.DataAccess.Entities;
using Moq;

namespace EducationPortal.BusinessLogic.UnitTests.Services.CourseServiceTests;

public sealed class UpdateAsyncTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new(MockBehavior.Strict);
    private readonly Mock<ICourseRepository> _courseRepositoryMock = new(MockBehavior.Strict);
    private readonly ICourseService _courseService;

    public UpdateAsyncTests()
    {
        _unitOfWorkMock.SetupGet(unitOfWork => unitOfWork.CourseRepository).Returns(_courseRepositoryMock.Object);
        _unitOfWorkMock.Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(1);

        _courseService = new CourseService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        _courseRepositoryMock.Setup(unitOfWork => unitOfWork.GetByIdAsync(10, It.IsAny<CancellationToken>()))
                             .ReturnsAsync((Course?)null);

        // Act + Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _courseService.UpdateAsync(10, new CourseEditDto { Name = "Name", Description = "Description" }, CancellationToken.None));

        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesNameAndDescription_AndSaves()
    {
        // Arrange
        var entity = new Course { Id = 10, Name = "OldName", Description = "OldDescription" };

        _courseRepositoryMock.Setup(courseRepository => courseRepository.GetByIdAsync(10, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(entity);
        _courseRepositoryMock.Setup(courseRepository => courseRepository.Update(entity));

        var edit = new CourseEditDto { Name = "NewName", Description = "NewDescription" };

        // Act
        await _courseService.UpdateAsync(10, edit, CancellationToken.None);

        // Assert
        Assert.Equal("NewName", entity.Name);
        Assert.Equal("NewDescription", entity.Description);

        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}