using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using StudyTracker.Api.Data;
using StudyTracker.Api.Dtos.Tags;
using StudyTracker.Api.Entities;

namespace StudyTracker.Api.Controllers;

[ApiController]
[Route("api/tags")]
public sealed class TagsController(StudyTrackerDbContext db) : ControllerBase
{
    private const long DemoUserId = 1;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TagResponse>>> List(CancellationToken cancellationToken)
    {
        var items = await db.Tags
            .AsNoTracking()
            .Where(t => t.UserId == DemoUserId)
            .OrderBy(t => t.Name)
            .Select(t => new TagResponse { Id = t.Id, Name = t.Name })
            .ToListAsync(cancellationToken);

        return Ok(items);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<TagResponse>> Get(long id, CancellationToken cancellationToken)
    {
        var item = await db.Tags
            .AsNoTracking()
            .Where(t => t.Id == id && t.UserId == DemoUserId)
            .Select(t => new TagResponse { Id = t.Id, Name = t.Name })
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null)
            return NotFound();

        return Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<TagResponse>> Create(
        CreateTagRequest request,
        CancellationToken cancellationToken)
    {
        var tag = new Tag { UserId = DemoUserId, Name = request.Name.Trim() };
        db.Tags.Add(tag);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            return Conflict(new { error = "Bu isimde bir etiket zaten var." });
        }

        return CreatedAtAction(
            nameof(Get),
            new { id = tag.Id },
            new TagResponse { Id = tag.Id, Name = tag.Name });
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        var tag = await db.Tags
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == DemoUserId, cancellationToken);

        if (tag is null)
            return NotFound();

        db.Tags.Remove(tag);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static bool IsUniqueViolation(DbUpdateException ex) =>
        ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation;
}
