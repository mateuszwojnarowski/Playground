using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AzureFunctionsFundamentals.Modules.Auth.JwtAuth.Examples;

/// <summary>
/// Demo sign-in endpoint. In a real system this would verify the user against an
/// identity store before issuing a token; here it checks a single configured demo
/// password so you can obtain a token locally and call the protected endpoint.
/// </summary>
public sealed class TokenFunction(JwtTokenService tokenService, IConfiguration configuration, ILogger<TokenFunction> logger)
{
    [Function("IssueToken")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "token")] HttpRequest request)
    {
        TokenRequest? body;
        try
        {
            body = await JsonSerializer.DeserializeAsync<TokenRequest>(
                request.Body,
                new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }
        catch (JsonException)
        {
            logger.LogWarning("Token request contained invalid JSON.");
            return new BadRequestObjectResult(new { error = "Invalid JSON body." });
        }

        if (body is null || string.IsNullOrWhiteSpace(body.Username) || string.IsNullOrWhiteSpace(body.Password))
        {
            logger.LogWarning("Token request was missing required credentials.");
            return new BadRequestObjectResult(new { error = "username and password are required." });
        }

        logger.LogInformation("Issuing token request received for {UserName}.", body.Username);

        var demoPassword = configuration["Jwt:DemoPassword"];
        if (string.IsNullOrEmpty(demoPassword) || body.Password != demoPassword)
        {
            logger.LogWarning("Token issuance failed for {UserName}.", body.Username);
            return new UnauthorizedObjectResult(new { error = "Invalid username or password." });
        }

        var token = tokenService.IssueToken(body.Username, [new Claim("role", "reader")]);
        logger.LogInformation("Token issued for {UserName} with {RoleCount} roles.", body.Username, 1);
        return new OkObjectResult(new { access_token = token, token_type = "Bearer" });
    }

    public sealed record TokenRequest(string? Username, string? Password);
}
