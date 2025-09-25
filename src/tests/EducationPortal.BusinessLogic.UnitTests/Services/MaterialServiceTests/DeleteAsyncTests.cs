using EducationPortal.BusinessLogic.Abstractions;
using EducationPortal.BusinessLogic.Services;
using EducationPortal.DataAccess.Abstractions;
using Moq;

namespace EducationPortal.BusinessLogic.UnitTests.Services.MaterialServiceTests;

public sealed class DeleteAsyncTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMaterialRepository> _materialRepositoryMock;
    private readonly Mock<IUserMaterialRepository> _userMaterialRepositoryMock;
    private readonly IMaterialService _materialService;

    public DeleteAsyncTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
        _materialRepositoryMock = new Mock<IMaterialRepository>(MockBehavior.Strict);
        _userMaterialRepositoryMock = new Mock<IUserMaterialRepository>(MockBehavior.Strict);

        _unitOfWorkMock.SetupGet(unitOfWork => unitOfWork.MaterialRepository)
            .Returns(_materialRepositoryMock.Object);

        _unitOfWorkMock.SetupGet(unitOfWork => unitOfWork.UserMaterialRepository)
            .Returns(_userMaterialRepositoryMock.Object);

        _unitOfWorkMock.Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _materialService = new MaterialService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task DeleteAsync_WhenNoUserCompletions_DeletesAndSaves_ReturnsTrue()
    {
        // Arrange
        const int materialId = 10;

        _userMaterialRepositoryMock
            .Setup(userMaterialRepository => userMaterialRepository.AnyByMaterialIdAsync(materialId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _materialRepositoryMock
            .Setup(materialRepository => materialRepository.DeleteByIdAsync(materialId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var deleted = await _materialService.DeleteAsync(materialId, CancellationToken.None);

        // Assert
        Assert.True(deleted);
        _userMaterialRepositoryMock.Verify(userMaterialRepository => userMaterialRepository.AnyByMaterialIdAsync(materialId, It.IsAny<CancellationToken>()), Times.Once);
        _materialRepositoryMock.Verify(materialRepository => materialRepository.DeleteByIdAsync(materialId, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotFound_ReturnsFalse_AndDoesNotSave()
    {
        // Arrange
        const int materialId = 10;

        _userMaterialRepositoryMock
            .Setup(userMaterialRepository => userMaterialRepository.AnyByMaterialIdAsync(materialId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _materialRepositoryMock
            .Setup(materialRepository => materialRepository.DeleteByIdAsync(materialId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var deleted = await _materialService.DeleteAsync(materialId, CancellationToken.None);

        // Assert
        Assert.False(deleted);
        _userMaterialRepositoryMock.Verify(userMaterialRepository => userMaterialRepository.AnyByMaterialIdAsync(materialId, It.IsAny<CancellationToken>()), Times.Once);
        _materialRepositoryMock.Verify(materialRepository => materialRepository.DeleteByIdAsync(materialId, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenHasUserCompletions_Throws_AndDoesNotDeleteOrSave()
    {
        // Arrange
        const int materialId = 10;

        _userMaterialRepositoryMock
            .Setup(userMaterialRepository => userMaterialRepository.AnyByMaterialIdAsync(materialId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act + Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _materialService.DeleteAsync(materialId, CancellationToken.None));

        Assert.Contains("material", exception.Message, StringComparison.OrdinalIgnoreCase);

        _userMaterialRepositoryMock.Verify(userMaterialRepository => userMaterialRepository.AnyByMaterialIdAsync(materialId, It.IsAny<CancellationToken>()), Times.Once);
        _materialRepositoryMock.Verify(materialRepository => materialRepository.DeleteByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}