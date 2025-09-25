using EducationPortal.BusinessLogic.Abstractions;
using EducationPortal.BusinessLogic.Services;
using EducationPortal.DataAccess.Abstractions;
using EducationPortal.DataAccess.Entities;
using Moq;

namespace EducationPortal.BusinessLogic.UnitTests.Services.SkillServiceTests;

public sealed class GetDetailsAsyncTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new(MockBehavior.Strict);
    private readonly Mock<ISkillRepository> _skillRepositoryMock = new(MockBehavior.Strict);
    private readonly ISkillService _skillService;

    public GetDetailsAsyncTests()
    {
        _unitOfWorkMock.SetupGet(unitOfWork => unitOfWork.SkillRepository)
                      .Returns(_skillRepositoryMock.Object);

        _skillService = new SkillService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetDetailsAsync_NotFound_ReturnsNull()
    {
        // Arrange
        _skillRepositoryMock
            .Setup(skillRepository => skillRepository.GetWithDetailsByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Skill?)null);

        // Act
        var details = await _skillService.GetDetailsAsync(999, CancellationToken.None);

        // Assert
        Assert.Null(details);
    }

    [Fact]
    public async Task GetDetailsAsync_Found_ReturnsDtoWithActiveDistinctCourseIds()
    {
        // Arrange
        var skill = new Skill
        {
            Id = 10,
            Name = "ASP.NET",
            Description = "web",
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

        // Act
        var details = await _skillService.GetDetailsAsync(10, CancellationToken.None);

        // Assert
        Assert.NotNull(details);
        Assert.Equal(10, details!.Id);
        Assert.Equal("ASP.NET", details.Name);
        Assert.Equal("web", details.Description);
        Assert.Equal(new[] { 1, 2 }, details.AssignedCourseIds);
    }
}