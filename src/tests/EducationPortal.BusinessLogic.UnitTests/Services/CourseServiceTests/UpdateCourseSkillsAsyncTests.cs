using EducationPortal.BusinessLogic.Abstractions;
using EducationPortal.BusinessLogic.Services;
using EducationPortal.DataAccess.Abstractions;
using EducationPortal.DataAccess.Entities;
using Moq;

namespace EducationPortal.BusinessLogic.UnitTests.Services.CourseServiceTests;

public sealed class UpdateCourseSkillsAsyncTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new(MockBehavior.Strict);
    private readonly Mock<ICourseRepository> _courseRepositoryMock = new(MockBehavior.Strict);
    private readonly ICourseService _courseService;

    public UpdateCourseSkillsAsyncTests()
    {
        _unitOfWorkMock.SetupGet(unitOfWork => unitOfWork.CourseRepository).Returns(_courseRepositoryMock.Object);
        _unitOfWorkMock.Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(1);

        _courseService = new CourseService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task UpdateCourseSkillsAsync_NotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        _courseRepositoryMock.Setup(courseRepository => courseRepository.GetWithDetailsByIdAsync(10, It.IsAny<CancellationToken>()))
                             .ReturnsAsync((Course?)null);

        // Act
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _courseService.UpdateCourseSkillsAsync(10, new[] { 5, 6 }, CancellationToken.None));

        // Assert
        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateCourseSkillsAsync_TogglesExisting_AddsMissing_AndDeletesRemoved()
    {
        // Arrange
        var course = new Course
        {
            Id = 10,
            Name = "Course",
            CourseSkills = new List<CourseSkill>
            {
                new() { CourseId = 10, SkillId = 5, RecordStatus = RecordStatus.Active },
                new() { CourseId = 10, SkillId = 6, RecordStatus = RecordStatus.Deleted }
            }
        };

        _courseRepositoryMock.Setup(courseRepository => courseRepository.GetWithDetailsByIdAsync(10, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(course);

        // Act
        await _courseService.UpdateCourseSkillsAsync(10, new[] { 6, 7, 7 }, CancellationToken.None);

        // Assert
        Assert.Contains(course.CourseSkills, courseSkill => courseSkill.SkillId == 6 && courseSkill.RecordStatus == RecordStatus.Active);
        Assert.Contains(course.CourseSkills, courseSkill => courseSkill.SkillId == 7 && courseSkill.RecordStatus == RecordStatus.Active);
        Assert.Contains(course.CourseSkills, courseSkill => courseSkill.SkillId == 5 && courseSkill.RecordStatus == RecordStatus.Deleted);
        Assert.Equal(1, course.CourseSkills.Count(courseSkill => courseSkill.SkillId == 7));

        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}