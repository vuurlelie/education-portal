using EducationPortal.BusinessLogic.Abstractions;
using EducationPortal.BusinessLogic.DTOs.Materials;
using EducationPortal.BusinessLogic.Services;
using EducationPortal.DataAccess.Abstractions;
using EducationPortal.DataAccess.Entities;
using Moq;

namespace EducationPortal.BusinessLogic.UnitTests.Services.MaterialServiceTests;

public sealed class CreateBookAsyncTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMaterialRepository> _materialRepositoryMock;
    private readonly IMaterialService _materialService;

    public CreateBookAsyncTests()
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
    public async Task CreateBookAsync_Persists_AndReturnsId()
    {
        // Arrange
        var generatedId = 456;
        BookMaterial? capturedEntity = null;

        _materialRepositoryMock
            .Setup(materialRepository => materialRepository.AddAsync(It.IsAny<Material>(), It.IsAny<CancellationToken>()))
            .Callback<Material, CancellationToken>((material, _) =>
            {
                capturedEntity = Assert.IsType<BookMaterial>(material);
                capturedEntity.Id = generatedId;
            })
            .Returns(Task.CompletedTask);

        var createDto = new BookMaterialCreateDto
        {
            Title = "Book",
            Description = "Description",
            Authors = "Jane",
            Pages = 321,
            FormatId = 2,
            PublicationYear = 2010
        };

        // Act
        var id = await _materialService.CreateBookAsync(createDto, CancellationToken.None);

        // Assert
        Assert.Equal(generatedId, id);
        Assert.NotNull(capturedEntity);
        Assert.Equal("Book", capturedEntity!.Title);
        Assert.Equal("Description", capturedEntity.Description);
        Assert.Equal("Jane", capturedEntity.Authors);
        Assert.Equal(321, capturedEntity.Pages);
        Assert.Equal(2, capturedEntity.FormatId);
        Assert.Equal(2010, capturedEntity.PublicationYear);

        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}