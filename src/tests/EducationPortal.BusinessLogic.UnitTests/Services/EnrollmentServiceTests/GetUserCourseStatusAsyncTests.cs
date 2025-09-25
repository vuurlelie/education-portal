using EducationPortal.BusinessLogic.Abstractions;
using EducationPortal.BusinessLogic.Services;
using EducationPortal.DataAccess.Abstractions;
using EducationPortal.DataAccess.Entities;
using EducationPortal.DataAccess.Enums;
using Moq;

namespace EducationPortal.BusinessLogic.UnitTests.Services.EnrollmentServiceTests;

public sealed class GetUserCourseStatusAsyncTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ICourseRepository> _courseRepositoryMock;
    private readonly Mock<IUserCourseRepository> _userCourseRepositoryMock;
    private readonly Mock<IUserSkillRepository> _userSkillRepositoryMock;
    private readonly Mock<IMaterialRepository> _materialRepositoryMock;
    private readonly Mock<IUserMaterialRepository> _userMaterialRepositoryMock;
    private readonly Mock<ISkillRepository> _skillRepositoryMock;

    private readonly IEnrollmentService _enrollmentService;

    public GetUserCourseStatusAsyncTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);

        _userRepositoryMock = new Mock<IUserRepository>(MockBehavior.Strict);
        _courseRepositoryMock = new Mock<ICourseRepository>(MockBehavior.Strict);
        _userCourseRepositoryMock = new Mock<IUserCourseRepository>(MockBehavior.Strict);
        _userSkillRepositoryMock = new Mock<IUserSkillRepository>(MockBehavior.Strict);
        _materialRepositoryMock = new Mock<IMaterialRepository>(MockBehavior.Strict);
        _userMaterialRepositoryMock = new Mock<IUserMaterialRepository>(MockBehavior.Strict);
        _skillRepositoryMock = new Mock<ISkillRepository>(MockBehavior.Strict);

        _unitOfWorkMock.SetupGet(unitOfWork => unitOfWork.UserRepository).Returns(_userRepositoryMock.Object);
        _unitOfWorkMock.SetupGet(unitOfWork => unitOfWork.CourseRepository).Returns(_courseRepositoryMock.Object);
        _unitOfWorkMock.SetupGet(unitOfWork => unitOfWork.UserCourseRepository).Returns(_userCourseRepositoryMock.Object);
        _unitOfWorkMock.SetupGet(unitOfWork => unitOfWork.UserSkillRepository).Returns(_userSkillRepositoryMock.Object);
        _unitOfWorkMock.SetupGet(unitOfWork => unitOfWork.MaterialRepository).Returns(_materialRepositoryMock.Object);
        _unitOfWorkMock.SetupGet(unitOfWork => unitOfWork.UserMaterialRepository).Returns(_userMaterialRepositoryMock.Object);
        _unitOfWorkMock.SetupGet(unitOfWork => unitOfWork.SkillRepository).Returns(_skillRepositoryMock.Object);

        _enrollmentService = new EnrollmentService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetUserCourseStatusAsync_NotEnrolled_ReturnsNotEnrolled()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const int courseId = 10;

        _userCourseRepositoryMock
            .Setup(userCourseRepository => userCourseRepository.GetByUserAndCourseWithStatusAsync(userId, courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserCourse?)null);

        // Act
        var state = await _enrollmentService.GetUserCourseStatusAsync(userId, courseId, CancellationToken.None);

        // Assert
        Assert.Equal(CourseEnrollmentState.NotEnrolled, state);
    }

    [Fact]
    public async Task GetUserCourseStatusAsync_StatusCompleted_ReturnsCompleted()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const int courseId = 10;
        var user = new ApplicationUser { Id = userId };
        var course = new Course { Id = courseId, Name = "C" };

        var completed = new CourseStatus { Id = 2, Name = "Completed" };

        _userCourseRepositoryMock
            .Setup(userCourseRepository => userCourseRepository.GetByUserAndCourseWithStatusAsync(userId, courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserCourse { User = user, Course = course, CourseStatus = completed });

        // Act
        var state = await _enrollmentService.GetUserCourseStatusAsync(userId, courseId, CancellationToken.None);

        // Assert
        Assert.Equal(CourseEnrollmentState.Completed, state);
    }

    [Fact]
    public async Task GetUserCourseStatusAsync_StatusInProgress_ReturnsInProgress()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const int courseId = 10;
        var user = new ApplicationUser { Id = userId };
        var course = new Course { Id = courseId, Name = "C" };

        var inProgress = new CourseStatus { Id = 1, Name = "InProgress" };

        _userCourseRepositoryMock
            .Setup(userCourseRepository => userCourseRepository.GetByUserAndCourseWithStatusAsync(userId, courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserCourse { User = user, Course = course, CourseStatus = inProgress });

        // Act
        var state = await _enrollmentService.GetUserCourseStatusAsync(userId, courseId, CancellationToken.None);

        // Assert
        Assert.Equal(CourseEnrollmentState.InProgress, state);
    }
}