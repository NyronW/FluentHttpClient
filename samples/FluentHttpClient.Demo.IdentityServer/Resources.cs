﻿using Duende.IdentityServer.Models;

namespace FluentHttpClient.Demo.IdentityServer;

internal class Resources
{
    public static IEnumerable<IdentityResource> GetIdentityResources()
    {
        return
        [
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email(),
            new IdentityResource
            {
                Name = "role",
                UserClaims = new List<string> {"role"}
            }
        ];
    }

    public static IEnumerable<ApiResource> GetApiResources()
    {
        return
        [
            new ApiResource
            {
                Name = "api1",
                DisplayName = "API #1",
                Description = "Allow the application to access API #1 on your behalf",
                Scopes = ["api1.read", "api1.write"],
                ApiSecrets = [new Secret("ScopeSecret".Sha256())], // change me!
                UserClaims = ["role"]
            }
        ];
    }

    public static IEnumerable<ApiScope> GetApiScopes()
    {
        return
        [
            new ApiScope("api1.read", "Read Access to API #1"),
            new ApiScope("api1.write", "Write Access to API #1")
        ];
    }
}