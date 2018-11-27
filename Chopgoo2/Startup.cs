using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Chopgoo2
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            //Environment = env;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        //public IHostingEnvironment Environment { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                sharedOptions.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
                //.AddCookie()
                .AddOpenIdConnect(o =>
                {
                    o.ClientId = Configuration["oidc:clientid"];
                    o.ClientSecret = Configuration["oidc:clientsecret"]; // for code flow
                    o.Authority = Configuration["oidc:authority"];

                    o.ResponseType = OpenIdConnectResponseType.CodeIdToken;
                    o.SaveTokens = true;
                    o.GetClaimsFromUserInfoEndpoint = true;

                    o.ClaimActions.MapAllExcept("aud", "iss", "iat", "nbf", "exp", "aio", "c_hash", "uti", "nonce");

                    o.Events = new OpenIdConnectEvents()
                    {
                        OnAuthenticationFailed = c =>
                        {
                            c.HandleResponse();

                            c.Response.StatusCode = 500;
                            c.Response.ContentType = "text/plain";
                            //if (Environment.IsDevelopment())
                            {
                                // Debug only, in production do not share exceptions with the remote host.
                                return c.Response.WriteAsync(c.Exception.ToString());
                            }
                            return c.Response.WriteAsync("An error occurred processing your authentication.");
                        }
                    };
                });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseStaticFiles();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
