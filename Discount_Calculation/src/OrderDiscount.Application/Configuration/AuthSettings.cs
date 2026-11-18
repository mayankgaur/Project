namespace OrderDiscount.Application.Configuration;
public sealed class AuthSettings
{
    public const string SectionName = "AuthSettings";
    public string Mode { get; set; } = "Bypass";
    public string ActiveProvider { get; set; } = "Keycloak";
    public LocalBypassUserSettings LocalUser { get; set; } = new();
    public Dictionary<string, OAuthProviderSettings> Providers { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
public sealed class LocalBypassUserSettings
{
    public string Subject { get; set; } = "local-dev-user";
    public string Name { get; set; } = "Local Developer";
    public string Role { get; set; } = "Orders.Write";
}
public sealed class OAuthProviderSettings
{
    public string Authority { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public bool RequireHttpsMetadata { get; set; } = true;
}