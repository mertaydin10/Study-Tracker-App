using System.ComponentModel.DataAnnotations;

namespace StudyTracker.Api.Dtos.Auth;

public sealed class UpdateProfileRequest
{
    [Required(ErrorMessage = "Ad gerekli.")]
    [MaxLength(200, ErrorMessage = "Ad en fazla 200 karakter olabilir.")]
    public string DisplayName { get; set; } = "";
}
