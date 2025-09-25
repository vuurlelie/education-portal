using EducationPortal.BusinessLogic.Abstractions;
using EducationPortal.BusinessLogic.Services;
using EducationPortal.DataAccess.Abstractions;
using EducationPortal.DataAccess.Entities;
using Moq;

namespace EducationPortal.BusinessLogic.UnitTests.Services.SkillServiceTests;

public sealed class GetAssignedCourseIdsAsyncTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new(MockBehavior.Strict);
    private readonly Mock<ISkillRepository> _skillRepositoryMock = new(MockBehavior.Strict);
    private readonly ISkillService _skillService;

    public GetAssignedCourseIdsAsyncTests()
    {
        _unitOfWorkMock.SetupGet(unitOfWork => unitOfWork.SkillRepository)
                      .Returns(_skillRepositoryMock.Object);

        _skillService = new SkillService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetAssignedCourseIdsAsync_NotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        _skillRepositoryMock
            .Setup(skillRepository => skillRepository.GetWithDetailsByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Skill?)null);
        
        // Act + Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _skillService.GetAssignedCourseIdsAsync(999, CancellationToken.None));
    }

    [Fact]
    public async Task GetAssignedCourseIdsAsync_ReturnsActiveDistinctCourseIds()
    {
        // Arrange
        var skill = new Skill
        {
            Id = 10,
            Name = "ASP.NET",
            CourseSkills = new List<CourseSkill>
            {
                new() { SkillId = 10, CourseId = 1, RecordStatus = RecordStatus.Active },
                new() { SkillId = 10, CourseId = 2, RecordStatus = RecordStatus.Active },
                new() { SkillId = 10, CourseId = 2, RecordStatus = RecordStatus.Active },
                new() { SkillId = 10, CourseId = 3, RecordStatus = RecordStatus.Deleted }
            }
        };

        _skillRepositoryMock
            .Setup(skillRepository => skillRepository.GetWithDetailsByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(skill);

        // Att
        var ids = await _skillService.GetAssignedCourseIdsAsync(10, CancellationToken.None);

        // Assert
        Assert.Equal(new[] { 1, 2 }, ids);
    }

    [Fact]
    public async Task GetAssignedCourseIdsAsync_NoActiveLinks_ReturnsEmpty()
    {
        // Arrange
        var skill = new Skill
        {
            Id = 10,
            Name = "Empty",
            CourseSkills = new List<CourseSkill>
            {
                new() { SkillId = 10, CourseId = 5, RecordStatus = RecordStatus.Deleted }
            }
        };

        _skillRepositoryMock
            .Setup(skillRepository => skillRepository.GetWithDetailsByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(skill);

        // Act
        var ids = await _skillService.GetAssignedCourseIdsAsync(10, CancellationToken.None);

        // Assert
        Assert.NotNull(ids);
        Assert.Empty(ids);
    }
}