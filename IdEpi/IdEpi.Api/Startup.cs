using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IdEpi.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public const string Authority = "http://10.11.12.13:5000";

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvcCore(options =>
                {
                    var policy = ScopePolicy.Create("api1", "openid"); // "api1", "openid", "offline_access"
                    options.Filters.Add(new AuthorizeFilter(policy));
                })
                .AddAuthorization()
                .AddJsonFormatters();

            // https://github.com/IdentityServer/IdentityServer4.AccessTokenValidation
            // id services
            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme) 
                // Supporting reference tokens
                //IdentityServerAuthenticationOptions
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = Authority;
                    options.SupportedTokens = SupportedTokens.Both; // jwt and reference
                    options.LegacyAudienceValidation = true; // if you need to support both JWTs and reference token
                    options.RequireHttpsMetadata = false;
                    options.ApiName = "api1";
                    options.ApiSecret = "secret";
                    options.RoleClaimType = ClaimTypes.Role; // override standard JwtClaimTypes.Role
                    options.EnableCaching = false;
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
