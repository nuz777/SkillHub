namespace SkillHub.Infrastructure.Security;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "SkillHub";
    public string Audience { get; set; } = "SkillHub.Users";
    public string SigningKey { get; set; } = "SkillHub_Super_Secret_Key_For_Jwt_Authentication_2026_Minimum_32_Bytes!";
    public int ExpirationMinutes { get; set; } = 1440; // 24 hours
}
