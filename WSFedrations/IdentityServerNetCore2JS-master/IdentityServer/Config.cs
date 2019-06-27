using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
using System.Collections.Generic;
using System.Security.Claims;


namespace IdentityServer
{
    public class Config
    {
        // scopes define the API resources in your system
        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("DM_secure_API", "Secure api by DM88")
            };
        }

        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };
        }

        // client want to access resources (aka scopes)
        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "diegomary",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,                    

                    ClientSecrets =
                    {   //cryptografic hash
                        new Secret("dmpassword".Sha256())
                    },
                    AllowedScopes = { "DM_secure_API" },
                    AllowedCorsOrigins = { "http://localhost:53387" },
                },
                new Client
                {
                    ClientId = "mvc",
                    ClientName = "MVC Client",
                    AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,
                    RequireConsent = true,
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    ClientUri="http://localhost:5002",
                    RedirectUris = { "http://localhost:5002/signin-oidc" },
                    PostLogoutRedirectUris = { "http://localhost:5002/signout-callback-oidc" },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "DM_secure_API"
                    },
                    AllowOfflineAccess = true
                },
                // JavaScript Client
                new Client
                {
                    ClientId = "js",
                    ClientName = "JavaScript Client",
                    RequireConsent = true,
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,

                    RedirectUris =           { "http://localhost:5003/callback.html" },
                    PostLogoutRedirectUris = { "http://localhost:5003/index.html" },
                    AllowedCorsOrigins =     { "http://localhost:5003" },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "DM_secure_API"
                    }
                },
                new Client
                {
                    ClientId = "ro.client",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    ClientSecrets = {new Secret("secret".Sha256())}, AllowedScopes = {"DM_secure_API"}
                }
            };
        }

        public static List<TestUser> GetTestUsers()
        {
            return new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = "1",
                    Username = "diego",
                    Password = "diegopassword",
                    Claims = new []
                    {
                        new Claim("name", "Diego Aldo Burlando"),
                        new Claim("website", "https://www.dmm888.com")
                    }
                },
                new TestUser
                {
                    SubjectId = "2",
                    Username = "maria",
                    Password = "mariapassword",                   
                    Claims = new []
                    {
                        new Claim("name", "Maria Valentina Burlando"),
                        new Claim("website", "https://www.dmm888.com")
                    }
                }
            };
        }
             

    }
}
