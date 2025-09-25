using EducationPortal.BusinessLogic.Abstractions;
using EducationPortal.BusinessLogic.Services;
using EducationPortal.DataAccess.Abstractions;
using EducationPortal.DataAccess.Entities;
using Moq;

namespace EducationPortal.BusinessLogic.UnitTests.Services.MaterialServiceTests;

public sealed class GetBookFormatsAsyncTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMaterialRepository> _materialRepositoryMock;
    private readonly IMaterialService _materialService;

    public GetBookFormatsAsyncTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
        _materialRepositoryMock = new Mock<IMaterialRepository>(MockBehavior.Strict);

        _unitOfWorkMock.SetupGet(unitOfWork => unitOfWork.MaterialRepository)
                      .Returns(_materialRepositoryMock.Object);

        _materialService = new MaterialService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetBookFormatsAsync_ReturnsIdNamePairs()
    {
        // Arrange
        var formats = new List<BookFormat>
        {
            new() { Id = 1, Name = "Hardcover" },
            new() { Id = 2, Name = "Paperback" }
        };

        _materialRepositoryMock.Setup(materialRepository => materialRepository.GetBookFormatsAsync(It.IsAny<CancellationToken>()))
                              .ReturnsAsync(formats);

        // Act
        var items = await _materialService.GetBookFormatsAsync(CancellationToken.None);

        // Assert
        Assert.Collection(items,
            first =>
            {
                Assert.Equal(1, first.Id);
                Assert.Equal("Hardcover", first.Name);
            },
            second =>
            {
                Assert.Equal(2, second.Id);
                Assert.Equal("Paperback", second.Name);
            });
    }
}