using System.ComponentModel.DataAnnotations;

namespace StudyTracker.Api.Dtos.Auth;

public sealed class RegisterRequest
{
    [Required(ErrorMessage = "E-posta gerekli.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta yaz.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Şifre gerekli.")]
    [MinLength(4, ErrorMessage = "Şifre en az 4 karakter olmalı.")]
    [MaxLength(100, ErrorMessage = "Şifre en fazla 100 karakter olabilir.")]
    public string Password { get; set; } = "";

    [MaxLength(200, ErrorMessage = "Ad en fazla 200 karakter olabilir.")]
    public string? DisplayName { get; set; }
}
