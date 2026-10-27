using SkillHub.Domain.Enums;
using SkillHub.Domain.Exceptions;

namespace SkillHub.Domain.Entities;

public class Enrollment : BaseEntity
{
    public Guid WorkshopId { get; set; }
    public Workshop? Workshop { get; set; }

    public string ParticipantId { get; set; } = string.Empty;
    public string ParticipantName { get; set; } = string.Empty;
    public string ParticipantEmail { get; set; } = string.Empty;

    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;

    public DateTime? CancelledAt { get; set; }
    public string? CancelledBy { get; set; }

    public void Cancel(string cancelledBy, bool isEmployee)
    {
        if (Status == EnrollmentStatus.Cancelled)
            throw new BusinessRuleException("La inscripción ya se encuentra cancelada.");

        if (Workshop is null)
            throw new BusinessRuleException("No se encontró la información del taller asociado.");

        if (isEmployee)
        {
            if (!Workshop.CanBeCancelledByEmployee())
                throw new BusinessRuleException("No se puede cancelar la inscripción porque el taller ya comenzó.");
        }
        else
        {
            if (!Workshop.CanBeCancelledByParticipant())
                throw new BusinessRuleException("Los participantes solo pueden cancelar su inscripción hasta 24 horas antes del inicio del taller.");
        }

        Status = EnrollmentStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        CancelledBy = cancelledBy;
        UpdatedAt = DateTime.UtcNow;
    }
}
