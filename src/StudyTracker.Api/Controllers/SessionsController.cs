using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyTracker.Api.Data;
using StudyTracker.Api.Dtos.Sessions;
using StudyTracker.Api.Entities;

namespace StudyTracker.Api.Controllers;

[ApiController]
[Route("api/sessions")]
public sealed class SessionsController(StudyTrackerDbContext db) : ControllerBase
{
    private const long DemoUserId = 1;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SessionResponse>>> List(
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] long? subjectId,
        [FromQuery] long? tagId,
        CancellationToken cancellationToken)
    {
        var query = db.StudySessions
            .AsNoTracking()
            .Where(s => s.UserId == DemoUserId);

        if (from is not null)
            query = query.Where(s => s.StartedAt >= from);

        if (to is not null)
            query = query.Where(s => s.StartedAt <= to);

        if (subjectId is not null)
            query = query.Where(s => s.SubjectId == subjectId);

        if (tagId is not null)
            query = query.Where(s => s.SessionTags.Any(st => st.TagId == tagId));

        var items = await Project(query)
            .OrderByDescending(s => s.StartedAt)
            .ToListAsync(cancellationToken);

        return Ok(items);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<SessionResponse>> Get(long id, CancellationToken cancellationToken)
    {
        var item = await Project(
                db.StudySessions.AsNoTracking().Where(s => s.Id == id && s.UserId == DemoUserId))
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
            s => s.Id == request.SubjectId && s.UserId == DemoUserId,
            cancellationToken);
        if (!subjectOk)
            return BadRequest(new { error = "Konu bulunamadı." });

        var tags = await LoadOwnedTags(request.TagIds, cancellationToken);
        if (tags is null)
            return BadRequest(new { error = "Etiketlerden biri bu kullanıcıya ait değil." });

        var session = new StudySession
        {
            UserId = DemoUserId,
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
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == DemoUserId, cancellationToken);

        if (session is null)
            return NotFound();

        var subjectOk = await db.Subjects.AnyAsync(
            s => s.Id == request.SubjectId && s.UserId == DemoUserId,
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
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == DemoUserId, cancellationToken);

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
            .Where(t => t.UserId == DemoUserId && distinctIds.Contains(t.Id))
            .ToListAsync(cancellationToken);

        return tags.Count == distinctIds.Count ? tags : null;
    }
}
