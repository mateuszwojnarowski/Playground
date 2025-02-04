using Duende.IdentityServer.AspNetIdentity;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Test;
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
        var id = (ClaimsIdentity)principal.Identity;

        //context.AddRequestedClaims(principal.Claims);

        context.IssuedClaims.AddRange(id.Claims);
    }
}
