using EducationPortal.BusinessLogic.Abstractions;
using EducationPortal.BusinessLogic.DTOs.Materials;
using EducationPortal.BusinessLogic.Services;
using EducationPortal.DataAccess.Abstractions;
using EducationPortal.DataAccess.Entities;
using Moq;

namespace EducationPortal.BusinessLogic.UnitTests.Services.MaterialServiceTests;

public sealed class CreateVideoAsyncTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMaterialRepository> _materialRepositoryMock;
    private readonly IMaterialService _materialService;

    public CreateVideoAsyncTests()
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
    public async Task CreateVideoAsync_Persists_AndReturnsId()
    {
        // Arrange
        var generatedId = 123;
        VideoMaterial? capturedEntity = null;

        _materialRepositoryMock
            .Setup(materialRepository => materialRepository.AddAsync(It.IsAny<Material>(), It.IsAny<CancellationToken>()))
            .Callback<Material, CancellationToken>((material, _) =>
            {
                capturedEntity = Assert.IsType<VideoMaterial>(material);
                capturedEntity.Id = generatedId;
            })
            .Returns(Task.CompletedTask);

        var createDto = new VideoMaterialCreateDto
        {
            Title = "New Video",
            Description = "d",
            DurationSeconds = 120,
            HeightPx = 720,
            WidthPx = 1280
        };

        // Act
        var id = await _materialService.CreateVideoAsync(createDto, CancellationToken.None);

        // Assert
        Assert.Equal(generatedId, id);
        Assert.NotNull(capturedEntity);
        Assert.Equal("New Video", capturedEntity!.Title);
        Assert.Equal("d", capturedEntity.Description);
        Assert.Equal(120, capturedEntity.DurationSec);
        Assert.Equal(720, capturedEntity.HeightPx);
        Assert.Equal(1280, capturedEntity.WidthPx);

        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}