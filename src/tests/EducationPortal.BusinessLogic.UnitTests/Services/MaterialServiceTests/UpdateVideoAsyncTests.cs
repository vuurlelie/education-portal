using EducationPortal.BusinessLogic.Abstractions;
using EducationPortal.BusinessLogic.DTOs.Materials;
using EducationPortal.BusinessLogic.Services;
using EducationPortal.DataAccess.Abstractions;
using EducationPortal.DataAccess.Entities;
using Moq;

namespace EducationPortal.BusinessLogic.UnitTests.Services.MaterialServiceTests;

public sealed class UpdateVideoAsyncTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMaterialRepository> _materialRepositoryMock;
    private readonly IMaterialService _materialService;

    public UpdateVideoAsyncTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
        _materialRepositoryMock = new Mock<IMaterialRepository>(MockBehavior.Strict);

        _unitOfWorkMock.SetupGet(unitOfWork => unitOfWork.MaterialRepository)
                      .Returns(_materialRepositoryMock.Object);
        _unitOfWorkMock.Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(1);

        _materialService = new MaterialService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task UpdateVideoAsync_NotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        _materialRepositoryMock.Setup(materialRepository => materialRepository.GetByIdAsync(10, It.IsAny<CancellationToken>()))
                              .ReturnsAsync((Material?)null);

        // Act + Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _materialService.UpdateVideoAsync(10, new VideoMaterialEditDto { Title = "T", Description = "D" }, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateVideoAsync_WrongType_ThrowsInvalidOperationException()
    {
        // Arrange
        _materialRepositoryMock.Setup(materialRepository => materialRepository.GetByIdAsync(10, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(new BookMaterial { Id = 10, Title = "Title", Authors = "Tom" });

        // Act + Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _materialService.UpdateVideoAsync(10, new VideoMaterialEditDto { Title = "Title", Description = "Description" }, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateVideoAsync_UpdatesFields_AndSaves()
    {
        // Arrange
        var entity = new VideoMaterial { Id = 10, Title = "OldTitle", Description = "OldDescription", DurationSec = 10, HeightPx = 100, WidthPx = 100 };

        _materialRepositoryMock.Setup(materialRepository => materialRepository.GetByIdAsync(10, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(entity);
        _materialRepositoryMock.Setup(materialRepository => materialRepository.Update(entity));

        var edit = new VideoMaterialEditDto
        {
            Title = "NewTitle",
            Description = "NewDescription",
            DurationSeconds = 200,
            HeightPx = 1080,
            WidthPx = 1920
        };

        // Act
        await _materialService.UpdateVideoAsync(10, edit, CancellationToken.None);

        // Assert
        Assert.Equal("NewTitle", entity.Title);
        Assert.Equal("NewDescription", entity.Description);
        Assert.Equal(200, entity.DurationSec);
        Assert.Equal(1080, entity.HeightPx);
        Assert.Equal(1920, entity.WidthPx);

        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}