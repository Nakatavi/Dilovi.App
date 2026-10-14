namespace Infrastructure.Settings;

/// <summary>
/// Credentials of the admin user created on start in Development.
/// Keep them in user-secrets, not in appsettings files.
/// </summary>
public class DevelopmentSeedSettings
{
    public const string SectionName = "DevelopmentSeed";

    public string? AdminEmail { get; set; }
    public string? AdminPassword { get; set; }
}
