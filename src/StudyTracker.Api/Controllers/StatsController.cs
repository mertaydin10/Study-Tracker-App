using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyTracker.Api.Data;
using StudyTracker.Api.Dtos.Stats;

namespace StudyTracker.Api.Controllers;

[ApiController]
[Route("api/stats")]
public sealed class StatsController(StudyTrackerDbContext db) : ControllerBase
{
    private const long DemoUserId = 1;

    [HttpGet("summary")]
    public async Task<ActionResult<SummaryResponse>> Summary(
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        CancellationToken cancellationToken)
    {
        var sessions = db.StudySessions
            .AsNoTracking()
            .Where(s => s.UserId == DemoUserId);

        if (from is not null)
            sessions = sessions.Where(s => s.StartedAt >= from);

        if (to is not null)
            sessions = sessions.Where(s => s.StartedAt <= to);

        // GROUP BY SQL'de; konu başına ayrı Count/Sum (N+1) yok.
        var bySubject = await sessions
            .GroupBy(s => new { s.SubjectId, s.Subject.Name })
            .Select(g => new SubjectStatsResponse
            {
                SubjectId = g.Key.SubjectId,
                SubjectName = g.Key.Name,
                SessionCount = g.Count(),
                TotalMinutes = g.Sum(s => s.DurationMinutes)
            })
            .OrderByDescending(x => x.TotalMinutes)
            .ToListAsync(cancellationToken);

        return Ok(new SummaryResponse
        {
            SessionCount = bySubject.Sum(x => x.SessionCount),
            TotalMinutes = bySubject.Sum(x => x.TotalMinutes),
            BySubject = bySubject
        });
    }
}
