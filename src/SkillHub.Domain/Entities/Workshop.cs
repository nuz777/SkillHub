using SkillHub.Domain.Enums;
using SkillHub.Domain.Exceptions;

namespace SkillHub.Domain.Entities;

public class Workshop : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    public Guid InstructorId { get; set; }
    public Instructor? Instructor { get; set; }
    public string InstructorEmail { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int MaxCapacity { get; set; }

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public int ActiveEnrollmentsCount => Enrollments.Count(e => e.Status == EnrollmentStatus.Active);
    public int AvailableSpots => Math.Max(0, MaxCapacity - ActiveEnrollmentsCount);
    public bool HasStarted => DateTime.UtcNow >= StartDate;

    public bool HasAvailableSpots() => AvailableSpots > 0;

    public bool OverlapsWith(DateTime otherStart, DateTime otherEnd)
    {
        return StartDate < otherEnd && otherStart < EndDate;
    }

    public void ValidateForCreation()
    {
        if (string.IsNullOrWhiteSpace(Code))
            throw new BusinessRuleException("El código del taller es obligatorio.");

        if (string.IsNullOrWhiteSpace(Title))
            throw new BusinessRuleException("El título del taller es obligatorio.");

        if (string.IsNullOrWhiteSpace(InstructorEmail))
            throw new BusinessRuleException("El correo del instructor es obligatorio.");

        if (StartDate <= DateTime.UtcNow)
            throw new BusinessRuleException("El taller debe programarse con una fecha futura.");

        if (EndDate <= StartDate)
            throw new BusinessRuleException("La hora de fin debe ser posterior a la de inicio.");

        if (MaxCapacity <= 0)
            throw new BusinessRuleException("El cupo máximo debe ser mayor a cero.");
    }

    public bool CanBeCancelledByParticipant()
    {
        return DateTime.UtcNow <= StartDate.AddHours(-24);
    }

    public bool CanBeCancelledByEmployee()
    {
        return DateTime.UtcNow < StartDate;
    }
}
