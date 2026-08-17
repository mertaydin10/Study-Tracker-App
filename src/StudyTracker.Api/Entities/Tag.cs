namespace StudyTracker.Api.Entities;

public sealed class Tag
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Name { get; set; } = "";

    public User User { get; set; } = null!;
    public ICollection<StudySessionTag> SessionTags { get; set; } = [];
}
