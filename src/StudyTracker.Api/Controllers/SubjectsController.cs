using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using StudyTracker.Api.Auth;
using StudyTracker.Api.Data;
using StudyTracker.Api.Dtos.Subjects;
using StudyTracker.Api.Entities;

namespace StudyTracker.Api.Controllers;

[ApiController]
[Route("api/subjects")]
[Authorize]
public sealed class SubjectsController(StudyTrackerDbContext db) : ControllerBase
{
    private long UserId => User.GetRequiredUserId();

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SubjectResponse>>> List(CancellationToken cancellationToken)
    {
        var items = await db.Subjects
            .AsNoTracking()
            .Where(s => s.UserId == UserId)
            .OrderBy(s => s.Id)
            .Select(s => new SubjectResponse
            {
                Id = s.Id,
                Name = s.Name,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Ok(items);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<SubjectResponse>> Get(long id, CancellationToken cancellationToken)
    {
        var item = await db.Subjects
            .AsNoTracking()
            .Where(s => s.Id == id && s.UserId == UserId)
            .Select(s => new SubjectResponse
            {
                Id = s.Id,
                Name = s.Name,
                CreatedAt = s.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null)
            return NotFound();

        return Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<SubjectResponse>> Create(
        CreateSubjectRequest request,
        CancellationToken cancellationToken)
    {
        var subject = new Subject
        {
            UserId = UserId,
            Name = request.Name.Trim()
        };

        db.Subjects.Add(subject);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            return Conflict(new { error = "Bu isimde bir konu zaten var." });
        }

        var body = new SubjectResponse
        {
            Id = subject.Id,
            Name = subject.Name,
            CreatedAt = subject.CreatedAt
        };

        return CreatedAtAction(nameof(Get), new { id = subject.Id }, body);
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<SubjectResponse>> Update(
        long id,
        UpdateSubjectRequest request,
        CancellationToken cancellationToken)
    {
        var subject = await db.Subjects
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == UserId, cancellationToken);

        if (subject is null)
            return NotFound();

        subject.Name = request.Name.Trim();

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            return Conflict(new { error = "Bu isimde bir konu zaten var." });
        }

        return Ok(new SubjectResponse
        {
            Id = subject.Id,
            Name = subject.Name,
            CreatedAt = subject.CreatedAt
        });
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        var subject = await db.Subjects
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == UserId, cancellationToken);

        if (subject is null)
            return NotFound();

        db.Subjects.Remove(subject);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsRestrictViolation(ex))
        {
            return Conflict(new { error = "Oturumu olan konu silinemez." });
        }

        return NoContent();
    }

    private static bool IsUniqueViolation(DbUpdateException ex) =>
        ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation;

    private static bool IsRestrictViolation(DbUpdateException ex) =>
        ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.ForeignKeyViolation;
}
