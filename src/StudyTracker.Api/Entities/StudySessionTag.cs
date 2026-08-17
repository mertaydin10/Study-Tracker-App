namespace StudyTracker.Api.Entities;

public sealed class StudySessionTag
{
    public long StudySessionId { get; set; }
    public long TagId { get; set; }

    public StudySession StudySession { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}
