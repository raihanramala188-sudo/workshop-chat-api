using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WorkshopsSite.Data;

namespace WorkshopAPI.Security;

public class ApiKeyHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger, UrlEncoder encoder, CatalogDB db)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "ApiKey";
    public const string HeaderName = "X-API-Key";
    public static string IssueKey() => WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
    public static string Digest(string key) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(key)));

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HeaderName, out var keys))
            return Task.FromResult(AuthenticateResult.NoResult());
        if (keys.Count != 1 || keys[0] is not { Length: 43 } key)
            return Task.FromResult(AuthenticateResult.Fail("Invalid API key."));

        // Match the digest so SQLite never has to store the credential sent by the browser.
        var digest = Digest(key);
        var user = db.Users.AsNoTracking().SingleOrDefault(u => u.ApiKeyHash == digest);
        if (user is null) return Task.FromResult(AuthenticateResult.Fail("Invalid API key."));

        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, user.UserName)], SchemeName);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.Headers.WWWAuthenticate = SchemeName;
        return base.HandleChallengeAsync(properties);
    }
}
