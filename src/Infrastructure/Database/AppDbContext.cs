using Application.Common.Interfaces;

namespace Infrastructure.Database;

public class AppDbContext:DbContext, IAppDbContext
{
    private readonly MiddlewareSettings _middlewareConfig;
    public AppDbContext(DbContextOptions<AppDbContext> options,
    IOptions<MiddlewareSettings> middlewareConfig)
        : base(options)
    {
        _middlewareConfig = middlewareConfig.Value;
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (_middlewareConfig.EnableDatabaseLogging)
        {
            optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
        }
    }

    public DbSet<User> Users { get; set; } = default!;


}
