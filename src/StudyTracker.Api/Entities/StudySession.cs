namespace StudyTracker.Api.Entities;

public sealed class StudySession
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long SubjectId { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public int DurationMinutes { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public User User { get; set; } = null!;
    public Subject Subject { get; set; } = null!;
    public ICollection<StudySessionTag> SessionTags { get; set; } = [];
}
