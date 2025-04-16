using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using ProductsService.Data;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddNewtonsoftJson();

builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console(
        outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
    .Enrich.FromLogContext()
    .ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        Console.WriteLine(builder.Configuration["IdentityServerUrl"]);
        options.Authority = builder.Configuration["IdentityServerUrl"];
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["IdentityServerUrl"],
            ValidateAudience = true,
            ValidAudience = "products",
        };
        //options.TokenValidationParameters.ValidAudiences = ["products"];
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Product Edit", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "product.edit");
        policy.RequireRole("admin");
    })
    .AddPolicy("Product Stock", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "product.edit");
        policy.RequireRole("admin", "customer");
    })
    .AddPolicy("Product View", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "product.view");
        policy.RequireRole("customer", "admin");
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ProductContext>(
    dbContextOptions => dbContextOptions.UseSqlServer(builder.Configuration["ConnectionStrings:ProductDB"]));

var app = builder.Build();

app.UseSerilogRequestLogging();

// Ensure the database is created and migrated to the latest version.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ProductContext>();
    context.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
