using EducationPortal.BusinessLogic.Abstractions;
using EducationPortal.BusinessLogic.DTOs.Materials;
using EducationPortal.BusinessLogic.Services;
using EducationPortal.DataAccess.Abstractions;
using EducationPortal.DataAccess.Entities;
using Moq;

namespace EducationPortal.BusinessLogic.UnitTests.Services.MaterialServiceTests;

public sealed class GetDetailsAsyncTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMaterialRepository> _materialRepositoryMock;
    private readonly IMaterialService _materialService;

    public GetDetailsAsyncTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
        _materialRepositoryMock = new Mock<IMaterialRepository>(MockBehavior.Strict);

        _unitOfWorkMock.SetupGet(unitOfWork => unitOfWork.MaterialRepository)
                      .Returns(_materialRepositoryMock.Object);

        _materialService = new MaterialService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetDetailsAsync_NotFound_ReturnsNull()
    {
        // Arrange
        _materialRepositoryMock.Setup(materialRepository => materialRepository.GetWithDetailsByIdAsync(999, It.IsAny<CancellationToken>()))
                              .ReturnsAsync((Material?)null);

        // Act
        var details = await _materialService.GetDetailsAsync(999, CancellationToken.None);

        // Assert
        Assert.Null(details);
    }

    [Fact]
    public async Task GetDetailsAsync_FoundBook_MapsFields()
    {
        // Arrange
        var entity = new BookMaterial
        {
            Id = 10,
            Title = "Clean Code",
            Description = "desc",
            Authors = "R. Martin",
            Pages = 400,
            FormatId = 2,
            Format = new BookFormat { Id = 2, Name = "Paperback" },
            PublicationYear = 2008
        };

        _materialRepositoryMock.Setup(materialRepository => materialRepository.GetWithDetailsByIdAsync(10, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(entity);

        // Act
        var details = await _materialService.GetDetailsAsync(10, CancellationToken.None);

        // Assert
        Assert.NotNull(details);
        Assert.Equal(10, details!.Id);
        Assert.Equal("Clean Code", details.Title);
        Assert.Equal("desc", details.Description);
        Assert.Equal(MaterialType.Book, details.Type);
        Assert.Equal("R. Martin", details.Authors);
        Assert.Equal(400, details.Pages);
        Assert.Equal(2, details.FormatId);
        Assert.Equal("Paperback", details.FormatName);
        Assert.Equal(2008, details.PublicationYear);
    }
}