using EducationPortal.BusinessLogic.Abstractions;
using EducationPortal.BusinessLogic.Services;
using EducationPortal.DataAccess.Abstractions;
using Moq;

namespace EducationPortal.BusinessLogic.UnitTests.Services.EnrollmentServiceTests;

public sealed class GetUserCompletedMaterialIdsAsyncTests
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

    public GetUserCompletedMaterialIdsAsyncTests()
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
    public async Task GetUserCompletedMaterialIdsAsync_NoCompleted_ReturnsEmptySet()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userMaterialRepositoryMock
            .Setup(userMaterialRepository => userMaterialRepository.GetCompletedMaterialIdsByUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<int>());

        // Act
        var ids = await _enrollmentService.GetUserCompletedMaterialIdsAsync(userId, CancellationToken.None);

        // Assert
        Assert.NotNull(ids);
        Assert.Empty(ids);
        VerifyNoSave();
    }

    [Fact]
    public async Task GetUserCompletedMaterialIdsAsync_CompletedExist_ReturnsDistinctIdsAsSet()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userMaterialRepositoryMock
            .Setup(userMaterialRepository => userMaterialRepository.GetCompletedMaterialIdsByUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<int> { 1, 1, 2, 3, 2 });

        // Act
        var ids = await _enrollmentService.GetUserCompletedMaterialIdsAsync(userId, CancellationToken.None);

        // Assert
        Assert.IsType<HashSet<int>>(ids);
        Assert.Equal(3, ids.Count);
        Assert.Contains(1, ids);
        Assert.Contains(2, ids);
        Assert.Contains(3, ids);
        VerifyNoSave();
    }

    private void VerifyNoSave()
    {
        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}