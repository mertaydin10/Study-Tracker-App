namespace StudyTracker.Api.Dtos.Auth;

public sealed class LoginResponse
{
    public required string Token { get; init; }
    public DateTimeOffset ExpiresAt { get; init; }
}
