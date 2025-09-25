using EducationPortal.BusinessLogic.Abstractions;
using EducationPortal.BusinessLogic.DTOs.Skills;
using EducationPortal.BusinessLogic.Services;
using EducationPortal.DataAccess.Abstractions;
using EducationPortal.DataAccess.Entities;
using Moq;

namespace EducationPortal.BusinessLogic.UnitTests.Services.SkillServiceTests;

public sealed class CreateAsyncTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new(MockBehavior.Strict);
    private readonly Mock<ISkillRepository> _skillRepositoryMock = new(MockBehavior.Strict);
    private readonly ISkillService _skillService;

    public CreateAsyncTests()
    {
        _unitOfWorkMock.SetupGet(unitOfWork => unitOfWork.SkillRepository)
                      .Returns(_skillRepositoryMock.Object);
        _unitOfWorkMock.Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(1);

        _skillService = new SkillService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task CreateAsync_PersistsSkill_AndReturnsGeneratedId()
    {
        // Arrange
        var generatedId = 123;

        _skillRepositoryMock
            .Setup(skillRepository => skillRepository.AddAsync(It.IsAny<Skill>(), It.IsAny<CancellationToken>()))
            .Callback<Skill, CancellationToken>((entity, _) => entity.Id = generatedId)
            .Returns(Task.CompletedTask);

        var create = new SkillCreateDto { Name = "NewSkill", Description = "Description" };

        // Act
        var returnedId = await _skillService.CreateAsync(create, CancellationToken.None);

        // Assert
        Assert.Equal(generatedId, returnedId);
        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}