namespace StudyTracker.Api.Dtos.Auth;

public sealed class UserResponse
{
    public long Id { get; init; }
    public string Email { get; init; } = "";
    public string DisplayName { get; init; } = "";
}
