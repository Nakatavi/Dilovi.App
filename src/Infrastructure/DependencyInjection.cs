using RunMethodsSequentially;
using static Microsoft.AspNetCore.Hosting.Internal.HostingApplication;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Infrastructure;

public static class DependencyInjection
{
    private static readonly ILogger _logger = Log.ForContext(typeof(DependencyInjection));
    public static IServiceCollection AddInfrastructure(this IServiceCollection services
        , IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("DefaultConnection");

        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString, b =>
            b.MigrationsAssembly("Migrations.PostgreSQL"));
        });

        services.AddDatabase(configuration);

        return services;
    }

    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        //Http context must be added befor 
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        var connection = configuration.GetConnectionString("DefaultConnection");

        return services;
    }

    //  Add-Migration Init -project Migrations.PostgreSQL -StartupProject Condominium -Context AppDbContext
}
