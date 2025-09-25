using EducationPortal.BusinessLogic.Abstractions;
using EducationPortal.BusinessLogic.DTOs.Materials;
using EducationPortal.BusinessLogic.Services;
using EducationPortal.DataAccess.Abstractions;
using EducationPortal.DataAccess.Entities;
using Moq;

namespace EducationPortal.BusinessLogic.UnitTests.Services.MaterialServiceTests;

public sealed class CreateArticleAsyncTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMaterialRepository> _materialRepositoryMock;
    private readonly IMaterialService _materialService;

    public CreateArticleAsyncTests()
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
    public async Task CreateArticleAsync_Persists_AndReturnsId()
    {
        // Arrange
        var generatedId = 789;
        ArticleMaterial? capturedEntity = null;

        _materialRepositoryMock
            .Setup(materialRepository => materialRepository.AddAsync(It.IsAny<Material>(), It.IsAny<CancellationToken>()))
            .Callback<Material, CancellationToken>((material, _) =>
            {
                capturedEntity = Assert.IsType<ArticleMaterial>(material);
                capturedEntity.Id = generatedId;
            })
            .Returns(Task.CompletedTask);

        var createDto = new ArticleMaterialCreateDto
        {
            Title = "Article",
            Description = "d",
            PublishedAt = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            SourceUrl = "https://example.com"
        };

        // Act
        var id = await _materialService.CreateArticleAsync(createDto, CancellationToken.None);

        // Assert
        Assert.Equal(generatedId, id);
        Assert.NotNull(capturedEntity);
        Assert.Equal("Article", capturedEntity!.Title);
        Assert.Equal("d", capturedEntity.Description);
        Assert.Equal(createDto.PublishedAt, capturedEntity.PublishedAt);
        Assert.Equal("https://example.com", capturedEntity.SourceUrl);

        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}