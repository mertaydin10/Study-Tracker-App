using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace StudyTracker.Api.Tests;

public sealed class ApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Health_Doner()
    {
        var client = factory.CreateClient();
        var res = await client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    [Fact]
    public async Task Konular_Tokensiz_401()
    {
        var client = factory.CreateClient();
        var res = await client.GetAsync("/api/subjects");
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Kayit_Konu_Oturum_Olusturur()
    {
        var client = factory.CreateClient();
        var email = $"u{Guid.NewGuid():N}@local.test";
        var register = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = "demo",
            displayName = "Test"
        });
        register.EnsureSuccessStatusCode();

        var login = await register.Content.ReadFromJsonAsync<LoginBody>();
        Assert.False(string.IsNullOrEmpty(login?.Token));
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", login.Token);

        var subjectRes = await client.PostAsJsonAsync("/api/subjects", new { name = "C#" });
        subjectRes.EnsureSuccessStatusCode();
        var subject = await subjectRes.Content.ReadFromJsonAsync<IdBody>();
        Assert.True(subject?.Id > 0);

        var sessionRes = await client.PostAsJsonAsync("/api/sessions", new
        {
            subjectId = subject.Id,
            startedAt = DateTimeOffset.UtcNow,
            durationMinutes = 25,
            tagIds = Array.Empty<long>()
        });
        sessionRes.EnsureSuccessStatusCode();
    }

    private sealed class LoginBody
    {
        public string Token { get; set; } = "";
    }

    private sealed class IdBody
    {
        public long Id { get; set; }
    }
}
