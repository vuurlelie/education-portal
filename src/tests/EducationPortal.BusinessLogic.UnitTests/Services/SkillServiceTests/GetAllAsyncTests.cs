using EducationPortal.BusinessLogic.Abstractions;
using EducationPortal.BusinessLogic.Services;
using EducationPortal.DataAccess.Abstractions;
using EducationPortal.DataAccess.Entities;
using Moq;

namespace EducationPortal.BusinessLogic.UnitTests.Services.SkillServiceTests;

public sealed class GetAllAsyncTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new(MockBehavior.Strict);
    private readonly Mock<ISkillRepository> _skillRepositoryMock = new(MockBehavior.Strict);
    private readonly ISkillService _skillService;

    public GetAllAsyncTests()
    {
        _unitOfWorkMock.SetupGet(unitOfWork => unitOfWork.SkillRepository)
                      .Returns(_skillRepositoryMock.Object);

        _skillService = new SkillService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_NoSkills_ReturnsEmptyList()
    {
        // Arrange
        _skillRepositoryMock
            .Setup(skillRepository => skillRepository.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Skill>());

        // Act
        var result = await _skillService.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_MapsItems()
    {
        // Arrange
        var skills = new List<Skill>
        {
            new() { Id = 1, Name = "LINQ",    Description = "Description" },
            new() { Id = 2, Name = "EF Core", Description = "Description" }
        };

        _skillRepositoryMock
            .Setup(skillRepository => skillRepository.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(skills);

        // Act
        var items = await _skillService.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.Collection(items,
            first =>
            {
                Assert.Equal(1, first.Id);
                Assert.Equal("LINQ", first.Name);
                Assert.Equal("Description", first.Description);
            },
            second =>
            {
                Assert.Equal(2, second.Id);
                Assert.Equal("EF Core", second.Name);
                Assert.Equal("Description", second.Description);
            });
    }
}