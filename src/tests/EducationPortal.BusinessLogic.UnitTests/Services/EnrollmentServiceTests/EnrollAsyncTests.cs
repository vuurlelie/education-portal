using EducationPortal.BusinessLogic.Abstractions;
using EducationPortal.BusinessLogic.Services;
using EducationPortal.DataAccess.Abstractions;
using EducationPortal.DataAccess.Entities;
using Moq;

namespace EducationPortal.BusinessLogic.UnitTests.Services.EnrollmentServiceTests;

public sealed class EnrollAsyncTests
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

    public EnrollAsyncTests()
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

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _enrollmentService = new EnrollmentService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task EnrollAsync_UserNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const int courseId = 10;

        _userRepositoryMock.Setup(userRepository => userRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act + Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _enrollmentService.EnrollAsync(userId, courseId, CancellationToken.None));

        VerifyNoSave();
    }

    [Fact]
    public async Task EnrollAsync_CourseNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const int courseId = 10;

        var user = new ApplicationUser { Id = userId };

        _userRepositoryMock.Setup(userRepository => userRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _courseRepositoryMock.Setup(courseRepository => courseRepository.GetWithDetailsByIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Course?)null);

        // Act + Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _enrollmentService.EnrollAsync(userId, courseId, CancellationToken.None));

        VerifyNoSave();
    }

    [Fact]
    public async Task EnrollAsync_ExistingEnrollmentAlreadyCompleted_DoesNothing()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const int courseId = 10;

        var user = new ApplicationUser { Id = userId };
        var course = CreateCourseWithMaterials(courseId, activeMaterialCount: 2);

        var completedStatus = new CourseStatus { Id = 2, Name = "Completed" };

        var existingEnrollment = new UserCourse
        {
            User = user,
            Course = course,
            CourseStatus = completedStatus,
            ProgressPercent = (byte)BusinessRules.MaxProgressPercent
        };

        _userRepositoryMock.Setup(userRepository => userRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _courseRepositoryMock.Setup(courseRepository => courseRepository.GetWithDetailsByIdAsync(courseId, It.IsAny<CancellationToken>())).ReturnsAsync(course);
        _userCourseRepositoryMock.Setup(userCourseRepository => userCourseRepository.GetByUserAndCourseAsync(userId, courseId, It.IsAny<CancellationToken>())).ReturnsAsync(existingEnrollment);

        // Act
        await _enrollmentService.EnrollAsync(userId, courseId, CancellationToken.None);

        // Assert
        VerifyNoSave();
    }

    [Fact]
    public async Task EnrollAsync_ExistingEnrollmentInProgress_UpdatesStatusAndProgress()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const int courseId = 10;

        var user = new ApplicationUser { Id = userId };
        var course = CreateCourseWithMaterials(courseId, activeMaterialCount: 4);

        var inProgressStatus = new CourseStatus { Id = 1, Name = "InProgress" };

        var existingEnrollment = new UserCourse
        {
            User = user,
            Course = course,
            CourseStatus = inProgressStatus,
            ProgressPercent = 0
        };

        _userRepositoryMock.Setup(userRepository => userRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _courseRepositoryMock.Setup(courseRepository => courseRepository.GetWithDetailsByIdAsync(courseId, It.IsAny<CancellationToken>())).ReturnsAsync(course);
        _userCourseRepositoryMock.Setup(userCourseRepository => userCourseRepository.GetByUserAndCourseAsync(userId, courseId, It.IsAny<CancellationToken>())).ReturnsAsync(existingEnrollment);

        _userCourseRepositoryMock.Setup(userCourseRepository => userCourseRepository.GetStatusByNameAsync("InProgress", It.IsAny<CancellationToken>()))
            .ReturnsAsync(inProgressStatus);

        _userMaterialRepositoryMock.Setup(userMaterialRepository =>
                userMaterialRepository.CountCompletedByUserForMaterialIdsAsync(userId, It.Is<int[]>(materialIds => materialIds.Length == 4), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _userCourseRepositoryMock.Setup(userCourseRepository => userCourseRepository.Update(existingEnrollment));

        // Act
        await _enrollmentService.EnrollAsync(userId, courseId, CancellationToken.None);

        // Assert
        Assert.Equal("InProgress", existingEnrollment.CourseStatus.Name);
        Assert.Equal(25, existingEnrollment.ProgressPercent);

        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EnrollAsync_NewEnrollment_SetsInProgressAndInitialProgress()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const int courseId = 10;

        var user = new ApplicationUser { Id = userId };
        var course = CreateCourseWithMaterials(courseId, activeMaterialCount: 3);

        var inProgressStatus = new CourseStatus { Id = 1, Name = "InProgress" };

        _userRepositoryMock.Setup(userRepository => userRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _courseRepositoryMock.Setup(courseRepository => courseRepository.GetWithDetailsByIdAsync(courseId, It.IsAny<CancellationToken>())).ReturnsAsync(course);
        _userCourseRepositoryMock.Setup(userCourseRepository => userCourseRepository.GetByUserAndCourseAsync(userId, courseId, It.IsAny<CancellationToken>())).ReturnsAsync((UserCourse?)null);

        _userCourseRepositoryMock.Setup(userCourseRepository => userCourseRepository.GetStatusByNameAsync("InProgress", It.IsAny<CancellationToken>()))
            .ReturnsAsync(inProgressStatus);

        _userMaterialRepositoryMock.Setup(userMaterialRepository =>
                userMaterialRepository.CountCompletedByUserForMaterialIdsAsync(userId, It.Is<int[]>(materialIds => materialIds.Length == 3), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _userCourseRepositoryMock.Setup(userCourseRepository => userCourseRepository.AddAsync(It.Is<UserCourse>(userCourse =>
                userCourse.User == user &&
                userCourse.Course == course &&
                userCourse.CourseStatus == inProgressStatus &&
                userCourse.ProgressPercent == 33),
            It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        await _enrollmentService.EnrollAsync(userId, courseId, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EnrollAsync_InProgressStatusMissing_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const int courseId = 10;

        var user = new ApplicationUser { Id = userId };
        var course = CreateCourseWithMaterials(courseId, activeMaterialCount: 2);

        _userRepositoryMock.Setup(userRepository => userRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _courseRepositoryMock.Setup(courseRepository => courseRepository.GetWithDetailsByIdAsync(courseId, It.IsAny<CancellationToken>())).ReturnsAsync(course);
        _userCourseRepositoryMock.Setup(userCourseRepository => userCourseRepository.GetByUserAndCourseAsync(userId, courseId, It.IsAny<CancellationToken>())).ReturnsAsync((UserCourse?)null);

        _userCourseRepositoryMock.Setup(userCourseRepository => userCourseRepository.GetStatusByNameAsync("InProgress", It.IsAny<CancellationToken>()))
            .ReturnsAsync((CourseStatus?)null);

        // Act + Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _enrollmentService.EnrollAsync(userId, courseId, CancellationToken.None));

        VerifyNoSave();
    }

    private static Course CreateCourseWithMaterials(int courseId, int activeMaterialCount)
    {
        var course = new Course
        {
            Id = courseId,
            Name = "Test Course",
            CourseMaterials = new List<CourseMaterial>()
        };

        for (int index = 0; index < activeMaterialCount; index++)
        {
            course.CourseMaterials.Add(new CourseMaterial
            {
                CourseId = courseId,
                MaterialId = 1000 + index,
                RecordStatus = RecordStatus.Active,
                Material = new VideoMaterial { Id = 1000 + index, Title = $"Material {index}" }
            });
        }

        return course;
    }

    private void VerifyNoSave()
    {
        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}