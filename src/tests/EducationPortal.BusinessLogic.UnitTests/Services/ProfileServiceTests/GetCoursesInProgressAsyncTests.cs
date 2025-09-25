using EducationPortal.BusinessLogic.Abstractions;
using EducationPortal.BusinessLogic.Services;
using EducationPortal.DataAccess.Abstractions;
using EducationPortal.DataAccess.Entities;
using Moq;

namespace EducationPortal.BusinessLogic.UnitTests.Services.ProfileServiceTests;

public sealed class GetCoursesInProgressAsyncTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserCourseRepository> _userCourseRepositoryMock;
    private readonly Mock<ICourseRepository> _courseRepositoryMock;

    private readonly IProfileService _profileService;

    public GetCoursesInProgressAsyncTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
        _userCourseRepositoryMock = new Mock<IUserCourseRepository>(MockBehavior.Strict);
        _courseRepositoryMock = new Mock<ICourseRepository>(MockBehavior.Strict);

        _unitOfWorkMock.SetupGet(unitOfWork => unitOfWork.UserCourseRepository).Returns(_userCourseRepositoryMock.Object);
        _unitOfWorkMock.SetupGet(unitOfWork => unitOfWork.CourseRepository).Returns(_courseRepositoryMock.Object);

        _profileService = new ProfileService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetCoursesInProgressAsync_NoInProgress_ReturnsEmpty()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userCourseRepositoryMock
            .Setup(userCourseRepository => userCourseRepository.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserCourse>());

        // Act
        var items = await _profileService.GetCoursesInProgressAsync(userId, CancellationToken.None);

        // Assert
        Assert.NotNull(items);
        Assert.Empty(items);
    }

    [Fact]
    public async Task GetCoursesInProgressAsync_FiltersActiveAndMapsSorted()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var applicationUser = new ApplicationUser { Id = userId };

        var inProgressLinks = new List<UserCourse>
        {
            new()
            {
                UserId = userId, User = applicationUser,
                CourseId = 1, Course = new Course{ Id = 1, Name = ".NET" },
                CourseStatus = new CourseStatus{ Id = 1, Name = "InProgress" },
                ProgressPercent = 70, RecordStatus = RecordStatus.Active
            },
            new()
            {
                UserId = userId, User = applicationUser,
                CourseId = 2, Course = new Course{ Id = 2, Name = "C#" },
                CourseStatus = new CourseStatus{ Id = 1, Name = "InProgress" },
                ProgressPercent = 50, RecordStatus = RecordStatus.Active
            },
            new()
            {
                UserId = userId, User = applicationUser,
                CourseId = 99, Course = new Course{ Id = 99, Name = "Old" },
                CourseStatus = new CourseStatus{ Id = 1, Name = "InProgress" },
                ProgressPercent = 10, RecordStatus = RecordStatus.Deleted
            }
        };

        _userCourseRepositoryMock
            .Setup(userCourseRepository => userCourseRepository.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inProgressLinks);

        _courseRepositoryMock
            .Setup(courseRepository => courseRepository.GetByIdsAsync(It.IsAny<IReadOnlyList<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Course>
            {
                new(){ Id = 1, Name = ".NET" },
                new(){ Id = 2, Name = "C#" }
            });

        // Act
        var result = await _profileService.GetCoursesInProgressAsync(userId, CancellationToken.None);

        // Assert
        Assert.Collection(result,
            first =>
            {
                Assert.Equal(1, first.CourseId);
                Assert.Equal(".NET", first.CourseName);
                Assert.Equal((byte)70, first.ProgressPercent);
            },
            second =>
            {
                Assert.Equal(2, second.CourseId);
                Assert.Equal("C#", second.CourseName);
                Assert.Equal((byte)50, second.ProgressPercent);
            });
    }
}