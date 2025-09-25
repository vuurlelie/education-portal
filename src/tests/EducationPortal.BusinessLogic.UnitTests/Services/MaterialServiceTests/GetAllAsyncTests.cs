using EducationPortal.BusinessLogic.Abstractions;
using EducationPortal.BusinessLogic.DTOs.Materials;
using EducationPortal.BusinessLogic.Services;
using EducationPortal.DataAccess.Abstractions;
using EducationPortal.DataAccess.Entities;
using Moq;

namespace EducationPortal.BusinessLogic.UnitTests.Services.MaterialServiceTests;

public sealed class GetAllAsyncTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMaterialRepository> _materialRepositoryMock;
    private readonly IMaterialService _materialService;

    public GetAllAsyncTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
        _materialRepositoryMock = new Mock<IMaterialRepository>(MockBehavior.Strict);

        _unitOfWorkMock.SetupGet(unitOfWork => unitOfWork.MaterialRepository).Returns(_materialRepositoryMock.Object);

        _materialService = new MaterialService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_NoMaterials_ReturnsEmptyList()
    {
        // Arrange
        _materialRepositoryMock
            .Setup(materialRepository => materialRepository.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Material>());

        // Act
        var items = await _materialService.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(items);
        Assert.Empty(items);
    }

    [Fact]
    public async Task GetAllAsync_MapsAllTypes()
    {
        // Arrange
        var materials = new List<Material>
        {
            new VideoMaterial { Id = 1, Title = "Video" },
            new BookMaterial { Id = 2, Title = "Book", Authors = "John", Pages = 100, FormatId = 1, PublicationYear = 2000 },
            new ArticleMaterial { Id = 3, Title = "Article", SourceUrl = "url", PublishedAt = DateOnly.FromDateTime(DateTime.UtcNow) }
        };

        _materialRepositoryMock
            .Setup(materialRepository => materialRepository.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(materials);

        // Act
        var items = await _materialService.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.Collection(items,
            first =>
            {
                Assert.Equal(1, first.Id);
                Assert.Equal("Video", first.Title);
                Assert.Equal(MaterialType.Video, first.Type);
            },
            second =>
            {
                Assert.Equal(2, second.Id);
                Assert.Equal("Book", second.Title);
                Assert.Equal(MaterialType.Book, second.Type);
            },
            third =>
            {
                Assert.Equal(3, third.Id);
                Assert.Equal("Article", third.Title);
                Assert.Equal(MaterialType.Article, third.Type);
            });
    }
}