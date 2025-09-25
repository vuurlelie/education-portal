using EducationPortal.BusinessLogic.Abstractions;
using EducationPortal.BusinessLogic.Services;
using EducationPortal.DataAccess.Abstractions;
using EducationPortal.DataAccess.Entities;
using Moq;

namespace EducationPortal.BusinessLogic.UnitTests.Services.ProfileServiceTests;

public sealed class GetSkillsAsyncTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserSkillRepository> _userSkillRepositoryMock;
    private readonly Mock<ISkillRepository> _skillRepositoryMock;

    private readonly IProfileService _profileService;

    public GetSkillsAsyncTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
        _userSkillRepositoryMock = new Mock<IUserSkillRepository>(MockBehavior.Strict);
        _skillRepositoryMock = new Mock<ISkillRepository>(MockBehavior.Strict);

        _unitOfWorkMock.SetupGet(unitOfWork => unitOfWork.UserSkillRepository).Returns(_userSkillRepositoryMock.Object);
        _unitOfWorkMock.SetupGet(unitOfWork => unitOfWork.SkillRepository).Returns(_skillRepositoryMock.Object);

        _profileService = new ProfileService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetSkillsAsync_NoSkills_ReturnsEmpty()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userSkillRepositoryMock
            .Setup(userSkillRepository => userSkillRepository.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserSkill>());

        // Act
        var items = await _profileService.GetSkillsAsync(userId, CancellationToken.None);

        // Assert
        Assert.NotNull(items);
        Assert.Empty(items);
    }

    [Fact]
    public async Task GetSkillsAsync_FiltersActiveAndMapsSortedByName()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var applicationUser = new ApplicationUser { Id = userId };

        var activeLinks = new List<UserSkill>
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
                SkillId = 9, Skill = new Skill{ Id = 9, Name = "EF Core" },
                Level = 1, RecordStatus = RecordStatus.Active
            },
            new()
            {
                UserId = userId, User = applicationUser,
                SkillId = 99, Skill = new Skill{ Id = 99, Name = "Old" },
                Level = 5, RecordStatus = RecordStatus.Deleted
            }
        };

        _userSkillRepositoryMock
            .Setup(userSkillRepository => userSkillRepository.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeLinks);

        _skillRepositoryMock
            .Setup(skillRepository => skillRepository.GetByIdsAsync(It.IsAny<IReadOnlyList<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Skill>
            {
                new(){ Id = 8, Name = "LINQ" },
                new(){ Id = 9, Name = "EF Core" }
            });

        // Act
        var result = await _profileService.GetSkillsAsync(userId, CancellationToken.None);

        // Assert
        Assert.Collection(result,
            first =>
            {
                Assert.Equal(9, first.SkillId);
                Assert.Equal("EF Core", first.SkillName);
                Assert.Equal(1, first.Level);
            },
            second =>
            {
                Assert.Equal(8, second.SkillId);
                Assert.Equal("LINQ", second.SkillName);
                Assert.Equal(2, second.Level);
            });
    }
}