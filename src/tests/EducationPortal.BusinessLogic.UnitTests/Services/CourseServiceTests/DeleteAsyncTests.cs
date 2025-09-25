using EducationPortal.BusinessLogic.Abstractions;
using EducationPortal.BusinessLogic.Services;
using EducationPortal.DataAccess.Abstractions;
using Moq;

namespace EducationPortal.BusinessLogic.UnitTests.Services.CourseServiceTests;

public sealed class DeleteAsyncTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new(MockBehavior.Strict);
    private readonly Mock<ICourseRepository> _courseRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IUserCourseRepository> _userCourseRepositoryMock = new(MockBehavior.Strict);

    private readonly ICourseService _courseService;

    public DeleteAsyncTests()
    {
        _unitOfWorkMock.SetupGet(unitOfWork => unitOfWork.CourseRepository)
                       .Returns(_courseRepositoryMock.Object);

        _unitOfWorkMock.SetupGet(unitOfWork => unitOfWork.UserCourseRepository)
                       .Returns(_userCourseRepositoryMock.Object);

        _unitOfWorkMock.Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(1);

        _courseService = new CourseService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task DeleteAsync_WhenCourseHasNoEnrollments_DeletesAndSaves_ReturnsTrue()
    {
        // Arrange
        const int courseId = 10;

        _userCourseRepositoryMock
            .Setup(userCourseRepository => userCourseRepository.AnyActiveByCourseIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _courseRepositoryMock
            .Setup(courseRepository => courseRepository.DeleteByIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var deleted = await _courseService.DeleteAsync(courseId, CancellationToken.None);

        // Assert
        Assert.True(deleted);

        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _courseRepositoryMock.Verify(courseRepository => courseRepository.DeleteByIdAsync(courseId, It.IsAny<CancellationToken>()), Times.Once);
        _userCourseRepositoryMock.Verify(userCourseRepository => userCourseRepository.AnyActiveByCourseIdAsync(courseId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenCourseNotFound_ReturnsFalse_AndDoesNotSave()
    {
        // Arrange
        const int courseId = 10;

        _userCourseRepositoryMock
            .Setup(userCourseRepository => userCourseRepository.AnyActiveByCourseIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _courseRepositoryMock
            .Setup(courseRepository => courseRepository.DeleteByIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var deleted = await _courseService.DeleteAsync(courseId, CancellationToken.None);

        // Assert
        Assert.False(deleted);

        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _courseRepositoryMock.Verify(courseRepository => courseRepository.DeleteByIdAsync(courseId, It.IsAny<CancellationToken>()), Times.Once);
        _userCourseRepositoryMock.Verify(userCourseRepository => userCourseRepository.AnyActiveByCourseIdAsync(courseId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenCourseHasEnrollments_ThrowsInvalidOperationException_AndDoesNotSaveOrDelete()
    {
        // Arrange
        const int courseId = 10;

        _userCourseRepositoryMock
            .Setup(userCourseRepository => userCourseRepository.AnyActiveByCourseIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act + Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _courseService.DeleteAsync(courseId, CancellationToken.None));

        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _courseRepositoryMock.Verify(courseRepository => courseRepository.DeleteByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _userCourseRepositoryMock.Verify(userCourseRepository => userCourseRepository.AnyActiveByCourseIdAsync(courseId, It.IsAny<CancellationToken>()), Times.Once);
    }
}