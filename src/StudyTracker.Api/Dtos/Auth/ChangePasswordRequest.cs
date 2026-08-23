using System.ComponentModel.DataAnnotations;

namespace StudyTracker.Api.Dtos.Auth;

public sealed class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = "";

    [Required]
    [MinLength(4)]
    [MaxLength(100)]
    public string NewPassword { get; set; } = "";
}
