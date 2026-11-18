using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderDiscount.Application.Configuration;

namespace OrderDiscount.Infrastructure.Authentication;

public sealed class LocalBypassAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, IOptionsMonitor<AuthSettings> authSettings)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "LocalBypass";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var user = authSettings.CurrentValue.LocalUser;
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Subject),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role)
        };
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(claims, SchemeName)), SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

public static class AuthExtensions
{
    public static IServiceCollection AddConfigurableAuth(this IServiceCollection services, IConfiguration config)
    {
        var auth = config.GetSection(AuthSettings.SectionName).Get<AuthSettings>() ?? new AuthSettings();
        if (string.Equals(auth.Mode, "OAuth", StringComparison.OrdinalIgnoreCase))
        {
            if (!auth.Providers.TryGetValue(auth.ActiveProvider, out var p))
                throw new InvalidOperationException($"AuthSettings:ActiveProvider '{auth.ActiveProvider}' not found.");

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(o => { o.Authority = p.Authority; o.Audience = p.Audience; o.RequireHttpsMetadata = p.RequireHttpsMetadata; });
        }
        else
        {
            services.AddAuthentication(LocalBypassAuthenticationHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, LocalBypassAuthenticationHandler>(LocalBypassAuthenticationHandler.SchemeName, _ => { });
        }
        services.AddAuthorization();
        return services;
    }
}