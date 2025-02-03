using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//builder.Services.AddControllers();
//// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOcelot();
builder.Services.AddSwaggerForOcelot(builder.Configuration);

builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
    .Enrich.FromLogContext()
    .ReadFrom.Configuration(ctx.Configuration));

builder.Configuration.AddJsonFile("ocelot.json");

var app = builder.Build();

//// Configure the HTTP request pipeline.

app.UseSwaggerForOcelotUI(opt =>
{
    var serverOcelot = app.Configuration["ServerOcelot"]; // Could be replaced by a non-existing functionality https://github.com/dotnet/aspnetcore/issues/5898
    opt.PathToSwaggerGenerator = "/swagger/docs";
    opt.DownstreamSwaggerEndPointBasePath = $"{serverOcelot}/swagger/docs";
    opt.ServerOcelot = $"{serverOcelot}";
});

app.UseHttpsRedirection();

app.UseAuthorization();

//app.MapControllers();
app.UseOcelot().Wait();
app.Run();
