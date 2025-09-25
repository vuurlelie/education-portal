using EducationPortal.BusinessLogic.Abstractions;
using EducationPortal.BusinessLogic.Services;
using EducationPortal.DataAccess.Abstractions;
using Moq;

namespace EducationPortal.BusinessLogic.UnitTests.Services.SkillServiceTests;

public sealed class DeleteAsyncTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new(MockBehavior.Strict);
    private readonly Mock<ISkillRepository> _skillRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IUserSkillRepository> _userSkillRepositoryMock = new(MockBehavior.Strict);
    private readonly ISkillService _skillService;

    public DeleteAsyncTests()
    {
        _unitOfWorkMock.SetupGet(unitOfWork => unitOfWork.SkillRepository)
                       .Returns(_skillRepositoryMock.Object);

        _unitOfWorkMock.SetupGet(unitOfWork => unitOfWork.UserSkillRepository)
                       .Returns(_userSkillRepositoryMock.Object);

        _unitOfWorkMock.Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(1);

        _skillService = new SkillService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task DeleteAsync_WhenNoUserHasSkill_DeletesAndSaves_ReturnsTrue()
    {
        // Arrange
        const int skillId = 10;

        _userSkillRepositoryMock
            .Setup(userSkillRepository => userSkillRepository.AnyBySkillIdAsync(skillId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _skillRepositoryMock
            .Setup(skillRepository => skillRepository.DeleteByIdAsync(skillId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var deleted = await _skillService.DeleteAsync(skillId, CancellationToken.None);

        // Assert
        Assert.True(deleted);
        _userSkillRepositoryMock.Verify(userSkillRepository => userSkillRepository.AnyBySkillIdAsync(skillId, It.IsAny<CancellationToken>()), Times.Once);
        _skillRepositoryMock.Verify(skillRepository => skillRepository.DeleteByIdAsync(skillId, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenSkillNotFound_ReturnsFalse_AndDoesNotSave()
    {
        // Arrange
        const int skillId = 10;

        _userSkillRepositoryMock
            .Setup(userSkillRepository => userSkillRepository.AnyBySkillIdAsync(skillId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _skillRepositoryMock
            .Setup(skillRepository => skillRepository.DeleteByIdAsync(skillId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var deleted = await _skillService.DeleteAsync(skillId, CancellationToken.None);

        // Assert
        Assert.False(deleted);
        _userSkillRepositoryMock.Verify(userSkillRepository => userSkillRepository.AnyBySkillIdAsync(skillId, It.IsAny<CancellationToken>()), Times.Once);
        _skillRepositoryMock.Verify(skillRepository => skillRepository.DeleteByIdAsync(skillId, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenSkillIsAwarded_ThrowsInvalidOperationException_AndDoesNotDeleteOrSave()
    {
        // Arrange
        const int skillId = 10;

        _userSkillRepositoryMock
            .Setup(userSkillRepository => userSkillRepository.AnyBySkillIdAsync(skillId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act + Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _skillService.DeleteAsync(skillId, CancellationToken.None));

        Assert.Contains("skill", exception.Message, StringComparison.OrdinalIgnoreCase);

        _userSkillRepositoryMock.Verify(userSkillRepository => userSkillRepository.AnyBySkillIdAsync(skillId, It.IsAny<CancellationToken>()), Times.Once);
        _skillRepositoryMock.Verify(skillRepository => skillRepository.DeleteByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}