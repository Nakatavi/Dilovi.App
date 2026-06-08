

public static class DependencyInjection 
{
    public static IServiceCollection AddPresentation(this IServiceCollection services,
          IConfiguration configuration, IHostEnvironment env) => services
        .AddHttpContextAccessor()
        .AddScoped<ExceptionMiddleware>();
}
