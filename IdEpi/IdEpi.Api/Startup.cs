using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.AccessTokenValidation;
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

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddMvc();

            services.AddMvcCore(options =>
                {
                    // require scope1 or scope2
                    var policy = ScopePolicy.Create("api1", "openid");
                    options.Filters.Add(new AuthorizeFilter(policy));
                })
                .AddAuthorization()
                .AddJsonFormatters();

            //JwtBearerDefaults.AuthenticationScheme
            // id services
            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme) //"Bearer"
                //.AddJwtBearer(options =>
                //{
                //    // http://docs.identityserver.io/en/release/topics/apis.html
                //    // base-address of your identityserver
                //    options.Authority = "http://localhost:5000";

                //    // name of the API resource
                //    options.Audience = "api1";
                //})
                // Supporting reference tokens
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = "http://localhost:5000";
                    options.RequireHttpsMetadata = false;
                    options.ApiName = "api1";
                    options.ApiSecret = "secret";
                    options.EnableCaching = true;
                });
            //.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
            //{
            //    options.SignInScheme = "Cookies";

            //    options.Authority = "http://localhost:5000";
            //    options.RequireHttpsMetadata = false;

            //    options.ClientId = "webclient";
            //    options.ClientSecret = "secret";
            //    options.ResponseType = "code id_token";
            //    options.GetClaimsFromUserInfoEndpoint = true;
            //    options.SaveTokens = true;
            //});
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
