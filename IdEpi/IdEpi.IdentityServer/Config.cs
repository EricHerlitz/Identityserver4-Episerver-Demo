using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;

namespace IdEpi.IdentityServer
{
    public class Config
    {
        public static IEnumerable<ApiResource> GetApis()
        {
            return new ApiResource[]
            {
                //new ApiResource()
                //{
                //    Name = "api1",
                //    DisplayName = "My API",
                //    Scopes =
                //    {
                //        new Scope
                //        {
                //            Name = "api1",
                //            DisplayName = "My API",
                //        },
                //    }
                //},
                new ApiResource("api1", "My API")
                {
                    UserClaims =
                    {
                        JwtClaimTypes.Role,
                        JwtClaimTypes.Email,
                        ClaimTypes.Country,
                        ClaimTypes.Role
                    }
                },
            };
        }

        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new IdentityResource[]
            {
                new IdentityResources.OpenId()
                {
                    // https://msdn.microsoft.com/en-us/library/microsoft.identitymodel.claims.claimtypes_members.aspx
                    UserClaims =
                    {
                        JwtClaimTypes.PreferredUserName,
                        JwtClaimTypes.Role,
                        JwtClaimTypes.Email,
                        ClaimTypes.Upn, // username
                        ClaimTypes.Country,
                        ClaimTypes.GroupSid,
                        ClaimTypes.Role
                    }
                },
                new IdentityResources.Profile(),
                new IdentityResources.Email(),
                new IdentityResource
                {
                    Name = JwtClaimTypes.Role,
                    DisplayName = "Role",
                    Description = "Allow the service access to your user roles.",
                    UserClaims = new[] { JwtClaimTypes.Role, ClaimTypes.Role },
                    ShowInDiscoveryDocument = true,
                    Required = true,
                    Emphasize = true
                }
            };
        }

        /// <summary>
        /// The clients are the applications and clients allowed to use the STS
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Client> GetClients()
        {
            return new Client[]
            {
                new Client
                {
                    ClientId = "client",
                    //ClientName = "Console App",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets = new List<Secret> { new Secret("secret".Sha256()) },
                    AllowedScopes = new List<string> { "api1" }
                },
                // resource owner password grant client
                new Client
                {
                    ClientId = "ro.client",
                    //ClientName = "Console App",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,
                    ClientSecrets = new List<Secret> { new Secret("secret".Sha256()) },
                    //AlwaysSendClientClaims = true,
                    //AlwaysIncludeUserClaimsInIdToken = true,
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "api1"
                    }
                },
                new Client
                {
                    ClientId = "webclient",
                    ClientName = "Web Client",
                    ClientSecrets = new List<Secret> { new Secret("secret".Sha256()) },
                    //ClientUri = "http://localhost:5020",
                    RequireConsent = false,
                    AllowAccessTokensViaBrowser = true,
                    AlwaysIncludeUserClaimsInIdToken = true,
                    RedirectUris =
                    {
                        "http://localhost:5020/signin-oidc",
                        "http://localhost:5030/",
                        "http://localhost:5030/episerver",
                        "http://localhost:5030/login",
                    },
                    PostLogoutRedirectUris =
                    {
                        "http://localhost:5020/signout-callback-oidc",
                        "http://localhost:5030/"
                    },
                    AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.StandardScopes.OfflineAccess,
                        JwtClaimTypes.Role,
                        "api1"
                    },
                    // offline_access scope - this allows requesting refresh tokens for long lived API access
                    AllowOfflineAccess = true,
                    AlwaysSendClientClaims = true
                },
            };
        }

        public static List<TestUser> GetUsers()
        {
            return new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = "1",
                    Username = "alice",
                    Password = "pass",
                    Claims =
                    {
                        new Claim(JwtClaimTypes.PreferredUserName, "alice"),
                        new Claim(ClaimTypes.Upn, "alice"),
                        new Claim(ClaimTypes.Country, "Sweden"),
                        new Claim(JwtClaimTypes.Name, "alice"),
                        new Claim(JwtClaimTypes.Email, "alice@gmail.com"),
                        new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                        //new Claim(JwtClaimTypes.Role, "WebEditors"),
                        new Claim(ClaimTypes.Role, "WebEditors"),
                        new Claim(JwtClaimTypes.WebSite, "https://alice.se")
                    },
                },
                new TestUser
                {
                    SubjectId = "2",
                    Username = "bob",
                    Password = "pass"
                }
            };
        }

        public static IEnumerable<string> GetScopes()
        {
            return new[]
            {
                IdentityServerConstants.StandardScopes.OpenId,
                IdentityServerConstants.StandardScopes.Profile,
                IdentityServerConstants.StandardScopes.Email,
                IdentityServerConstants.StandardScopes.Address,
                IdentityServerConstants.StandardScopes.OfflineAccess
            };
        }

    }
}
