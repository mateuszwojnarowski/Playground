using System.Text;
using AzureFunctionsFundamentals.Modules.Auth.BasicAuth.Exercise;
using Xunit;

namespace BasicAuthExercise.Tests;

public sealed class BasicAuthenticatorTests
{
    private static BasicAuthenticator CreateAuthenticator() =>
        new(new InMemoryCredentialStore(new Dictionary<string, string>
        {
            ["alice"] = "s3cret",
        }));

    private static string BasicHeader(string username, string password) =>
        "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));

    [Fact]
    public void ValidCredentialsAuthenticate()
    {
        var result = CreateAuthenticator().Authenticate(BasicHeader("alice", "s3cret"));

        Assert.True(result.Succeeded);
        Assert.Equal("alice", result.Principal?.Identity?.Name);
    }

    [Fact]
    public void WrongPasswordIsRejected()
    {
        var result = CreateAuthenticator().Authenticate(BasicHeader("alice", "wrong"));

        Assert.False(result.Succeeded);
        Assert.Null(result.Principal);
    }

    [Fact]
    public void UnknownUserIsRejected()
    {
        var result = CreateAuthenticator().Authenticate(BasicHeader("eve", "s3cret"));

        Assert.False(result.Succeeded);
    }

    [Fact]
    public void MissingHeaderIsRejected()
    {
        var result = CreateAuthenticator().Authenticate(null);

        Assert.False(result.Succeeded);
        Assert.Equal("Missing or malformed Basic credentials.", result.Error);
    }

    [Theory]
    [InlineData("Negotiate abc123")] // wrong scheme
    [InlineData("Basic not-base64!!")]
    [InlineData("Basic " + "bm9jb2xvbg==")] // base64 of "nocolon"
    public void MalformedHeadersAreRejected(string header)
    {
        var result = CreateAuthenticator().Authenticate(header);

        Assert.False(result.Succeeded);
    }

    private sealed class InMemoryCredentialStore(IReadOnlyDictionary<string, string> credentials) : ICredentialStore
    {
        public bool TryGetExpectedPassword(string username, out string? password)
        {
            var found = credentials.TryGetValue(username, out var value);
            password = value;
            return found;
        }
    }
}
