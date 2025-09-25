using EducationPortal.BusinessLogic.Abstractions;
using EducationPortal.BusinessLogic.Services;
using EducationPortal.DataAccess.Abstractions;
using EducationPortal.DataAccess.Entities;
using Moq;

namespace EducationPortal.BusinessLogic.UnitTests.Services.EnrollmentServiceTests;

public sealed class CompleteCourseAsyncTests
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

    public CompleteCourseAsyncTests()
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
    public async Task CompleteCourseAsync_EnrollmentNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const int courseId = 10;

        _userCourseRepositoryMock
            .Setup(userCourseRepository => userCourseRepository.GetByUserAndCourseAsync(userId, courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserCourse?)null);

        // Act + Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _enrollmentService.CompleteCourseAsync(userId, courseId, CancellationToken.None));

        VerifyNoSave();
    }

    [Fact]
    public async Task CompleteCourseAsync_AlreadyCompleted_IsIdempotent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const int courseId = 10;

        var user = new ApplicationUser { Id = userId };
        var course = CreateCourseWithMaterials(courseId, activeMaterialCount: 2);
        var completed = new CourseStatus { Id = 2, Name = "Completed" };

        var enrollment = new UserCourse
        {
            User = user,
            Course = course,
            CourseStatus = completed,
            ProgressPercent = (byte)BusinessRules.MaxProgressPercent
        };

        _userCourseRepositoryMock
            .Setup(userCourseRepository => userCourseRepository.GetByUserAndCourseAsync(userId, courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);

        // Act
        await _enrollmentService.CompleteCourseAsync(userId, courseId, CancellationToken.None);

        // Assert
        VerifyNoSave();
    }

    [Fact]
    public async Task CompleteCourseAsync_CompletesAndAwardsSkills_NewAndExisting()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const int courseId = 10;

        var user = new ApplicationUser { Id = userId };
        var course = CreateCourseWithMaterials(courseId, activeMaterialCount: 3);

        var skillA = new Skill { Id = 5, Name = "EF Core" };
        var skillB = new Skill { Id = 6, Name = "ASP.NET" };

        course.CourseSkills = new List<CourseSkill>
        {
            new CourseSkill { CourseId = courseId, SkillId = 5, Skill = skillA, RecordStatus = RecordStatus.Active },
            new CourseSkill { CourseId = courseId, SkillId = 6, Skill = skillB, RecordStatus = RecordStatus.Active }
        };

        var inProgress = new CourseStatus { Id = 1, Name = "InProgress" };
        var completed = new CourseStatus { Id = 2, Name = "Completed" };

        var enrollment = new UserCourse
        {
            User = user,
            Course = course,
            CourseStatus = inProgress,
            ProgressPercent = 80
        };

        var existingUserSkillB = new UserSkill { User = user, Skill = skillB, Level = 2 };

        _userCourseRepositoryMock
            .Setup(userCourseRepository => userCourseRepository.GetByUserAndCourseAsync(userId, courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);

        _courseRepositoryMock
            .Setup(courseRepository => courseRepository.GetWithDetailsByIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(course);

        _userCourseRepositoryMock
            .Setup(userCourseRepository => userCourseRepository.GetStatusByNameAsync("Completed", It.IsAny<CancellationToken>()))
            .ReturnsAsync(completed);

        _userCourseRepositoryMock.Setup(userCourseRepository => userCourseRepository.Update(enrollment));

        _unitOfWorkMock.SetupGet(unitOfWork => unitOfWork.UserRepository).Returns(_userRepositoryMock.Object);
        _userRepositoryMock.Setup(userRepository => userRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _skillRepositoryMock.Setup(skillRepository => skillRepository.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(skillA);
        _skillRepositoryMock.Setup(skillRepository => skillRepository.GetByIdAsync(6, It.IsAny<CancellationToken>())).ReturnsAsync(skillB);

        _userSkillRepositoryMock.Setup(userSkillRepository => userSkillRepository.GetByUserAndSkillAsync(userId, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserSkill?)null);
        _userSkillRepositoryMock.Setup(userSkillRepository => userSkillRepository.GetByUserAndSkillAsync(userId, 6, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUserSkillB);

        _userSkillRepositoryMock.Setup(userSkillRepository => userSkillRepository.AddAsync(It.Is<UserSkill>(userSkill =>
                userSkill.User == user && userSkill.Skill == skillA && userSkill.Level == 1),
            It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        _userSkillRepositoryMock.Setup(r => r.Update(existingUserSkillB));

        // Act
        await _enrollmentService.CompleteCourseAsync(userId, courseId, CancellationToken.None);

        // Assert
        Assert.Equal("Completed", enrollment.CourseStatus.Name);
        Assert.Equal(BusinessRules.MaxProgressPercent, enrollment.ProgressPercent);
        Assert.Equal(3, existingUserSkillB.Level);

        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CompleteCourseAsync_CompletedStatusMissing_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const int courseId = 10;

        var user = new ApplicationUser { Id = userId };
        var inProgress = new CourseStatus { Id = 1, Name = "InProgress" };
        var course = CreateCourseWithMaterials(courseId, activeMaterialCount: 1);

        var enrollment = new UserCourse
        {
            User = user,
            Course = course,
            CourseStatus = inProgress,
            ProgressPercent = 90
        };

        _userCourseRepositoryMock
            .Setup(userCourseRepository => userCourseRepository.GetByUserAndCourseAsync(userId, courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);

        _courseRepositoryMock
            .Setup(courseRepository => courseRepository.GetWithDetailsByIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(course);

        _userCourseRepositoryMock
            .Setup(userCourseRepository => userCourseRepository.GetStatusByNameAsync("Completed", It.IsAny<CancellationToken>()))
            .ReturnsAsync((CourseStatus?)null);

        // Act + Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _enrollmentService.CompleteCourseAsync(userId, courseId, CancellationToken.None));

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