
using Serilog;
using Infrastructure;


try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, config) =>
    {
        config.ReadFrom.Configuration(context.Configuration);
    });

    builder.Services
        .AddPresentation(builder.Configuration, builder.Environment)
        .AddApplication(builder.Configuration, builder.Environment)
        .AddInfrastructure(builder.Configuration);


    var app = builder.Build();

    app.MapGet("/", () => "Hello World!");

    app.Run();
}
catch (Exception ex)
{

    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}