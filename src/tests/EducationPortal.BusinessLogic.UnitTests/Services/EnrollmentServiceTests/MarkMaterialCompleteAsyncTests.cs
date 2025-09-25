using EducationPortal.BusinessLogic.Abstractions;
using EducationPortal.BusinessLogic.Services;
using EducationPortal.DataAccess.Abstractions;
using EducationPortal.DataAccess.Entities;
using Moq;

namespace EducationPortal.BusinessLogic.UnitTests.Services.EnrollmentServiceTests;

public sealed class MarkMaterialCompleteAsyncTests
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

    public MarkMaterialCompleteAsyncTests()
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
    public async Task MarkMaterialCompleteAsync_UserNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const int materialId = 200;

        _userRepositoryMock.Setup(userRepository => userRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act + Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _enrollmentService.MarkMaterialCompleteAsync(userId, materialId, CancellationToken.None));

        VerifyNoSave();
    }

    [Fact]
    public async Task MarkMaterialCompleteAsync_MaterialNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const int materialId = 200;

        var user = new ApplicationUser { Id = userId };

        _userRepositoryMock.Setup(userRepository => userRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _materialRepositoryMock.Setup(materialRepository => materialRepository.GetWithDetailsByIdAsync(materialId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Material?)null);

        // Act + Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _enrollmentService.MarkMaterialCompleteAsync(userId, materialId, CancellationToken.None));

        VerifyNoSave();
    }

    [Fact]
    public async Task MarkMaterialCompleteAsync_UserMaterialNotExists_AddsIt()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const int materialId = 200;

        var user = new ApplicationUser { Id = userId };
        var material = new VideoMaterial { Id = materialId, Title = "Video material" };
        material.CourseMaterials = new List<CourseMaterial>();

        _userRepositoryMock.Setup(userRepository => userRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _materialRepositoryMock.Setup(materialRepository => materialRepository.GetWithDetailsByIdAsync(materialId, It.IsAny<CancellationToken>())).ReturnsAsync(material);

        _userMaterialRepositoryMock
            .Setup(userMaterialRepository => userMaterialRepository.GetByUserAndMaterialAsync(userId, materialId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserMaterial?)null);

        _userMaterialRepositoryMock
            .Setup(userMaterialRepository => userMaterialRepository.AddAsync(It.Is<UserMaterial>(userMaterial => userMaterial.User == user && userMaterial.Material == material),
                It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        await _enrollmentService.MarkMaterialCompleteAsync(userId, materialId, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkMaterialCompleteAsync_AffectedCourseWithoutEnrollment_SkipsRecalc()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const int materialId = 200;
        const int courseId = 10;

        var user = new ApplicationUser { Id = userId };
        var material = new VideoMaterial { Id = materialId, Title = "Video Material" };

        material.CourseMaterials = new List<CourseMaterial>
        {
            new CourseMaterial { CourseId = courseId, MaterialId = materialId, RecordStatus = RecordStatus.Active }
        };

        _userRepositoryMock.Setup(userRepository => userRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _materialRepositoryMock.Setup(materialRepository => materialRepository.GetWithDetailsByIdAsync(materialId, It.IsAny<CancellationToken>())).ReturnsAsync(material);

        _userMaterialRepositoryMock
            .Setup(userMaterialRepository => userMaterialRepository.GetByUserAndMaterialAsync(userId, materialId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserMaterial { User = user, Material = material });

        _userCourseRepositoryMock
            .Setup(userCourseRepository => userCourseRepository.GetByUserAndCourseAsync(userId, courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserCourse?)null);

        // Act
        await _enrollmentService.MarkMaterialCompleteAsync(userId, materialId, CancellationToken.None);

        // Assert
        VerifyNoSave();
    }

    [Fact]
    public async Task MarkMaterialCompleteAsync_AffectedCourseRecalculatesProgress_NotComplete()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const int materialId = 200;
        const int courseId = 10;

        var user = new ApplicationUser { Id = userId };
        var course = CreateCourseWithMaterials(courseId, activeMaterialCount: 4);
        var material = new VideoMaterial { Id = materialId, Title = "Video Material" };

        material.CourseMaterials = new List<CourseMaterial>
        {
            new CourseMaterial { CourseId = courseId, MaterialId = materialId, RecordStatus = RecordStatus.Active }
        };

        var inProgress = new CourseStatus { Id = 1, Name = "InProgress" };

        var enrollment = new UserCourse
        {
            User = user,
            Course = course,
            CourseStatus = inProgress,
            ProgressPercent = 0
        };

        _userRepositoryMock.Setup(userRepository => userRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _materialRepositoryMock.Setup(materialRepository => materialRepository.GetWithDetailsByIdAsync(materialId, It.IsAny<CancellationToken>())).ReturnsAsync(material);

        _userMaterialRepositoryMock
            .Setup(userMaterialRepository => userMaterialRepository.GetByUserAndMaterialAsync(userId, materialId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserMaterial { User = user, Material = material });

        _userCourseRepositoryMock
            .Setup(userCourseRepository => userCourseRepository.GetByUserAndCourseAsync(userId, courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);

        _courseRepositoryMock
            .Setup(courseRepository => courseRepository.GetWithDetailsByIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(course);

        _userMaterialRepositoryMock
            .Setup(userMaterialRepository => userMaterialRepository.CountCompletedByUserForMaterialIdsAsync(userId, It.Is<int[]>(materialIds => materialIds.Length == 4), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _userCourseRepositoryMock.Setup(userCourseRepository => userCourseRepository.Update(enrollment));

        // Act
        await _enrollmentService.MarkMaterialCompleteAsync(userId, materialId, CancellationToken.None);

        // Assert
        Assert.Equal(25, enrollment.ProgressPercent);
        Assert.Equal("InProgress", enrollment.CourseStatus.Name);

        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task MarkMaterialCompleteAsync_AffectedCourseBecomesComplete_AwardsNewSkills()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const int materialId = 200;
        const int courseId = 10;

        var user = new ApplicationUser { Id = userId };

        var course = CreateCourseWithMaterials(courseId, activeMaterialCount: 1);

        var skill = new Skill { Id = 7, Name = "LINQ" };
        course.CourseSkills = new List<CourseSkill>
        {
            new CourseSkill { CourseId = courseId, SkillId = 7, Skill = skill, RecordStatus = RecordStatus.Active }
        };

        var material = new VideoMaterial { Id = materialId, Title = "Video material" };
        material.CourseMaterials = new List<CourseMaterial>
        {
            new CourseMaterial { CourseId = courseId, MaterialId = materialId, RecordStatus = RecordStatus.Active }
        };

        var inProgress = new CourseStatus { Id = 1, Name = "InProgress" };
        var completed = new CourseStatus { Id = 2, Name = "Completed" };

        var enrollment = new UserCourse
        {
            User = user,
            Course = course,
            CourseStatus = inProgress,
            ProgressPercent = 0
        };

        _userRepositoryMock.Setup(userRepository => userRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _materialRepositoryMock.Setup(materialRepository => materialRepository.GetWithDetailsByIdAsync(materialId, It.IsAny<CancellationToken>())).ReturnsAsync(material);

        _userMaterialRepositoryMock
            .Setup(userMaterialRepository => userMaterialRepository.GetByUserAndMaterialAsync(userId, materialId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserMaterial { User = user, Material = material });

        _userCourseRepositoryMock
            .Setup(userCourseRepository => userCourseRepository.GetByUserAndCourseAsync(userId, courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);

        _courseRepositoryMock
            .Setup(courseRepository => courseRepository.GetWithDetailsByIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(course);

        _userMaterialRepositoryMock
            .Setup(userMaterialRepository => userMaterialRepository.CountCompletedByUserForMaterialIdsAsync(userId, It.Is<int[]>(materialIds => materialIds.Length == 1), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _userCourseRepositoryMock
            .Setup(userCourseRepository => userCourseRepository.GetStatusByNameAsync("Completed", It.IsAny<CancellationToken>()))
            .ReturnsAsync(completed);

        _userCourseRepositoryMock.Setup(userCourseRepository => userCourseRepository.Update(enrollment));

        _skillRepositoryMock.Setup(skillRepository => skillRepository.GetByIdAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(skill);
        _userSkillRepositoryMock.Setup(userSkillRepository => userSkillRepository.GetByUserAndSkillAsync(userId, 7, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserSkill?)null);

        _userSkillRepositoryMock.Setup(userSkillRepository => userSkillRepository.AddAsync(It.Is<UserSkill>(userSkill =>
                userSkill.User == user && userSkill.Skill == skill && userSkill.Level == 1),
            It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        await _enrollmentService.MarkMaterialCompleteAsync(userId, materialId, CancellationToken.None);

        // Assert
        Assert.Equal("Completed", enrollment.CourseStatus.Name);
        Assert.Equal(BusinessRules.MaxProgressPercent, enrollment.ProgressPercent);

        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task MarkMaterialCompleteAsync_AffectedCourseBecomesComplete_IncrementsExistingSkills()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const int materialId = 200;
        const int courseId = 10;

        var user = new ApplicationUser { Id = userId };

        var course = CreateCourseWithMaterials(courseId, activeMaterialCount: 1);

        var skill = new Skill { Id = 7, Name = "LINQ" };
        course.CourseSkills = new List<CourseSkill>
        {
            new CourseSkill { CourseId = courseId, SkillId = 7, Skill = skill, RecordStatus = RecordStatus.Active }
        };

        var material = new VideoMaterial { Id = materialId, Title = "Video Material" };
        material.CourseMaterials = new List<CourseMaterial>
        {
            new CourseMaterial { CourseId = courseId, MaterialId = materialId, RecordStatus = RecordStatus.Active }
        };

        var inProgress = new CourseStatus { Id = 1, Name = "InProgress" };
        var completed = new CourseStatus { Id = 2, Name = "Completed" };

        var enrollment = new UserCourse
        {
            User = user,
            Course = course,
            CourseStatus = inProgress,
            ProgressPercent = 0
        };

        var existingUserSkill = new UserSkill { User = user, Skill = skill, Level = 3 };

        _userRepositoryMock.Setup(userRepository => userRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _materialRepositoryMock.Setup(materialRepository => materialRepository.GetWithDetailsByIdAsync(materialId, It.IsAny<CancellationToken>())).ReturnsAsync(material);

        _userMaterialRepositoryMock
            .Setup(userMaterialRepository => userMaterialRepository.GetByUserAndMaterialAsync(userId, materialId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserMaterial { User = user, Material = material });

        _userCourseRepositoryMock
            .Setup(userCourseRepository => userCourseRepository.GetByUserAndCourseAsync(userId, courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);

        _courseRepositoryMock
            .Setup(courseRepository => courseRepository.GetWithDetailsByIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(course);

        _userMaterialRepositoryMock
            .Setup(userMaterialRepository => userMaterialRepository.CountCompletedByUserForMaterialIdsAsync(userId, It.Is<int[]>(materialIds => materialIds.Length == 1), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _userCourseRepositoryMock
            .Setup(userCourseRepository => userCourseRepository.GetStatusByNameAsync("Completed", It.IsAny<CancellationToken>()))
            .ReturnsAsync(completed);

        _userCourseRepositoryMock.Setup(userCourseRepository => userCourseRepository.Update(enrollment));

        _skillRepositoryMock.Setup(skillRepository => skillRepository.GetByIdAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(skill);
        _userSkillRepositoryMock.Setup(userSkillRepository => userSkillRepository.GetByUserAndSkillAsync(userId, 7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUserSkill);
        _userSkillRepositoryMock.Setup(userSkillRepository => userSkillRepository.Update(existingUserSkill));

        // Act
        await _enrollmentService.MarkMaterialCompleteAsync(userId, materialId, CancellationToken.None);

        // Assert
        Assert.Equal(4, existingUserSkill.Level);
        Assert.Equal("Completed", enrollment.CourseStatus.Name);
        Assert.Equal(BusinessRules.MaxProgressPercent, enrollment.ProgressPercent);

        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
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