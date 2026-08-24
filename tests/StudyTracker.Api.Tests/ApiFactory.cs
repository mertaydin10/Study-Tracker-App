using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StudyTracker.Api.Data;

namespace StudyTracker.Api.Tests;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureTestServices(services =>
        {
            foreach (var descriptor in services
                         .Where(d =>
                             d.ServiceType == typeof(StudyTrackerDbContext) ||
                             d.ServiceType == typeof(DbContextOptions<StudyTrackerDbContext>) ||
                             (d.ServiceType.IsGenericType &&
                              d.ServiceType.GenericTypeArguments.Any(t => t == typeof(StudyTrackerDbContext))))
                         .ToList())
                services.Remove(descriptor);

            services.AddDbContext<StudyTrackerDbContext>(options =>
                options.UseInMemoryDatabase("study-tracker-tests"));
        });
    }
}
