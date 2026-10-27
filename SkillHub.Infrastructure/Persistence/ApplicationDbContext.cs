using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SkillHub.Domain.Entities;
using SkillHub.Infrastructure.Identity;

namespace SkillHub.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Workshop> Workshops => Set<Workshop>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Instructor> Instructors => Set<Instructor>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Instructor>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.FullName).IsRequired().HasMaxLength(150);
            entity.Property(i => i.Email).IsRequired().HasMaxLength(150);
            entity.Property(i => i.Specialty).IsRequired().HasMaxLength(100);
            entity.HasIndex(i => i.Email).IsUnique();
        });

        modelBuilder.Entity<Workshop>(entity =>
        {
            entity.HasKey(w => w.Id);
            entity.Property(w => w.Code).IsRequired().HasMaxLength(20);
            entity.Property(w => w.Title).IsRequired().HasMaxLength(150);
            entity.Property(w => w.Description).HasMaxLength(1000);
            entity.Property(w => w.InstructorEmail).IsRequired().HasMaxLength(150);
            entity.Property(w => w.StartDate).IsRequired();
            entity.Property(w => w.EndDate).IsRequired();
            entity.Property(w => w.MaxCapacity).IsRequired();

            entity.HasIndex(w => w.Code).IsUnique();

            entity.HasOne(w => w.Instructor)
                  .WithMany(i => i.Workshops)
                  .HasForeignKey(w => w.InstructorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ParticipantId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ParticipantName).IsRequired().HasMaxLength(150);
            entity.Property(e => e.ParticipantEmail).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.CancelledBy).HasMaxLength(150);

            entity.HasOne(e => e.Workshop)
                  .WithMany(w => w.Enrollments)
                  .HasForeignKey(e => e.WorkshopId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.ParticipantEmail, e.WorkshopId, e.Status });
        });
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder.Properties<DateTime>()
            .HaveConversion<UtcDateTimeConverter>();

        configurationBuilder.Properties<DateTime?>()
            .HaveConversion<NullableUtcDateTimeConverter>();
    }

    private class UtcDateTimeConverter : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>
    {
        public UtcDateTimeConverter() : base(
            d => d.Kind == DateTimeKind.Utc ? d : (d.Kind == DateTimeKind.Local ? d.ToUniversalTime() : DateTime.SpecifyKind(d, DateTimeKind.Utc)),
            d => DateTime.SpecifyKind(d, DateTimeKind.Utc))
        {
        }
    }

    private class NullableUtcDateTimeConverter : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime?, DateTime?>
    {
        public NullableUtcDateTimeConverter() : base(
            d => !d.HasValue ? d : (d.Value.Kind == DateTimeKind.Utc ? d : (d.Value.Kind == DateTimeKind.Local ? d.Value.ToUniversalTime() : DateTime.SpecifyKind(d.Value, DateTimeKind.Utc))),
            d => !d.HasValue ? d : DateTime.SpecifyKind(d.Value, DateTimeKind.Utc))
        {
        }
    }
}
