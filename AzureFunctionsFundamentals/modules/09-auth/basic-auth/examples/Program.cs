using AzureFunctionsFundamentals.Modules.Auth.BasicAuth.Examples;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);
builder.ConfigureFunctionsWebApplication();

builder.Services.AddSingleton<ICredentialStore, ConfigurationCredentialStore>();
builder.Services.AddSingleton<BasicAuthenticator>();

builder.Build().Run();
