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
        new(name: "product.view", displayName: "View Products")
    ];

    public static IEnumerable<ApiResource> ApiResources =>
    [
        new ApiResource("orders", "Orders API")
        {
            Scopes = { "order.edit", "order.view" }
        },
        new ApiResource("products", "Products API")
        {
            Scopes = { "product.edit", "product.view" }
        }
    ];

    public static IEnumerable<Client> Clients =>
    [
        new()
        {
            ClientId = "client",
            Description = "Client for admin",
            ClientSecrets = { new Secret("secret".Sha256()) },
            RedirectUris = { "http://127.0.0.1" },

            AllowedGrantTypes = GrantTypes.Code,

            AllowedScopes = { "order.edit", "order.view", "product.edit", "product.view" },
            AccessTokenLifetime = 300
        },
        new()
        {
            ClientId = "client2",
            Description = "Client for user",
            ClientSecrets = { new Secret("secret".Sha256()) },
            AllowedGrantTypes = GrantTypes.Code,

            RedirectUris = { "http://127.0.0.1" },
            AllowedScopes = { "order.edit", "order.view", "product.view" },
            AccessTokenLifetime = 300
        }
    ];
}
