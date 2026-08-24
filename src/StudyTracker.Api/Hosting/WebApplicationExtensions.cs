namespace StudyTracker.Api.Hosting;

public static class WebApplicationExtensions
{
    public static WebApplication UseStudyTrackerApi(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/openapi/v1.json", "Study Tracker");
                options.EnablePersistAuthorization();
            });
        }

        if (!app.Environment.IsDevelopment())
            app.UseHttpsRedirection();

        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        return app;
    }
}
