namespace StudyTracker.Api.Dtos.Stats;

public sealed class SubjectStatsResponse
{
    public long SubjectId { get; init; }
    public string SubjectName { get; init; } = "";
    public int SessionCount { get; init; }
    public int TotalMinutes { get; init; }
}

public sealed class SummaryResponse
{
    public int SessionCount { get; init; }
    public int TotalMinutes { get; init; }
    public IReadOnlyList<SubjectStatsResponse> BySubject { get; init; } = [];
}
