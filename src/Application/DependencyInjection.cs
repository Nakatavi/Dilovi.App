using Application.Common.Behaviors;
using Domain.Billing;
using Domain.Billing.Strategies;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services,
           IConfiguration configuration, IHostEnvironment env)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // API messages are in English regardless of the server culture
        ValidatorOptions.Global.LanguageManager.Enabled = false;
        services.AddValidatorsFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);
        services.TryAddSingleton(TimeProvider.System);

        services.AddSingleton<IChargeStrategy, MeterConsumptionChargeStrategy>();
        services.AddSingleton<IChargeStrategy, AreaChargeStrategy>();
        services.AddSingleton<IChargeStrategy, PerResidentChargeStrategy>();
        services.AddSingleton<PaymentCalculator>();

        return services;
    }
}

public class ApplicationAssemblyMarker
{
}
