using Microsoft.EntityFrameworkCore;
using SkillHub.Domain.Entities;
using SkillHub.Domain.Interfaces;
using SkillHub.Infrastructure.Persistence;

namespace SkillHub.Infrastructure.Repositories;

public class InstructorRepository(ApplicationDbContext context) : IInstructorRepository
{
    public async Task<Instructor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Instructors
            .Include(i => i.Workshops)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<Instructor?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return await context.Instructors
            .Include(i => i.Workshops)
            .FirstOrDefaultAsync(i => i.Email.ToLower() == normalized, cancellationToken);
    }

    public async Task<IEnumerable<Instructor>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Instructors
            .OrderBy(i => i.FullName)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Instructor> AddAsync(Instructor instructor, CancellationToken cancellationToken = default)
    {
        context.Instructors.Add(instructor);
        await context.SaveChangesAsync(cancellationToken);
        return instructor;
    }
}
