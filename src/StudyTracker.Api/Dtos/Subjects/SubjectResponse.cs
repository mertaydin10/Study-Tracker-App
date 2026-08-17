namespace StudyTracker.Api.Dtos.Subjects;

public sealed class SubjectResponse
{
    public long Id { get; init; }
    public string Name { get; init; } = "";
    public DateTimeOffset CreatedAt { get; init; }
}
