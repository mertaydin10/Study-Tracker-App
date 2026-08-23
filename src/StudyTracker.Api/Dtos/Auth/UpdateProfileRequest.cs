using System.ComponentModel.DataAnnotations;

namespace StudyTracker.Api.Dtos.Auth;

public sealed class UpdateProfileRequest
{
    [Required]
    [MaxLength(200)]
    public string DisplayName { get; set; } = "";
}
