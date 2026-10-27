using System.ComponentModel.DataAnnotations;

namespace SkillHub.Application.DTOs;

public record RegisterParticipantDto(
    [Required(ErrorMessage = "El nombre completo es requerido.")]
    [MaxLength(150)]
    string FullName,

    [Required(ErrorMessage = "El correo electrónico es requerido.")]
    [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
    string Email,

    [Required(ErrorMessage = "La contraseña es requerida.")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
    string Password
);

public record LoginDto(
    [Required(ErrorMessage = "El correo electrónico es requerido.")]
    [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
    string Email,

    [Required(ErrorMessage = "La contraseña es requerida.")]
    string Password
);

public record AuthResponseDto(
    string Token,
    string Email,
    string FullName,
    string Role,
    DateTime ExpiresAt
);
