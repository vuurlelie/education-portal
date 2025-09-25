using EducationPortal.BusinessLogic.Abstractions;
using EducationPortal.BusinessLogic.DTOs.Materials;
using EducationPortal.BusinessLogic.Services;
using EducationPortal.DataAccess.Abstractions;
using EducationPortal.DataAccess.Entities;
using Moq;

namespace EducationPortal.BusinessLogic.UnitTests.Services.MaterialServiceTests;

public sealed class UpdateBookAsyncTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMaterialRepository> _materialRepositoryMock;
    private readonly IMaterialService _materialService;

    public UpdateBookAsyncTests()
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
    public async Task UpdateBookAsync_NotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        _materialRepositoryMock.Setup(materialRepository => materialRepository.GetByIdAsync(10, It.IsAny<CancellationToken>()))
                              .ReturnsAsync((Material?)null);

        // Act + Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _materialService.UpdateBookAsync(10, new BookMaterialEditDto { Title = "Title", Description = "Description" }, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateBookAsync_WrongType_ThrowsInvalidOperationException()
    {
        // Arrange
        _materialRepositoryMock.Setup(materialRepository => materialRepository.GetByIdAsync(10, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(new VideoMaterial { Id = 10, Title = "Title" });

        // Act + Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _materialService.UpdateBookAsync(10, new BookMaterialEditDto { Title = "Title", Description = "Description" }, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateBookAsync_UpdatesFields_AndSaves()
    {
        // Arrange
        var entity = new BookMaterial { Id = 10, Title = "OldTitle", Description = "OldDescription", Authors = "OldAuthor", Pages = 10, FormatId = 1, PublicationYear = 1999 };

        _materialRepositoryMock.Setup(materialRepository => materialRepository.GetByIdAsync(10, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(entity);
        _materialRepositoryMock.Setup(materialRepository => materialRepository.Update(entity));

        var edit = new BookMaterialEditDto
        {
            Title = "NewTitle",
            Description = "NewDescription",
            Authors = "NewAuthor",
            Pages = 321,
            FormatId = 7,
            PublicationYear = 2011
        };

        // Act
        await _materialService.UpdateBookAsync(10, edit, CancellationToken.None);

        // Assert
        Assert.Equal("NewTitle", entity.Title);
        Assert.Equal("NewDescription", entity.Description);
        Assert.Equal("NewAuthor", entity.Authors);
        Assert.Equal(321, entity.Pages);
        Assert.Equal(7, entity.FormatId);
        Assert.Equal(2011, entity.PublicationYear);

        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}