using System.ComponentModel.DataAnnotations;

namespace StudyTracker.Api.Dtos.Tags;

public sealed class CreateTagRequest
{
    [Required(ErrorMessage = "Etiket adı gerekli.")]
    [MinLength(1, ErrorMessage = "Etiket adı gerekli.")]
    [MaxLength(200, ErrorMessage = "Etiket adı en fazla 200 karakter olabilir.")]
    public string Name { get; set; } = "";
}
