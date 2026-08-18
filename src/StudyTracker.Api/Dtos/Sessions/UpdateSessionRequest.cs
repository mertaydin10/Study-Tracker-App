using System.ComponentModel.DataAnnotations;

namespace StudyTracker.Api.Dtos.Sessions;

public sealed class UpdateSessionRequest
{
    [Range(1, long.MaxValue)]
    public long SubjectId { get; set; }

    [Required]
    public DateTimeOffset StartedAt { get; set; }

    [Range(1, int.MaxValue)]
    public int DurationMinutes { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public List<long> TagIds { get; set; } = [];
}
