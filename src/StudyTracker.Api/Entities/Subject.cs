namespace StudyTracker.Api.Entities;

public sealed class Subject
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Name { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }

    public User User { get; set; } = null!;
    public ICollection<StudySession> StudySessions { get; set; } = [];
}
