using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyTracker.Api.Auth;
using StudyTracker.Api.Data;
using StudyTracker.Api.Dtos.Stats;

namespace StudyTracker.Api.Controllers;

[ApiController]
[Route("api/stats")]
[Authorize]
public sealed class StatsController(StudyTrackerDbContext db) : ControllerBase
{
    private long UserId => User.GetRequiredUserId();

    [HttpGet("summary")]
    public async Task<ActionResult<SummaryResponse>> Summary(
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] long? subjectId,
        CancellationToken cancellationToken)
    {
        if (from is not null && to is not null && from > to)
            return BadRequest(new { error = "Başlangıç bitişten sonra olamaz." });

        var sessions = db.StudySessions
            .AsNoTracking()
            .Where(s => s.UserId == UserId);

        if (from is not null)
            sessions = sessions.Where(s => s.StartedAt >= from);

        if (to is not null)
            sessions = sessions.Where(s => s.StartedAt <= to);

        if (subjectId is not null)
            sessions = sessions.Where(s => s.SubjectId == subjectId);

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
