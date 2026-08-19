using StudyTracker.Api.Hosting;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddStudyTrackerApi(builder.Configuration);

var app = builder.Build();
app.UseStudyTrackerApi();
app.Run();
