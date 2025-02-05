using Duende.IdentityModel;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using IdentityServer.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Security.Claims;

namespace IdentityServer.Data;

public static class SeedData
{
    public static void InitializeDatabase(IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>()!.CreateScope();
        var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureDeleted(); // this wipes the db every time it runs so that we get a clean slate. 
        context.Database.Migrate();

        var userMgr = serviceScope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var mat = userMgr.FindByNameAsync("mat").Result;
        if (mat is null)
        {
            mat = new User
            {
                UserName = "mat",
                Email = "mat@icaneverything.com"
            };
            var result = userMgr.CreateAsync(mat, "Pas123.").Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            result = userMgr.AddClaimsAsync(mat, [
                new Claim(JwtClaimTypes.Name, "Mat W"),
                new Claim(JwtClaimTypes.GivenName, "Mat"),
                new Claim(JwtClaimTypes.FamilyName, "W"),
                new Claim("role", "admin")
            ]).Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            Log.Debug("mat created");

        }

        var leon = userMgr.FindByNameAsync("leon").Result;
        if (leon is null)
        {
            leon = new User
            {
                UserName = "leon",
                Email = "leon@icandomore.com"
            };
            var result = userMgr.CreateAsync(leon, "Pas123.").Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            result = userMgr.AddClaimsAsync(leon, [
                new Claim(JwtClaimTypes.Name, "Leon W"),
                new Claim(JwtClaimTypes.GivenName, "Leon"),
                new Claim(JwtClaimTypes.FamilyName, "W"),
                new Claim("role", "customer")
            ]).Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }
            Log.Debug("leon created");
        }


        serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

        var configContext = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
        configContext.Database.Migrate();

        if (!configContext.Clients.Any())
        {
            foreach (var client in Config.Clients)
            {
                configContext.Clients.Add(client.ToEntity());
            }
            configContext.SaveChanges();
        }

        if (!configContext.IdentityResources.Any())
        {
            foreach (var resource in Config.IdentityResources)
            {
                configContext.IdentityResources.Add(resource.ToEntity());
            }
            configContext.SaveChanges();
        }

        if (!configContext.ApiScopes.Any())
        {
            foreach (var resource in Config.ApiScopes)
            {
                configContext.ApiScopes.Add(resource.ToEntity());
            }
            configContext.SaveChanges();
        }

        if (!configContext.ApiResources.Any())
        {
            foreach (var resource in Config.ApiResources)
            {
                configContext.ApiResources.Add(resource.ToEntity());
            }
            configContext.SaveChanges();
        }
    }
}
