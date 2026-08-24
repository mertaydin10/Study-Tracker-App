using System.ComponentModel.DataAnnotations;

namespace StudyTracker.Api.Dtos.Subjects;

public sealed class CreateSubjectRequest
{
    [Required(ErrorMessage = "Konu adı gerekli.")]
    [MinLength(1, ErrorMessage = "Konu adı gerekli.")]
    [MaxLength(200, ErrorMessage = "Konu adı en fazla 200 karakter olabilir.")]
    public string Name { get; set; } = "";
}
