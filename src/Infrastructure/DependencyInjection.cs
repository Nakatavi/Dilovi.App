using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Infrastructure.Authentication;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddDatabase(configuration)
            .AddJwtSettings(configuration);

        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddRepositories();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    public static IServiceCollection AddJwtSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }

    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MiddlewareSettings>(configuration.GetSection(MiddlewareSettings.SectionName));

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql => npgsql.MigrationsAssembly("Migrations.PostgreSQL")));

        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        return services;
    }

    public static async Task MigrateDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if ((await dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
        {
            Log.Information("Applying database migrations");
            await dbContext.Database.MigrateAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Creates an admin user from the DevelopmentSeed configuration section when it does not exist yet.
    /// Does nothing when the section is not configured.
    /// </summary>
    public static async Task SeedDevelopmentAdminAsync(this IServiceProvider services, IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        var settings = configuration.GetSection(DevelopmentSeedSettings.SectionName).Get<DevelopmentSeedSettings>();
        if (string.IsNullOrWhiteSpace(settings?.AdminEmail) || string.IsNullOrWhiteSpace(settings.AdminPassword))
        {
            return;
        }

        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (await dbContext.Users.AnyAsync(u => u.Email == settings.AdminEmail, cancellationToken))
        {
            return;
        }

        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        dbContext.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Admin",
            LastName = "Development",
            Email = settings.AdminEmail,
            PasswordHash = passwordHasher.Hash(settings.AdminPassword),
            Role = Domain.Enums.Role.SuperAdmin
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        Log.Information("Development admin user {Email} created", settings.AdminEmail);
    }
}
