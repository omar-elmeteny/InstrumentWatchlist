using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace InstrumentWatchlistTests.Integration.Infrastructure;

public class WatchlistWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var databaseName = $"InstrumentWatchlistTests-{Guid.NewGuid()}";

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<WatchlistItemDbContext>>();
            services.AddDbContext<WatchlistItemDbContext>(options =>
                options.UseInMemoryDatabase(databaseName));
        });
    }
}