using Duende.IdentityServer.Models;

namespace IdentityServer;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
    [
        new IdentityResources.OpenId(),
        new IdentityResources.Profile(),
        new IdentityResource("role", "User Role", new []{"admin", "customer"})
    ];

    public static IEnumerable<ApiScope> ApiScopes =>
    [
        new(name: "order.edit", displayName: "Create orders"),
        new(name: "order.view", displayName: "View Orders"),
        new(name: "product.edit", displayName: "Manage Products"),
        new(name: "product.view", displayName: "View Products"),
        new(name: "product.stock", displayName: "Modify product stock") // this could have been avoided by involving delegate token but for the sake of simplicity we are using this
    ];

    public static IEnumerable<ApiResource> ApiResources =>
    [
        new ApiResource("orders", "Orders API")
        {
            Scopes = { "order.edit", "order.view" }
        },
        new ApiResource("products", "Products API")
        {
            Scopes = { "product.edit", "product.view", "product.stock" }
        }
    ];

    public static IEnumerable<Client> Clients =>
    [
        new()
        {
            ClientId = "client",
            Description = "Client for api",
            ClientSecrets = { new Secret("secret".Sha256()) },
            RedirectUris = { "http://127.0.0.1" },

            AllowedGrantTypes = GrantTypes.Code,

            AllowedScopes = { "order.edit", "order.view", "product.edit", "product.view", "product.stock" },
            AccessTokenLifetime = 300
        },
        new()
        {
            ClientId = "react-client",
            ClientName = "React SPA Client",
            AllowedGrantTypes = GrantTypes.Code,
            RequireClientSecret = false,
            RequirePkce = true,
            
            RedirectUris = { "http://localhost:3000/callback", "http://localhost:3000/silent-renew" },
            PostLogoutRedirectUris = { "http://localhost:3000" },
            AllowedCorsOrigins = { "http://localhost:3000" },
            
            AllowedScopes = { 
                "openid", 
                "profile",
                "order.edit", 
                "order.view", 
                "product.edit", 
                "product.view", 
                "product.stock" 
            },
            
            AllowAccessTokensViaBrowser = true,
            AccessTokenLifetime = 3600,
            RequireConsent = false
        }
    ];
}
