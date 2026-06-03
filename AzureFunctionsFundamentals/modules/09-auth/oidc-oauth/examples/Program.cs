using AzureFunctionsFundamentals.Modules.Auth.OidcOAuth.Examples;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

var builder = FunctionsApplication.CreateBuilder(args);
builder.ConfigureFunctionsWebApplication();

builder.Services.AddSingleton(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    return Options.Create(AuthOptions.FromConfiguration(configuration));
});
builder.Services.AddSingleton<ISigningKeyProvider, OidcSigningKeyProvider>();
builder.Services.AddSingleton<ITokenValidator, JwtBearerValidator>();

builder.Build().Run();
