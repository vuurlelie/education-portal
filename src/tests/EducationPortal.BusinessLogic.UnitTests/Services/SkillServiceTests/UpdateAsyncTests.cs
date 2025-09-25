using EducationPortal.BusinessLogic.Abstractions;
using EducationPortal.BusinessLogic.DTOs.Skills;
using EducationPortal.BusinessLogic.Services;
using EducationPortal.DataAccess.Abstractions;
using EducationPortal.DataAccess.Entities;
using Moq;

namespace EducationPortal.BusinessLogic.UnitTests.Services.SkillServiceTests;

public sealed class UpdateAsyncTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new(MockBehavior.Strict);
    private readonly Mock<ISkillRepository> _skillRepositoryMock = new(MockBehavior.Strict);
    private readonly ISkillService _skillService;

    public UpdateAsyncTests()
    {
        _unitOfWorkMock.SetupGet(unitOfWork => unitOfWork.SkillRepository)
                      .Returns(_skillRepositoryMock.Object);
        _unitOfWorkMock.Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(1);

        _skillService = new SkillService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        _skillRepositoryMock
            .Setup(skillRepository => skillRepository.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Skill?)null);

        // Act + Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _skillService.UpdateAsync(10, new SkillEditDto { Name = "Name", Description = "Description" }, CancellationToken.None));

        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesNameAndDescription_AndSaves()
    {
        // Arrange
        var entity = new Skill { Id = 10, Name = "Old", Description = "OldDescription" };

        _skillRepositoryMock
            .Setup(skillRepository => skillRepository.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        _skillRepositoryMock
            .Setup(skillRepository => skillRepository.Update(entity));

        var edit = new SkillEditDto { Name = "NewName", Description = "NewDescription" };

        // Act
        await _skillService.UpdateAsync(10, edit, CancellationToken.None);

        // Assert
        Assert.Equal("NewName", entity.Name);
        Assert.Equal("NewDescription", entity.Description);

        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}