using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        services.AddScoped<ExceptionMiddleware>();

        services.AddControllers();

        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen();

        var jwtSettings = configuration.GetSection("JwtSettings").Get<Infrastructure.Settings.JwtSettings>();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings?.Issuer ?? "Condominium",
                ValidAudience = jwtSettings?.Audience ?? "Condominium.Client",
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings?.SecretKey ?? "dev-secret-key-that-is-long-enough-for-hs256"))
            };
        });
        services.AddAuthorization();

        return services;
    }
}
