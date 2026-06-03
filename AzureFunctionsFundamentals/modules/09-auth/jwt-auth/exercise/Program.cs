using AzureFunctionsFundamentals.Modules.Auth.JwtAuth.Exercise;
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
    return Options.Create(JwtOptions.FromConfiguration(configuration));
});
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddSingleton<DocumentsAuthorizer>();

builder.Build().Run();
