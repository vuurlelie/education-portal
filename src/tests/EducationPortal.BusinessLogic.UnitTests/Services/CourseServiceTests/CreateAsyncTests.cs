using EducationPortal.BusinessLogic.Abstractions;
using EducationPortal.BusinessLogic.DTOs.Courses;
using EducationPortal.BusinessLogic.Services;
using EducationPortal.DataAccess.Abstractions;
using EducationPortal.DataAccess.Entities;
using Moq;

namespace EducationPortal.BusinessLogic.UnitTests.Services.CourseServiceTests;

public sealed class CreateAsyncTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new(MockBehavior.Strict);
    private readonly Mock<ICourseRepository> _courseRepositoryMock = new(MockBehavior.Strict);
    private readonly ICourseService _courseService;

    public CreateAsyncTests()
    {
        _unitOfWorkMock.SetupGet(unitOfWork => unitOfWork.CourseRepository).Returns(_courseRepositoryMock.Object);
        _unitOfWorkMock.Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(1);

        _courseService = new CourseService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task CreateAsync_PersistsCourse_AndReturnsGeneratedId()
    {   
        // Arrange
        var generatedId = 123;

        _courseRepositoryMock
            .Setup(courseRepository => courseRepository.AddAsync(It.IsAny<Course>(), It.IsAny<CancellationToken>()))
            .Callback<Course, CancellationToken>((entity, _) => entity.Id = generatedId)
            .Returns(Task.CompletedTask);

        var newCourseDto = new CourseCreateDto { Name = "Name", Description = "Description" };

        // Act
        var returnedId = await _courseService.CreateAsync(newCourseDto, CancellationToken.None);

        // Assert
        Assert.Equal(generatedId, returnedId);
        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}