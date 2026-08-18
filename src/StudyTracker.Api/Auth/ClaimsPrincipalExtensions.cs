using System.Security.Claims;

namespace StudyTracker.Api.Auth;

public static class ClaimsPrincipalExtensions
{
    public static long GetRequiredUserId(this ClaimsPrincipal user)
    {
        var raw = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(raw, out var id))
            throw new InvalidOperationException("Token içinde kullanıcı id yok.");
        return id;
    }
}
