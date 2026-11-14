using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;

// Training-only API-key scheme. Use a real identity provider for production.
public sealed class DemoKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IConfiguration _configuration;
    public DemoKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IConfiguration configuration) : base(options, logger, encoder)
    {
        _configuration = configuration;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-Demo-Key", out var supplied) || supplied.Count != 1)
            return Task.FromResult(AuthenticateResult.NoResult());
        var key = supplied.ToString();
        if (string.IsNullOrWhiteSpace(key))
            return Task.FromResult(AuthenticateResult.Fail("Invalid API key"));

        string? role = null;
        if (Matches(key, _configuration["DemoAuth:AdminKey"])) role = "Admin";
        else if (Matches(key, _configuration["DemoAuth:CashierKey"])) role = "Cashier";
        if (role is null) return Task.FromResult(AuthenticateResult.Fail("Invalid API key"));

        var identity = new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.NameIdentifier, role), new Claim(ClaimTypes.Role, role)
        }, Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(
            new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name)));
    }

    private static bool Matches(string supplied, string? configured)
    {
        if (string.IsNullOrEmpty(configured)) return false;
        var a = SHA256.HashData(Encoding.UTF8.GetBytes(supplied));
        var b = SHA256.HashData(Encoding.UTF8.GetBytes(configured));
        return CryptographicOperations.FixedTimeEquals(a, b);
    }
}
