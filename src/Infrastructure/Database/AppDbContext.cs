using Application.Common.Interfaces;

namespace Infrastructure.Database;

public class AppDbContext : DbContext, IAppDbContext
{
    private readonly MiddlewareSettings _middlewareSettings;

    static AppDbContext()
    {
        // The existing schema stores DateTime as "timestamp without time zone".
        // Set once per process, also for design-time tools (dotnet ef).
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    public AppDbContext(DbContextOptions<AppDbContext> options, IOptions<MiddlewareSettings> middlewareSettings)
        : base(options)
    {
        _middlewareSettings = middlewareSettings.Value;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Apartment> Apartments => Set<Apartment>();
    public DbSet<MeterReading> MeterReadings => Set<MeterReading>();
    public DbSet<Tariff> Tariffs => Set<Tariff>();
    public DbSet<AdditionalCharge> AdditionalCharges => Set<AdditionalCharge>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (_middlewareSettings.EnableDatabaseLogging)
        {
            optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
        }
    }
}
