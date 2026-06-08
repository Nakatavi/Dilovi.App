
namespace Application;

public static class DependencyInjections
{
    public static IServiceCollection AddApplication(this IServiceCollection services,
           IConfiguration configuration, IHostEnvironment env) => services
        .AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);
        })

        ;


    public static IServiceCollection AddAppSettings(this IServiceCollection services, IConfiguration configuration) => services;
}
public class ApplicationAssemblyMarker
{
}
