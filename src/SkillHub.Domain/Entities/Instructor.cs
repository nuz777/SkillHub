namespace SkillHub.Domain.Entities;

public class Instructor : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;

    public ICollection<Workshop> Workshops { get; set; } = new List<Workshop>();
}
