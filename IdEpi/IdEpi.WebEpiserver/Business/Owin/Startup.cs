using System;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using IdentityModel.Client;
using IdEpi.WebEpiserver.Business.Owin;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Notifications;

[assembly: OwinStartup(typeof(Startup))]
namespace IdEpi.WebEpiserver.Business.Owin
{
    public class Startup
    {
        public static string Authority = "http://10.11.12.13:5000";

        /// <summary>
        /// https://www.johanbostrom.se/blog/setting-up-episerver-to-use-openid-connect-with-identityserver
        /// https://github.com/zarxor/EPiServer.OidcExample
        /// </summary>
        /// <param name="app"></param>
        public void Configuration(IAppBuilder app)
        {
            Console.WriteLine("OWIN Startup");
            //System.Diagnostics.Debugger.Launch();

            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);
            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                ClientId = "epiclient",
                ClientSecret = "secret",
                Authority = Authority,
                ResponseType = "code id_token token",

                PostLogoutRedirectUri = "http://10.11.12.13:5030/",
                RedirectUri = "http://10.11.12.13:5030/",

                RequireHttpsMetadata = false,

                //Scope = "api1 openid profile email role offline_access",
                Scope = "api1 openid",

                TokenValidationParameters = new TokenValidationParameters
                {
                    //SaveSigninToken = true, // save token for BootstrapContext
                    ValidateIssuer = false,
                    NameClaimType = ClaimTypes.NameIdentifier,
                    RoleClaimType = ClaimTypes.Role,
                },

                UseTokenLifetime = false,

                // Notifications is events in netcore
                // https://github.com/IdentityServer/IdentityServer3/issues/2457
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    // AuthenticationFailed
                    AuthenticationFailed = context =>
                    {
                        context.HandleResponse();
                        context.Response.Write(context.Exception.Message);
                        Console.WriteLine(context.Exception.Message);
                        return Task.FromResult(0);
                    },
                    // RedirectToIdentityProvider
                    RedirectToIdentityProvider = context =>
                    {
                        // When trying to login
                        if (context.ProtocolMessage.RedirectUri == null)
                        {
                            var currentUrl = SiteDefinition.Current.SiteUrl;
                            context.ProtocolMessage.RedirectUri = new UriBuilder(
                                currentUrl.Scheme,
                                currentUrl.Host,
                                currentUrl.Port,
                                HttpContext.Current.Request.Url.AbsolutePath).ToString();
                        }

                        // If the user is trying to access 
                        if (context.OwinContext.Response.StatusCode == 401 &&
                            context.OwinContext.Authentication.User.Identity.IsAuthenticated)
                        {
                            context.OwinContext.Response.StatusCode = 403;
                            context.HandleResponse();
                        }
                        return Task.FromResult(0);
                    },
                    // If token validation was successful
                    SecurityTokenValidated = context =>
                    {
                        var redirectUri = new Uri(context.AuthenticationTicket.Properties.RedirectUri, UriKind.RelativeOrAbsolute);
                        if (redirectUri.IsAbsoluteUri)
                        {
                            context.AuthenticationTicket.Properties.RedirectUri = redirectUri.PathAndQuery;
                        }

                        // https://docs.sitefinity.com/request-access-token-for-calling-web-services
                        // Save accesstoken in a claim
                        var id = context.AuthenticationTicket.Identity;
                        id.AddClaim(new System.Security.Claims.Claim("access_token", context.ProtocolMessage.AccessToken));

                        // Contains user identity information as well as additional authentication state.
                        context.AuthenticationTicket = new Microsoft.Owin.Security.AuthenticationTicket(id, context.AuthenticationTicket.Properties);

                        // Sync identity to database
                        ServiceLocator.Current.GetInstance<ISynchronizingUserService>().SynchronizeAsync(context.AuthenticationTicket.Identity);

                        return Task.FromResult(0);
                    },
                    AuthorizationCodeReceived = async n =>
                    {
                        // https://www.scottbrady91.com/Identity-Server/Identity-Server-3-Standalone-Implementation-Part-3
                    }
                }


            });

            // configures previously registered middleware components to run on the authentication stage of the pipeline
            app.UseStageMarker(PipelineStage.Authenticate);

            // override default epi logout page to identityserver
            app.Map("/util/logout.aspx", config =>
            {
                config.Run(ctx =>
                {
                    ctx.Authentication.SignOut();
                    return Task.FromResult(0);
                });
            });
        }
    }
}