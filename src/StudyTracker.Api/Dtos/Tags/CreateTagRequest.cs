using System.ComponentModel.DataAnnotations;

namespace StudyTracker.Api.Dtos.Tags;

public sealed class CreateTagRequest
{
    [Required]
    [MinLength(1)]
    [MaxLength(200)]
    public string Name { get; set; } = "";
}
