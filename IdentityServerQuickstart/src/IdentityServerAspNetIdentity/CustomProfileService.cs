using Duende.IdentityServer.AspNetIdentity;
using Duende.IdentityServer.Models;
using IdentityServerAspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace IdentityServerAspNetIdentity
{
    public class CustomProfileService : ProfileService<ApplicationUser>
    {
        public CustomProfileService(UserManager<ApplicationUser> userManager, IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory) : base(userManager, claimsFactory)
        {
        }

        protected override async Task GetProfileDataAsync(ProfileDataRequestContext context, ApplicationUser user)
        {
            var principal = await GetUserClaimsAsync(user);
            var id = (ClaimsIdentity)principal.Identity;
            if (!string.IsNullOrEmpty(user.FavouriteColour))
            {
                id.AddClaim(new Claim("favourite_colour", user.FavouriteColour));
            }

            //context.AddRequestedClaims(principal.Claims);

            context.IssuedClaims.AddRange(id.Claims);
        }
    }
}
