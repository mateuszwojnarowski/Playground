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
    // TODO: Implement Basic authentication.
    // - Parse the Authorization header, load the expected password, and compare credentials securely.
    // - Return the correct success or failure result described in README.md.
    // - Build the ClaimsPrincipal for authenticated users.
    public BasicAuthResult Authenticate(string? authorizationHeader)
    {
        throw new NotImplementedException("TODO: implement this method.");
    }

    // TODO: Implement Basic header parsing.
    // - Decode the Basic credentials and split them into username and password.
    // - Reject malformed headers exactly as the exercise README.md requires.
    private static bool TryReadCredentials(string? header, out string? username, out string? password)
    {
        throw new NotImplementedException("TODO: implement this method.");
    }

    // TODO: Implement fixed-time comparison.
    // - Compare two credential values in constant time to avoid timing leaks.
    // - Keep the implementation aligned with the security guidance in README.md.
    private static bool FixedTimeEquals(string a, string b)
    {
        throw new NotImplementedException("TODO: implement this method.");
    }
}
