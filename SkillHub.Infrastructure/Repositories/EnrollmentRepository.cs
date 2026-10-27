using Microsoft.EntityFrameworkCore;
using SkillHub.Domain.Entities;
using SkillHub.Domain.Enums;
using SkillHub.Domain.Interfaces;
using SkillHub.Infrastructure.Persistence;

namespace SkillHub.Infrastructure.Repositories;

public class EnrollmentRepository(ApplicationDbContext context) : IEnrollmentRepository
{
    public async Task<Enrollment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Enrollments
            .Include(e => e.Workshop)
                .ThenInclude(w => w!.Instructor)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Enrollment>> GetByParticipantEmailAsync(string participantEmail, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = participantEmail.Trim().ToLowerInvariant();
        return await context.Enrollments
            .Include(e => e.Workshop)
                .ThenInclude(w => w!.Instructor)
            .Where(e => e.ParticipantEmail == normalizedEmail)
            .OrderByDescending(e => e.CreatedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Enrollment>> GetActiveByParticipantEmailAsync(string participantEmail, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = participantEmail.Trim().ToLowerInvariant();
        return await context.Enrollments
            .Include(e => e.Workshop)
                .ThenInclude(w => w!.Instructor)
            .Where(e => e.ParticipantEmail == normalizedEmail && e.Status == EnrollmentStatus.Active)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Enrollment>> GetByWorkshopIdAsync(Guid workshopId, CancellationToken cancellationToken = default)
    {
        return await context.Enrollments
            .Include(e => e.Workshop)
                .ThenInclude(w => w!.Instructor)
            .Where(e => e.WorkshopId == workshopId)
            .OrderByDescending(e => e.CreatedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasActiveEnrollmentAsync(string participantEmail, Guid workshopId, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = participantEmail.Trim().ToLowerInvariant();
        return await context.Enrollments.AnyAsync(
            e => e.ParticipantEmail == normalizedEmail && e.WorkshopId == workshopId && e.Status == EnrollmentStatus.Active,
            cancellationToken);
    }

    public async Task<Enrollment> AddAsync(Enrollment enrollment, CancellationToken cancellationToken = default)
    {
        context.Enrollments.Add(enrollment);
        await context.SaveChangesAsync(cancellationToken);
        return enrollment;
    }

    public async Task UpdateAsync(Enrollment enrollment, CancellationToken cancellationToken = default)
    {
        context.Enrollments.Update(enrollment);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> ClearAllAsync(CancellationToken cancellationToken = default)
    {
        var all = await context.Enrollments.ToListAsync(cancellationToken);
        var count = all.Count;
        context.Enrollments.RemoveRange(all);
        await context.SaveChangesAsync(cancellationToken);
        return count;
    }
}
