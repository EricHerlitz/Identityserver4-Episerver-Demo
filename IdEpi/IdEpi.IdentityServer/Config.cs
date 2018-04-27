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

        private static readonly Secret _secret = new Secret("secret".Sha256());

        public static IEnumerable<ApiResource> GetApis()
        {
            return new ApiResource[]
            {
                new ApiResource("ConsoleApi", "Console API"),
                new ApiResource("api1", "My API")
                {
                    ApiSecrets = { _secret },
                    // Include the following using claims in JWT Access Tokens (in addition to subject id)
                    UserClaims =
                    {
                        ClaimTypes.Role,
                    },

                },
            };
        }

        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new IdentityResource[]
            {
                new IdentityResources.OpenId()
                {
                    // The claims here is included in UserInfoClient requests 
                    // https://msdn.microsoft.com/en-us/library/microsoft.identitymodel.claims.claimtypes_members.aspx
                    UserClaims =
                    {
                        //JwtClaimTypes.PreferredUserName,
                        //JwtClaimTypes.Role,
                        //JwtClaimTypes.Email,
                        //ClaimTypes.Upn, // username
                        //ClaimTypes.Country,
                        //ClaimTypes.GroupSid,
                        ClaimTypes.Role
                    }
                },
                new IdentityResources.Profile(),
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
                // simple client
                new Client
                {
                    ClientId = "client",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets = new List<Secret> { _secret },
                    AllowedScopes = new List<string> { "ConsoleApi" , "api1" } // api1 IdentityServerConstants.StandardScopes.OpenId, 
                },
                // resource owner password grant client
                new Client
                {
                    ClientId = "ro.client",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,
                    ClientSecrets = new List<Secret> { _secret },
                    AccessTokenType = AccessTokenType.Jwt,
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        //IdentityServerConstants.StandardScopes.Profile,
                        "api1" // we need the api-name here as IdentityServerAuthentication use it for ref tokens
                    }
                },
                // resource owner password grant client with reference tokens
                new Client
                {
                    ClientId = "ro.client.reference",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,
                    ClientSecrets = new List<Secret> { _secret },
                    AccessTokenType = AccessTokenType.Reference,
                    //AllowOfflineAccess = true, // activate refreshtoken
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        "api1", // we need the api-name here as IdentityServerAuthentication use it for ref tokens
                        //"offline_access"
                    }
                },
                // HybridAndClientCredentials client for web applications
                new Client
                {
                    ClientId = "webclient",
                    ClientName = "Web Client",
                    ClientSecrets = new List<Secret> { _secret },
                    //ClientUri = "http://10.11.12.13:5000",

                    // FrontChannelLogoutUri for single signout
                    FrontChannelLogoutUri = "http://10.11.12.13:5020/signout-oidc",
                    RequireConsent = false,
                    AccessTokenType = AccessTokenType.Reference, // disable for jwt token
                    AllowAccessTokensViaBrowser = true,
                    AlwaysIncludeUserClaimsInIdToken = true,
                    RedirectUris =
                    {
                        "http://10.11.12.13:5020/signin-oidc",
                    },
                    PostLogoutRedirectUris =
                    {
                        "http://10.11.12.13:5020/signout-callback-oidc",
                    },
                    AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId, // OpenID Connect requests MUST contain the openid scope value
                        IdentityServerConstants.StandardScopes.Profile,
                        "api1",
                    },
                    // offline_access scope - this allows requesting refresh tokens for long lived API access
                    AllowOfflineAccess = true,
                    AlwaysSendClientClaims = true
                },
                // HybridAndClientCredentials client for web applications
                new Client
                {
                    ClientId = "epiclient",
                    ClientName = "Epi Client",
                    ClientSecrets = new List<Secret> { _secret },
                    //ClientUri = "http://10.11.12.13:5000",

                    // FrontChannelLogoutUri for single signout
                    FrontChannelLogoutUri = "http://10.11.12.13:5030/signout-oidc",
                    RequireConsent = false,
                    AccessTokenType = AccessTokenType.Reference, // disable for jwt token
                    AllowAccessTokensViaBrowser = true,
                    AlwaysIncludeUserClaimsInIdToken = true,
                    RedirectUris =
                    {
                        "http://10.11.12.13:5030/",
                        "http://10.11.12.13:5030/episerver",
                        "http://10.11.12.13:5030/login",
                    },
                    PostLogoutRedirectUris =
                    {
                        "http://10.11.12.13:5030/signout-callback-oidc",
                    },
                    AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId, // OpenID Connect requests MUST contain the openid scope value
                        IdentityServerConstants.StandardScopes.Profile,
                        "api1",
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
                    SubjectId = "Alice Minion",
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
                        new Claim(ClaimTypes.Role, "WebEditors"),
                        new Claim(ClaimTypes.Role, "WebAdmins"),
                        new Claim(JwtClaimTypes.WebSite, "https://alice.se"),
                        //new Claim(JwtClaimTypes.Role, "WebEditors"), // For api
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
