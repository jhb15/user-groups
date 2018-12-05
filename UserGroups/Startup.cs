using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using UserGroups.Models;
using UserGroups.Repositories;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;
using System.Net;
using Microsoft.AspNetCore.HttpOverrides;

namespace UserGroups
{
    public class Startup
    {
        private readonly IHostingEnvironment environment;
        private readonly IConfigurationSection appConfig;

        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            Configuration = configuration;
            this.environment = environment;
            appConfig = configuration.GetSection("UserGroups");
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IGroupRepository, GroupRepository>();
            services.AddScoped<IGroupMemberRepository, GroupMemberRepository>();

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddDbContext<UserGroupsContext>(options =>
                    options.UseMySql(Configuration.GetConnectionString("UserGroupsContext")));

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Cookies";
                options.DefaultChallengeScheme = "oidc";
            })
            .AddCookie("Cookies")
            .AddOpenIdConnect("oidc", options =>
            {
                options.SignInScheme = "Cookies";
                options.Authority = appConfig.GetValue<string>("GatekeeperUrl");
                options.ClientId = appConfig.GetValue<string>("ClientId");
                options.ClientSecret = appConfig.GetValue<string>("ClientSecret");
                options.ResponseType = "code id_token";
                options.SaveTokens = true;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.Scope.Add("profile");
                options.Scope.Add("offline_access");
                options.ClaimActions.MapJsonKey("locale", "locale");
                options.ClaimActions.MapJsonKey("user_type", "user_type");
            })
            .AddIdentityServerAuthentication("Bearer", options =>
            {
                options.Authority = appConfig.GetValue<string>("GatekeeperUrl");
                options.ApiName = appConfig.GetValue<string>("ApiResourceName");
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("Administrator", pb => pb.RequireClaim("user_type", "administrator"));

                // Coordinator policy allows both Coordinators and Administrators
                options.AddPolicy("Coordinator", pb => pb.RequireClaim("user_type", new[] { "administrator", "coordinator" }));
            });

            if (!environment.IsDevelopment())
            {
                services.Configure<ForwardedHeadersOptions>(options =>
                {
                    var proxyHost = appConfig.GetValue<string>("ReverseProxyHostname", "http://nginx");
                    var proxyAddresses = Dns.GetHostAddresses(proxyHost);
                    foreach (var ip in proxyAddresses)
                    {
                        options.KnownProxies.Add(ip);
                    }
                });
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                var pathBase = appConfig.GetValue<string>("PathBase", "/user-groups");
                RunMigrations(app);
                app.UsePathBase(pathBase);
                app.Use((context, next) =>
                {
                    context.Request.PathBase = new PathString(pathBase);
                    return next();
                });
                app.UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                });
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=GroupsManagement}/{action=Index}/{id?}");
            });
        }

        private void RunMigrations(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var serviceProvider = serviceScope.ServiceProvider;
                var dbContext = serviceProvider.GetService<UserGroupsContext>();
                dbContext.Database.Migrate();
            }
        }
    }
}
