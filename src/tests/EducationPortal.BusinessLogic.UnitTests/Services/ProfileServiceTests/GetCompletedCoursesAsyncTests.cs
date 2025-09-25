using EducationPortal.BusinessLogic.Abstractions;
using EducationPortal.BusinessLogic.Services;
using EducationPortal.DataAccess.Abstractions;
using EducationPortal.DataAccess.Entities;
using Moq;

namespace EducationPortal.BusinessLogic.UnitTests.Services.ProfileServiceTests;

public sealed class GetCompletedCoursesAsyncTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserCourseRepository> _userCourseRepositoryMock;
    private readonly Mock<ICourseRepository> _courseRepositoryMock;

    private readonly IProfileService _profileService;

    public GetCompletedCoursesAsyncTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
        _userCourseRepositoryMock = new Mock<IUserCourseRepository>(MockBehavior.Strict);
        _courseRepositoryMock = new Mock<ICourseRepository>(MockBehavior.Strict);

        _unitOfWorkMock.SetupGet(unitOfWork => unitOfWork.UserCourseRepository).Returns(_userCourseRepositoryMock.Object);
        _unitOfWorkMock.SetupGet(unitOfWork => unitOfWork.CourseRepository).Returns(_courseRepositoryMock.Object);

        _profileService = new ProfileService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetCompletedCoursesAsync_NoCompleted_ReturnsEmpty()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userCourseRepositoryMock
            .Setup(userCourseRepository => userCourseRepository.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserCourse>());

        // Act
        var items = await _profileService.GetCompletedCoursesAsync(userId, CancellationToken.None);

        // Assert
        Assert.NotNull(items);
        Assert.Empty(items);
    }

    [Fact]
    public async Task GetCompletedCoursesAsync_FiltersActiveAndMaps()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var applicationUser = new ApplicationUser { Id = userId };

        var completedLinks = new List<UserCourse>
        {
            new()
            {
                UserId = userId, User = applicationUser,
                CourseId = 3, Course = new Course{ Id = 3, Name = "ASP.NET" },
                CourseStatus = new CourseStatus{ Id = 2, Name = "Completed" },
                ProgressPercent = 100, RecordStatus = RecordStatus.Active
            }
        };

        _userCourseRepositoryMock
            .Setup(userCourseRepository => userCourseRepository.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(completedLinks);

        _courseRepositoryMock
            .Setup(courseRepository => courseRepository.GetByIdsAsync(It.IsAny<IReadOnlyList<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Course> { new() { Id = 3, Name = "ASP.NET" } });

        // Act
        var result = await _profileService.GetCompletedCoursesAsync(userId, CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal(3, result[0].CourseId);
        Assert.Equal("ASP.NET", result[0].CourseName);
        Assert.Equal((byte)100, result[0].ProgressPercent);
    }
}