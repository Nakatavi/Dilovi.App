
namespace Application;

public static class DependencyInjections
{
    public static IServiceCollection AddApplication(this IServiceCollection services,
           IConfiguration configuration, IHostEnvironment env) => services
        .AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);
        });
    
}
public class ApplicationAssemblyMarker
{
}
