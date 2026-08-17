namespace StudyTracker.Api.Entities;

public sealed class User
{
    public long Id { get; set; }
    public string Email { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<Subject> Subjects { get; set; } = [];
    public ICollection<StudySession> StudySessions { get; set; } = [];
    public ICollection<Tag> Tags { get; set; } = [];
}
