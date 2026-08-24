using System.ComponentModel.DataAnnotations;

namespace StudyTracker.Api.Dtos.Auth;

public sealed class LoginRequest
{
    [Required(ErrorMessage = "E-posta gerekli.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta yaz.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Şifre gerekli.")]
    [MinLength(1, ErrorMessage = "Şifre gerekli.")]
    public string Password { get; set; } = "";
}
