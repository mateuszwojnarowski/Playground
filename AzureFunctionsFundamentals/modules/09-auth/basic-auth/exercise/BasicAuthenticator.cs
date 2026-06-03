using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace AzureFunctionsFundamentals.Modules.Auth.BasicAuth.Exercise;

public sealed record BasicAuthResult(bool Succeeded, ClaimsPrincipal? Principal, string? Error)
{
    public static BasicAuthResult Success(ClaimsPrincipal principal) => new(true, principal, null);
    public static BasicAuthResult Failure(string error) => new(false, null, error);
}

public interface ICredentialStore
{
    bool TryGetExpectedPassword(string username, out string? password);
}

public sealed class ConfigurationCredentialStore(IConfiguration configuration) : ICredentialStore
{
    public bool TryGetExpectedPassword(string username, out string? password)
    {
        password = configuration[$"BasicAuth:Users:{username}"];
        return !string.IsNullOrEmpty(password);
    }
}

/// <summary>
/// Validates HTTP Basic credentials. Basic auth sends the password on every
/// request, so it must only be used over HTTPS.
/// </summary>
public sealed class BasicAuthenticator(ICredentialStore credentialStore)
{
    public BasicAuthResult Authenticate(string? authorizationHeader)
    {
        if (!TryReadCredentials(authorizationHeader, out var username, out var password))
        {
            return BasicAuthResult.Failure("Missing or malformed Basic credentials.");
        }

        if (!credentialStore.TryGetExpectedPassword(username!, out var expected) || expected is null)
        {
            FixedTimeEquals(password!, password! + "x");
            return BasicAuthResult.Failure("Invalid username or password.");
        }

        if (!FixedTimeEquals(password!, expected))
        {
            return BasicAuthResult.Failure("Invalid username or password.");
        }

        var identity = new ClaimsIdentity("Basic", ClaimTypes.Name, ClaimTypes.Role);
        identity.AddClaim(new Claim(ClaimTypes.Name, username!));
        return BasicAuthResult.Success(new ClaimsPrincipal(identity));
    }

    private static bool TryReadCredentials(string? header, out string? username, out string? password)
    {
        username = null;
        password = null;
        const string prefix = "Basic ";
        if (header is null || !header.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var encoded = header[prefix.Length..].Trim();
        string decoded;
        try
        {
            decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
        }
        catch (FormatException)
        {
            return false;
        }

        var separator = decoded.IndexOf(':');
        if (separator <= 0)
        {
            return false;
        }

        username = decoded[..separator];
        password = decoded[(separator + 1)..];
        return true;
    }

    private static bool FixedTimeEquals(string a, string b) =>
        CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(a), Encoding.UTF8.GetBytes(b));
}
