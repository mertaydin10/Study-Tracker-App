using Microsoft.EntityFrameworkCore;
using StudyTracker.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("StudyTracker")
    ?? throw new InvalidOperationException("Connection string 'StudyTracker' is missing.");

builder.Services.AddDbContext<StudyTrackerDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
