using RunMethodsSequentially;

namespace Infrastructure.Database.Services;

public class MigrateDatabaseService : IStartupServiceToRunSequentially
{
    public int OrderNum { get; }
    private static readonly ILogger _logger = Log.ForContext(typeof(MigrateDatabaseService));
    public async ValueTask ApplyYourChangeAsync(IServiceProvider scopedServices)
    {
        var dbContext = scopedServices.GetRequiredService<AppDbContext>();

        if (dbContext.Database.GetMigrations().Any()
            && dbContext.Database.GetPendingMigrations().Any())
        {
            _logger.Information("Applying Migrations.");
            await dbContext.Database.MigrateAsync();
        }

    }
}
