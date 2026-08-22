using System.ComponentModel.DataAnnotations;

namespace StudyTracker.Api.Dtos.Auth;

public sealed class RegisterRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    [MinLength(4)]
    [MaxLength(100)]
    public string Password { get; set; } = "";

    [MaxLength(200)]
    public string? DisplayName { get; set; }
}
