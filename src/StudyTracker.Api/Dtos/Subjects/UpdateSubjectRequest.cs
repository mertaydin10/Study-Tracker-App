using System.ComponentModel.DataAnnotations;

namespace StudyTracker.Api.Dtos.Subjects;

public sealed class UpdateSubjectRequest
{
    [Required]
    [MinLength(1)]
    [MaxLength(200)]
    public string Name { get; set; } = "";
}
