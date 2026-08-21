using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyTracker.Api.Data;
using StudyTracker.Api.Dtos.Health;

namespace StudyTracker.Api.Controllers;

[ApiController]
[Route("health")]
[AllowAnonymous]
public sealed class HealthController(StudyTrackerDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<HealthResponse>> Get(CancellationToken cancellationToken)
    {
        var dbUp = await db.Database.CanConnectAsync(cancellationToken);
        var body = new HealthResponse
        {
            Status = dbUp ? "ok" : "degraded",
            Database = dbUp ? "up" : "down"
        };

        return dbUp ? Ok(body) : StatusCode(StatusCodes.Status503ServiceUnavailable, body);
    }
}
