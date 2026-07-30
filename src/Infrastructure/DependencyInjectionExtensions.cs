using System.Reflection;

namespace Infrastructure;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        var repositoryTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Repository") && t.GetInterfaces().Any(i => i.Name.EndsWith("Repository")));

        foreach (var repositoryType in repositoryTypes)
        {
            var repositoryInterface = repositoryType.GetInterfaces()
                .FirstOrDefault(i => i.Name == "I" + repositoryType.Name);

            if (repositoryInterface != null)
            {
                services.AddScoped(repositoryInterface, repositoryType);
            }
        }

        return services;
    }
}
