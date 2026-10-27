using Microsoft.EntityFrameworkCore;
using SkillHub.Domain.Entities;
using SkillHub.Domain.Enums;
using SkillHub.Domain.Interfaces;
using SkillHub.Infrastructure.Persistence;

namespace SkillHub.Infrastructure.Repositories;

public class WorkshopRepository(ApplicationDbContext context) : IWorkshopRepository
{
    public async Task<IEnumerable<Workshop>> GetAvailableWorkshopsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.Now;
        return await context.Workshops
            .Include(w => w.Instructor)
            .Include(w => w.Enrollments)
            .Where(w => w.StartDate > now)
            .OrderBy(w => w.StartDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Workshop?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Workshops
            .Include(w => w.Instructor)
            .Include(w => w.Enrollments)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<Workshop?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();
        return await context.Workshops
            .Include(w => w.Instructor)
            .Include(w => w.Enrollments)
            .FirstOrDefaultAsync(w => w.Code == normalizedCode, cancellationToken);
    }

    public async Task<bool> ExistsCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();
        return await context.Workshops.AnyAsync(w => w.Code == normalizedCode, cancellationToken);
    }

    public async Task<IEnumerable<Workshop>> GetTopWorkshopsAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        return await context.Workshops
            .Include(w => w.Instructor)
            .Include(w => w.Enrollments)
            .OrderByDescending(w => w.Enrollments.Count(e => e.Status == EnrollmentStatus.Active))
            .Take(count)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Workshop> AddAsync(Workshop workshop, CancellationToken cancellationToken = default)
    {
        context.Workshops.Add(workshop);
        await context.SaveChangesAsync(cancellationToken);
        return workshop;
    }

    public async Task UpdateAsync(Workshop workshop, CancellationToken cancellationToken = default)
    {
        context.Workshops.Update(workshop);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Workshop workshop, CancellationToken cancellationToken = default)
    {
        context.Workshops.Remove(workshop);
        await context.SaveChangesAsync(cancellationToken);
    }
}
