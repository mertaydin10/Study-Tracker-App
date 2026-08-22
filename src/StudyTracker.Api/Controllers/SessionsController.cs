using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyTracker.Api.Auth;
using StudyTracker.Api.Data;
using StudyTracker.Api.Dtos.Paging;
using StudyTracker.Api.Dtos.Sessions;
using StudyTracker.Api.Entities;

namespace StudyTracker.Api.Controllers;

[ApiController]
[Route("api/sessions")]
[Authorize]
public sealed class SessionsController(StudyTrackerDbContext db) : ControllerBase
{
    private long UserId => User.GetRequiredUserId();

    [HttpGet]
    public async Task<ActionResult<PagedResponse<SessionResponse>>> List(
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] long? subjectId,
        [FromQuery] long? tagId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
            return BadRequest(new { error = "page 1 veya daha büyük olmalı." });

        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = FilterSessions(from, to, subjectId, tagId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await Project(query)
            .OrderByDescending(s => s.StartedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return Ok(new PagedResponse<SessionResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export(
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] long? subjectId,
        [FromQuery] long? tagId,
        CancellationToken cancellationToken)
    {
        var query = FilterSessions(from, to, subjectId, tagId);
        var items = await Project(query)
            .OrderByDescending(s => s.StartedAt)
            .Take(5000)
            .ToListAsync(cancellationToken);

        var csv = new System.Text.StringBuilder();
        csv.Append('\uFEFF');
        csv.AppendLine("subject,started_at,duration_minutes,notes");
        foreach (var row in items)
            csv.AppendLine(string.Join(',',
                Csv(row.SubjectName),
                Csv(row.StartedAt.ToString("o")),
                row.DurationMinutes.ToString(),
                Csv(row.Notes)));

        return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "sessions.csv");
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<SessionResponse>> Get(long id, CancellationToken cancellationToken)
    {
        var item = await Project(
                db.StudySessions.AsNoTracking().Where(s => s.Id == id && s.UserId == UserId))
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null)
            return NotFound();

        return Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<SessionResponse>> Create(
        CreateSessionRequest request,
        CancellationToken cancellationToken)
    {
        var subjectOk = await db.Subjects.AnyAsync(
            s => s.Id == request.SubjectId && s.UserId == UserId,
            cancellationToken);
        if (!subjectOk)
            return BadRequest(new { error = "Konu bulunamadı." });

        var tags = await LoadOwnedTags(request.TagIds, cancellationToken);
        if (tags is null)
            return BadRequest(new { error = "Etiketlerden biri bu kullanıcıya ait değil." });

        var session = new StudySession
        {
            UserId = UserId,
            SubjectId = request.SubjectId,
            StartedAt = request.StartedAt,
            DurationMinutes = request.DurationMinutes,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim()
        };

        foreach (var tag in tags)
            session.SessionTags.Add(new StudySessionTag { Tag = tag });

        db.StudySessions.Add(session);
        await db.SaveChangesAsync(cancellationToken);

        var body = await Project(
                db.StudySessions.AsNoTracking().Where(s => s.Id == session.Id))
            .SingleAsync(cancellationToken);

        return CreatedAtAction(nameof(Get), new { id = session.Id }, body);
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<SessionResponse>> Update(
        long id,
        UpdateSessionRequest request,
        CancellationToken cancellationToken)
    {
        var session = await db.StudySessions
            .Include(s => s.SessionTags)
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == UserId, cancellationToken);

        if (session is null)
            return NotFound();

        var subjectOk = await db.Subjects.AnyAsync(
            s => s.Id == request.SubjectId && s.UserId == UserId,
            cancellationToken);
        if (!subjectOk)
            return BadRequest(new { error = "Konu bulunamadı." });

        var tags = await LoadOwnedTags(request.TagIds, cancellationToken);
        if (tags is null)
            return BadRequest(new { error = "Etiketlerden biri bu kullanıcıya ait değil." });

        session.SubjectId = request.SubjectId;
        session.StartedAt = request.StartedAt;
        session.DurationMinutes = request.DurationMinutes;
        session.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        session.SessionTags.Clear();
        foreach (var tag in tags)
            session.SessionTags.Add(new StudySessionTag { Tag = tag });

        await db.SaveChangesAsync(cancellationToken);

        var body = await Project(
                db.StudySessions.AsNoTracking().Where(s => s.Id == session.Id))
            .SingleAsync(cancellationToken);

        return Ok(body);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        var session = await db.StudySessions
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == UserId, cancellationToken);

        if (session is null)
            return NotFound();

        db.StudySessions.Remove(session);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    // Tek Select: konu adı + etiketler aynı sorguda; oturum başına ayrı Tag sorgusu yok (N+1 değil).
    private static IQueryable<SessionResponse> Project(IQueryable<StudySession> query) =>
        query.Select(s => new SessionResponse
        {
            Id = s.Id,
            SubjectId = s.SubjectId,
            SubjectName = s.Subject.Name,
            StartedAt = s.StartedAt,
            DurationMinutes = s.DurationMinutes,
            Notes = s.Notes,
            CreatedAt = s.CreatedAt,
            Tags = s.SessionTags
                .Select(st => new SessionTagResponse { Id = st.Tag.Id, Name = st.Tag.Name })
                .ToList()
        });

    private async Task<List<Tag>?> LoadOwnedTags(IReadOnlyCollection<long> tagIds, CancellationToken cancellationToken)
    {
        var distinctIds = tagIds.Distinct().ToList();
        if (distinctIds.Count == 0)
            return [];

        var tags = await db.Tags
            .Where(t => t.UserId == UserId && distinctIds.Contains(t.Id))
            .ToListAsync(cancellationToken);

        return tags.Count == distinctIds.Count ? tags : null;
    }

    private IQueryable<StudySession> FilterSessions(
        DateTimeOffset? from,
        DateTimeOffset? to,
        long? subjectId,
        long? tagId)
    {
        var query = db.StudySessions
            .AsNoTracking()
            .Where(s => s.UserId == UserId);

        if (from is not null)
            query = query.Where(s => s.StartedAt >= from);

        if (to is not null)
            query = query.Where(s => s.StartedAt <= to);

        if (subjectId is not null)
            query = query.Where(s => s.SubjectId == subjectId);

        if (tagId is not null)
            query = query.Where(s => s.SessionTags.Any(st => st.TagId == tagId));

        return query;
    }

    private static string Csv(string? value)
    {
        var text = value ?? "";
        if (text.Contains('"') || text.Contains(',') || text.Contains('\n'))
            return $"\"{text.Replace("\"", "\"\"")}\"";
        return text;
    }
}
