using System;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using IdEpi.WebEpiserver.Business.Owin;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.Security;

[assembly: OwinStartup(typeof(Startup))]
namespace IdEpi.WebEpiserver.Business.Owin
{
    public class Startup
    {
        /// <summary>
        /// https://www.johanbostrom.se/blog/setting-up-episerver-to-use-openid-connect-with-identityserver
        /// https://github.com/zarxor/EPiServer.OidcExample
        /// </summary>
        /// <param name="app"></param>
        public void Configuration(IAppBuilder app)
        {
            Console.WriteLine("OWIN Startup");
            //System.Diagnostics.Debugger.Launch();

            //JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();
            //JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);
            app.UseCookieAuthentication(new CookieAuthenticationOptions()
            //{
            //    AuthenticationType = CookieAuthenticationDefaults.AuthenticationType
            //}
            );

            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                //SignInAsAuthenticationType = "Cookies",

                ClientId = "webclient",
                //ClientSecret = "secret",
                Authority = "http://localhost:5000",
                ResponseType = "code id_token token",

                //PostLogoutRedirectUri = "http://localhost:5030/",
                RedirectUri = "http://localhost:5030/",

                RequireHttpsMetadata = false,
                
                Scope = "api1 openid profile email role offline_access",
                //Scope = "api1 offline_access",

                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    NameClaimType = ClaimTypes.NameIdentifier,
                    RoleClaimType = ClaimTypes.Role,
                },

                UseTokenLifetime = false,

                // Notifications is events in netcore
                // https://github.com/IdentityServer/IdentityServer3/issues/2457
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    //AuthenticationFailed = context =>
                    //{
                    //    context.HandleResponse();
                    //    context.Response.Write(context.Exception.Message);
                    //    return Task.FromResult(0);
                    //},
                    SecurityTokenValidated = context =>
                    {
                        Console.WriteLine("-- SecurityTokenValidated");
                        var redirectUri = new Uri(context.AuthenticationTicket.Properties.RedirectUri,
                            UriKind.RelativeOrAbsolute);
                        if (redirectUri.IsAbsoluteUri)
                            context.AuthenticationTicket.Properties.RedirectUri = redirectUri.PathAndQuery;

                        ServiceLocator.Current.GetInstance<ISynchronizingUserService>()
                            .SynchronizeAsync(context.AuthenticationTicket.Identity);

                        return Task.FromResult(0);
                    }
                }
                
                
            });

            app.UseStageMarker(PipelineStage.Authenticate);

            //app.Map("http://localhost:5030/", config =>
            //{
            //    config.Run(ctx =>
            //    {
            //        if (ctx.Authentication.User == null || !ctx.Authentication.User.Identity.IsAuthenticated)
            //            ctx.Response.StatusCode = 401;
            //        else
            //            ctx.Response.Redirect("/");
            //        return Task.FromResult(0);
            //    });
            //});

            //app.Map(UrlLogout, config =>
            //{
            //    config.Run(ctx =>
            //    {
            //        ctx.Authentication.SignOut();
            //        return Task.FromResult(0);
            //    });
            //});
        }
    }
}