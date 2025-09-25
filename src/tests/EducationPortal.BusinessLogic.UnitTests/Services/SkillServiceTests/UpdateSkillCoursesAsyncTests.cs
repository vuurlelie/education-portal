using EducationPortal.BusinessLogic.Abstractions;
using EducationPortal.BusinessLogic.Services;
using EducationPortal.DataAccess.Abstractions;
using EducationPortal.DataAccess.Entities;
using Moq;

namespace EducationPortal.BusinessLogic.UnitTests.Services.SkillServiceTests;

public sealed class UpdateSkillCoursesAsyncTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new(MockBehavior.Strict);
    private readonly Mock<ISkillRepository> _skillRepositoryMock = new(MockBehavior.Strict);
    private readonly ISkillService _skillService;

    public UpdateSkillCoursesAsyncTests()
    {
        _unitOfWorkMock.SetupGet(unitOfWork => unitOfWork.SkillRepository)
                      .Returns(_skillRepositoryMock.Object);
        _unitOfWorkMock.Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(1);

        _skillService = new SkillService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task UpdateSkillCoursesAsync_NotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        _skillRepositoryMock
            .Setup(skillRepository => skillRepository.GetWithDetailsByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Skill?)null);

        // Act
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _skillService.UpdateSkillCoursesAsync(10, new[] { 1, 2 }, CancellationToken.None));

        // Assert
        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateSkillCoursesAsync_TogglesExisting_AddsMissing_AndDeletesRemoved()
    {
        // Arrange
        var skill = new Skill
        {
            Id = 10,
            Name = "Skill",
            CourseSkills = new List<CourseSkill>
            {
                new() { CourseId = 1, SkillId = 10, RecordStatus = RecordStatus.Active },
                new() { CourseId = 2, SkillId = 10, RecordStatus = RecordStatus.Deleted }
            }
        };

        _skillRepositoryMock
            .Setup(skillRepository => skillRepository.GetWithDetailsByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(skill);

        // Act
        await _skillService.UpdateSkillCoursesAsync(10, new[] { 2, 3, 3 }, CancellationToken.None);

        // Assert
        Assert.Contains(skill.CourseSkills, courseSkill => courseSkill.CourseId == 2 && courseSkill.RecordStatus == RecordStatus.Active);
        Assert.Contains(skill.CourseSkills, courseSkill => courseSkill.CourseId == 3 && courseSkill.RecordStatus == RecordStatus.Active);
        Assert.Contains(skill.CourseSkills, courseSkill => courseSkill.CourseId == 1 && courseSkill.RecordStatus == RecordStatus.Deleted);
        Assert.Equal(1, skill.CourseSkills.Count(courseSkill => courseSkill.CourseId == 3));

        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}