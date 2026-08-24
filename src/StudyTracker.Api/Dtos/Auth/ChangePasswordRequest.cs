using System.ComponentModel.DataAnnotations;

namespace StudyTracker.Api.Dtos.Auth;

public sealed class ChangePasswordRequest
{
    [Required(ErrorMessage = "Mevcut şifre gerekli.")]
    public string CurrentPassword { get; set; } = "";

    [Required(ErrorMessage = "Yeni şifre gerekli.")]
    [MinLength(4, ErrorMessage = "Yeni şifre en az 4 karakter olmalı.")]
    [MaxLength(100, ErrorMessage = "Yeni şifre en fazla 100 karakter olabilir.")]
    public string NewPassword { get; set; } = "";
}
