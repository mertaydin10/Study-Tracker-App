namespace StudyTracker.Api.Dtos.Health;

public sealed class HealthResponse
{
    public string Status { get; init; } = "ok";
    public string Database { get; init; } = "unknown";
}
