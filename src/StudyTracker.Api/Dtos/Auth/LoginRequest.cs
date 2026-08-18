using System.ComponentModel.DataAnnotations;

namespace StudyTracker.Api.Dtos.Auth;

public sealed class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    [MinLength(1)]
    public string Password { get; set; } = "";
}
