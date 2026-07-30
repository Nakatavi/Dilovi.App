using Application.Common.Interfaces;
using Infrastructure.Authentication;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddDatabase(configuration)
            .AddJwtSettings(configuration);

        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }

    public static IServiceCollection AddJwtSettings(this IServiceCollection services, IConfiguration configuration) => services
        .Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

    public static IServiceCollection AddAppSettings(this IServiceCollection services, IConfiguration configuration) => services;


    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            var connection = configuration.GetConnectionString("DefaultConnection");
            options.UseNpgsql(connection, b =>
            b.MigrationsAssembly("Migrations.PostgreSQL"));
        });

        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        return services;
    }
    
}
