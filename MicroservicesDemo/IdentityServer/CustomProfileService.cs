using Duende.IdentityServer.AspNetIdentity;
using Duende.IdentityServer.Models;
using IdentityServer.Data.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace IdentityServer;

public class CustomProfileService(
    UserManager<User> userManager,
    IUserClaimsPrincipalFactory<User> claimsFactory)
    : ProfileService<User>(userManager, claimsFactory)
{
    protected override async Task GetProfileDataAsync(ProfileDataRequestContext context, User user)
    {
        var principal = await GetUserClaimsAsync(user);
        if (principal.Identity is ClaimsIdentity id)
        {
            context.IssuedClaims.AddRange(id.Claims);
        }
    }
}
