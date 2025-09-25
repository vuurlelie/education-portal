using EducationPortal.BusinessLogic;
using EducationPortal.BusinessLogic.Abstractions;
using EducationPortal.DataAccess.Abstractions;
using EducationPortal.DataAccess.Entities;
using EducationPortal.DataAccess.Enums;

namespace EducationPortal.BusinessLogic.Services;

public sealed class EnrollmentService : IEnrollmentService
{
    private readonly IUnitOfWork _unitOfWork;

    public EnrollmentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task EnrollAsync(Guid userId, int courseId, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.UserRepository.GetByIdAsync(userId, cancellationToken)
                   ?? throw new KeyNotFoundException($"User '{userId}' not found.");

        var course = await _unitOfWork.CourseRepository.GetWithDetailsByIdAsync(courseId, cancellationToken)
                    ?? throw new KeyNotFoundException($"Course '{courseId}' not found.");

        var existingEnrollment = await _unitOfWork.UserCourseRepository
            .GetByUserAndCourseAsync(userId, courseId, cancellationToken);

        if (existingEnrollment is not null)
        {
            if (IsCompleted(existingEnrollment))
            {
                return;
            }

            var inProgressStatus = await RequireCourseStatusAsync("InProgress", cancellationToken);

            existingEnrollment.CourseStatus = inProgressStatus;
            existingEnrollment.ProgressPercent = await CalculateProgressPercentAsync(userId, course, cancellationToken);

            _unitOfWork.UserCourseRepository.Update(existingEnrollment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        var initialStatus = await RequireCourseStatusAsync("InProgress", cancellationToken);
        var initialProgress = await CalculateProgressPercentAsync(userId, course, cancellationToken);

        var newEnrollment = new UserCourse
        {
            User = user,
            Course = course,
            CourseStatus = initialStatus,
            ProgressPercent = initialProgress
        };

        await _unitOfWork.UserCourseRepository.AddAsync(newEnrollment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkMaterialCompleteAsync(Guid userId, int materialId, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.UserRepository.GetByIdAsync(userId, cancellationToken)
                   ?? throw new KeyNotFoundException($"User '{userId}' not found.");

        var material = await _unitOfWork.MaterialRepository.GetWithDetailsByIdAsync(materialId, cancellationToken)
                      ?? throw new KeyNotFoundException($"Material '{materialId}' not found.");

        var existing = await _unitOfWork.UserMaterialRepository
            .GetByUserAndMaterialAsync(userId, materialId, cancellationToken);

        if (existing is null)
        {
            var newUserMaterial = new UserMaterial
            {
                User = user,
                Material = material
            };

            await _unitOfWork.UserMaterialRepository.AddAsync(newUserMaterial, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var affectedCourseIds = material.CourseMaterials
            .Where(link => link.RecordStatus == RecordStatus.Active)
            .Select(link => link.CourseId)
            .Distinct()
            .ToArray();

        foreach (var affectedCourseId in affectedCourseIds)
        {
            var enrollment = await _unitOfWork.UserCourseRepository
                .GetByUserAndCourseAsync(userId, affectedCourseId, cancellationToken);

            if (enrollment is null)
            {
                continue;
            }

            await RecalculateProgressAndMaybeCompleteAsync(userId, affectedCourseId, enrollment, cancellationToken);
        }
    }

    public async Task CompleteCourseAsync(Guid userId, int courseId, CancellationToken cancellationToken = default)
    {
        var enrollment = await _unitOfWork.UserCourseRepository
            .GetByUserAndCourseAsync(userId, courseId, cancellationToken)
            ?? throw new KeyNotFoundException($"Enrollment for user '{userId}' and course '{courseId}' not found.");

        if (IsCompleted(enrollment))
        {
            return;
        }

        var course = await _unitOfWork.CourseRepository.GetWithDetailsByIdAsync(courseId, cancellationToken)
                    ?? throw new KeyNotFoundException($"Course '{courseId}' not found.");

        var completedStatus = await RequireCourseStatusAsync("Completed", cancellationToken);

        enrollment.CourseStatus = completedStatus;
        enrollment.ProgressPercent = (byte)BusinessRules.MaxProgressPercent;

        _unitOfWork.UserCourseRepository.Update(enrollment);
        await AwardCourseSkillsAsync(userId, course, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<CourseEnrollmentState> GetUserCourseStatusAsync(Guid userId, int courseId, CancellationToken cancellationToken = default)
    {
        var link = await _unitOfWork.UserCourseRepository
            .GetByUserAndCourseWithStatusAsync(userId, courseId, cancellationToken);

        if (link is null)
        {
            return CourseEnrollmentState.NotEnrolled;
        }

        var statusName = link.CourseStatus.Name.Trim();

        if (statusName.Equals("Completed", StringComparison.OrdinalIgnoreCase))
        {
            return CourseEnrollmentState.Completed;
        }

        return CourseEnrollmentState.InProgress;
    }

    public async Task<HashSet<int>> GetUserCompletedMaterialIdsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var ids = await _unitOfWork.UserMaterialRepository
            .GetCompletedMaterialIdsByUserAsync(userId, cancellationToken);

        return ids.Count is 0 ? [] : ids.ToHashSet();
    }

    private static bool IsCompleted(UserCourse enrollment)
    {
        return string.Equals(enrollment.CourseStatus.Name, "Completed", StringComparison.OrdinalIgnoreCase);
    }

    private async Task RecalculateProgressAndMaybeCompleteAsync(
        Guid userId,
        int courseId,
        UserCourse enrollment,
        CancellationToken cancellationToken)
    {
        var course = await _unitOfWork.CourseRepository.GetWithDetailsByIdAsync(courseId, cancellationToken)
                    ?? throw new KeyNotFoundException($"Course '{courseId}' not found.");

        var progressPercent = await CalculateProgressPercentAsync(userId, course, cancellationToken);
        enrollment.ProgressPercent = progressPercent;

        if (progressPercent >= BusinessRules.MaxProgressPercent && !IsCompleted(enrollment))
        {
            var completedStatus = await RequireCourseStatusAsync("Completed", cancellationToken);
            enrollment.CourseStatus = completedStatus;

            _unitOfWork.UserCourseRepository.Update(enrollment);
            await AwardCourseSkillsAsync(userId, course, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        _unitOfWork.UserCourseRepository.Update(enrollment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<byte> CalculateProgressPercentAsync(Guid userId, Course course, CancellationToken cancellationToken)
    {
        var activeMaterialIds = course.CourseMaterials
            .Where(link => link.RecordStatus == RecordStatus.Active)
            .Select(link => link.MaterialId)
            .Distinct()
            .ToArray();

        if (activeMaterialIds.Length == 0)
        {
            return (byte)BusinessRules.MinProgressPercent;
        }

        var completedCount = await _unitOfWork.UserMaterialRepository
            .CountCompletedByUserForMaterialIdsAsync(userId, activeMaterialIds, cancellationToken);

        var percent = (int)Math.Round(
            completedCount * (double)BusinessRules.MaxProgressPercent / activeMaterialIds.Length,
            MidpointRounding.AwayFromZero);

        if (percent < BusinessRules.MinProgressPercent)
        {
            percent = BusinessRules.MinProgressPercent;
        }

        if (percent > BusinessRules.MaxProgressPercent)
        {
            percent = BusinessRules.MaxProgressPercent;
        }

        return (byte)percent;
    }

    private async Task AwardCourseSkillsAsync(Guid userId, Course course, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.UserRepository.GetByIdAsync(userId, cancellationToken)
                   ?? throw new KeyNotFoundException($"User '{userId}' not found.");

        var skillIdsToAward = course.CourseSkills
            .Where(link => link.RecordStatus == RecordStatus.Active)
            .Select(link => link.SkillId)
            .Distinct()
            .ToArray();

        if (skillIdsToAward.Length == 0)
        {
            return;
        }

        foreach (var skillId in skillIdsToAward)
        {
            var skill = await _unitOfWork.SkillRepository.GetByIdAsync(skillId, cancellationToken);
            if (skill is null)
            {
                continue;
            }

            var existing = await _unitOfWork.UserSkillRepository
                .GetByUserAndSkillAsync(userId, skillId, cancellationToken);

            if (existing is null)
            {
                var newUserSkill = new UserSkill
                {
                    User = user,
                    Skill = skill,
                    Level = 1
                };

                await _unitOfWork.UserSkillRepository.AddAsync(newUserSkill, cancellationToken);
            }
            else
            {
                existing.Level += 1;
                _unitOfWork.UserSkillRepository.Update(existing);
            }
        }
    }

    private async Task<CourseStatus> RequireCourseStatusAsync(string name, CancellationToken cancellationToken)
    {
        var status = await _unitOfWork.UserCourseRepository.GetStatusByNameAsync(name, cancellationToken);
        if (status is null)
        {
            throw new InvalidOperationException($"CourseStatus '{name}' not found. Check seed data.");
        }

        return status;
    }
}