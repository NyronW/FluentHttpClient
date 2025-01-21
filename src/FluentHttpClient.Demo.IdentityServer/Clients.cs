using Duende.IdentityServer.Models;

namespace FluentHttpClient.Demo.IdentityServer;

internal static class Clients
{
    public static IEnumerable<Client> Get()
    {
        return
        [
            new Client
            {
                ClientId = "oauthClient",
                ClientName = "Example client application using client credentials",
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets = [new("SuperSecretPassword".Sha256())], // change me!
                AllowedScopes = ["api1.read","api1.write"]
            }
        ];
    }
}
