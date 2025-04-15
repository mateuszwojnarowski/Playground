using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOcelot();
builder.Services.AddSwaggerForOcelot(builder.Configuration);

builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
    .Enrich.FromLogContext()
    .ReadFrom.Configuration(ctx.Configuration));

builder.Configuration.AddJsonFile("ocelot.json");

// Replace placeholders in Ocelot configuration
// get serilog

var orderServiceUrl = builder.Configuration["OrderServiceUrl"];
var orderServicePort = builder.Configuration["OrderServicePort"];
var productsServiceUrl = builder.Configuration["ProductsServiceUrl"];
var productsServicePort = builder.Configuration["ProductsServicePort"];

// this is to ensure we have correct values for ocelot. Should be somewhere else ideally but this project is not about making iţ pristine.
builder.Configuration["Routes:0:DownstreamHostAndPorts:0:Host"] = orderServiceUrl;
builder.Configuration["Routes:0:DownstreamHostAndPorts:0:Port"] = orderServicePort;
builder.Configuration["Routes:1:DownstreamHostAndPorts:0:Host"] = productsServiceUrl;
builder.Configuration["Routes:1:DownstreamHostAndPorts:0:Port"] = productsServicePort;

builder.Configuration["SwaggerEndPoints:0:Config:0:Url"] = $"http://{productsServiceUrl}:{productsServicePort}/swagger/v1/swagger.json";
builder.Configuration["SwaggerEndPoints:1:Config:0:Url"] = $"http://{orderServiceUrl}:{orderServicePort}/swagger/v1/swagger.json";

var app = builder.Build();

//// Configure the HTTP request pipeline.

app.UseSwaggerForOcelotUI(opt =>
{
    var serverOcelot = app.Configuration["ServerOcelot"];
    opt.PathToSwaggerGenerator = "/swagger/docs";
    opt.DownstreamSwaggerEndPointBasePath = $"{serverOcelot}/swagger/docs";
    opt.ServerOcelot = $"{serverOcelot}";
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseOcelot().Wait();
app.Run();
