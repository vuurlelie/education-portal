using EducationPortal.BusinessLogic.Abstractions;
using EducationPortal.BusinessLogic.Services;
using EducationPortal.DataAccess.Abstractions;
using EducationPortal.DataAccess.Entities;
using Moq;

namespace EducationPortal.BusinessLogic.UnitTests.Services.ProfileServiceTests;

public sealed class GetProfileAsyncTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUserCourseRepository> _userCourseRepositoryMock;
    private readonly Mock<IUserSkillRepository> _userSkillRepositoryMock;

    private readonly IProfileService _profileService;

    public GetProfileAsyncTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
        _userRepositoryMock = new Mock<IUserRepository>(MockBehavior.Strict);
        _userCourseRepositoryMock = new Mock<IUserCourseRepository>(MockBehavior.Strict);
        _userSkillRepositoryMock = new Mock<IUserSkillRepository>(MockBehavior.Strict);

        _unitOfWorkMock.SetupGet(unitOfWork => unitOfWork.UserRepository).Returns(_userRepositoryMock.Object);
        _unitOfWorkMock.SetupGet(unitOfWork => unitOfWork.UserCourseRepository).Returns(_userCourseRepositoryMock.Object);
        _unitOfWorkMock.SetupGet(unitOfWork => unitOfWork.UserSkillRepository).Returns(_userSkillRepositoryMock.Object);

        _profileService = new ProfileService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetProfileAsync_UserNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userRepositoryMock
            .Setup(userRepository => userRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act + Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _profileService.GetProfileAsync(userId, CancellationToken.None));
    }

    [Fact]
    public async Task GetProfileAsync_Found_ReturnsCounts()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var applicationUser = new ApplicationUser
        {
            Id = userId,
            FullName = "Test User",
            Email = "test@example.com",
            CreatedAt = DateTime.UtcNow
        };

        var userCourses = new List<UserCourse>
        {
            new()
            {
                UserId = userId, User = applicationUser,
                CourseId = 1, Course = new Course{ Id = 1, Name = "C1" },
                CourseStatus = new CourseStatus{ Id = 2, Name = "Completed" },
                ProgressPercent = 100, RecordStatus = RecordStatus.Active
            },
            new()
            {
                UserId = userId, User = applicationUser,
                CourseId = 2, Course = new Course{ Id = 2, Name = "C2" },
                CourseStatus = new CourseStatus{ Id = 1, Name = "InProgress" },
                ProgressPercent = 50, RecordStatus = RecordStatus.Active
            },
            new()
            {
                UserId = userId, User = applicationUser,
                CourseId = 3, Course = new Course{ Id = 3, Name = "C3" },
                CourseStatus = new CourseStatus{ Id = 1, Name = "InProgress" },
                ProgressPercent = 0, RecordStatus = RecordStatus.Deleted
            }
        };

        var userSkills = new List<UserSkill>
        {
            new()
            {
                UserId = userId, User = applicationUser,
                SkillId = 8, Skill = new Skill{ Id = 8, Name = "LINQ" },
                Level = 2, RecordStatus = RecordStatus.Active
            },
            new()
            {
                UserId = userId, User = applicationUser,
                SkillId = 9, Skill = new Skill{ Id = 9, Name = "Old" },
                Level = 1, RecordStatus = RecordStatus.Deleted
            }
        };

        _userRepositoryMock
            .Setup(userRepository => userRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(applicationUser);

        _userCourseRepositoryMock
            .Setup(userCourseRepository => userCourseRepository.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userCourses);

        _userSkillRepositoryMock
            .Setup(userSkillRepository => userSkillRepository.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userSkills);

        // Act
        var userProfile = await _profileService.GetProfileAsync(userId, CancellationToken.None);

        // Assert
        Assert.Equal(userId, userProfile.UserId);
        Assert.Equal(1, userProfile.CompletedCoursesCount);
        Assert.Equal(1, userProfile.InProgressCoursesCount);
        Assert.Equal(1, userProfile.SkillsCount);
    }
}