namespace StudyTracker.Api.Dtos.Sessions;

public sealed class SessionTagResponse
{
    public long Id { get; init; }
    public string Name { get; init; } = "";
}

public sealed class SessionResponse
{
    public long Id { get; init; }
    public long SubjectId { get; init; }
    public string SubjectName { get; init; } = "";
    public DateTimeOffset StartedAt { get; init; }
    public int DurationMinutes { get; init; }
    public string? Notes { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public IReadOnlyList<SessionTagResponse> Tags { get; init; } = [];
}
