using EducationPortal.BusinessLogic.Abstractions;
using EducationPortal.BusinessLogic.DTOs.Materials;
using EducationPortal.BusinessLogic.Services;
using EducationPortal.DataAccess.Abstractions;
using EducationPortal.DataAccess.Entities;
using Moq;

namespace EducationPortal.BusinessLogic.UnitTests.Services.MaterialServiceTests;

public sealed class UpdateArticleAsyncTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMaterialRepository> _materialRepositoryMock;
    private readonly IMaterialService _materialService;

    public UpdateArticleAsyncTests()
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
    public async Task UpdateArticleAsync_NotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        _materialRepositoryMock.Setup(materialRepository => materialRepository.GetByIdAsync(10, It.IsAny<CancellationToken>()))
                              .ReturnsAsync((Material?)null);

        // Act + Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _materialService.UpdateArticleAsync(10, new ArticleMaterialEditDto { Title = "Title", Description = "Description" }, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateArticleAsync_WrongType_ThrowsInvalidOperationException()
    {
        // Arrange
        _materialRepositoryMock.Setup(materialRepository => materialRepository.GetByIdAsync(10, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(new VideoMaterial { Id = 10, Title = "Title" });

        // Act + Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _materialService.UpdateArticleAsync(10, new ArticleMaterialEditDto { Title = "Title", Description = "Description" }, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateArticleAsync_UpdatesFields_AndSaves()
    {
        // Arrange
        var entity = new ArticleMaterial { Id = 10, Title = "OldTitle", Description = "OldDescription", SourceUrl = "OldUrl", PublishedAt = DateOnly.FromDateTime(DateTime.UtcNow.Date) };

        _materialRepositoryMock.Setup(materialRepository => materialRepository.GetByIdAsync(10, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(entity);
        _materialRepositoryMock.Setup(materialRepository => materialRepository.Update(entity));

        var edit = new ArticleMaterialEditDto
        {
            Title = "NewTitle",
            Description = "NewDescription",
            SourceUrl = "https://example.com/new",
            PublishedAt = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-1))
        };

        // Act
        await _materialService.UpdateArticleAsync(10, edit, CancellationToken.None);

        // Assert
        Assert.Equal("NewTitle", entity.Title);
        Assert.Equal("NewDescription", entity.Description);
        Assert.Equal("https://example.com/new", entity.SourceUrl);
        Assert.Equal(edit.PublishedAt, entity.PublishedAt);

        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}