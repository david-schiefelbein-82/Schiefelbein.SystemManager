using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using Schiefelbein.Common.Web.Oidc;
using Schiefelbein.Common.Web.ActiveDirectory;
using Schiefelbein.Common.Web.ActiveDirectory.Config;
using Schiefelbein.SystemManager.Configuration;
using Schiefelbein.SystemManager.Managers;
using Schiefelbein.Common.Web.Oidc.Config;
using Serilog;
using System.Text;
using Schiefelbein.SystemManager.Data;
using Schiefelbein.Common.Web;
using Schiefelbein.Common.Web.Configuration;

namespace Schiefelbein.SystemManager
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            OidcKeyCache oidcKeyCache;

            try
            {
                oidcKeyCache = OidcKeyCache.Load(OidcKeyCache.DefaultConfigName);
            }
            catch
            {
                oidcKeyCache = new OidcKeyCache(OidcKeyCache.DefaultConfigName);
            }

            var configManager = new ConfigManager();
            builder.Services.AddSingleton<IConfigManager>(configManager);
            builder.Services.AddTransient(x => configManager.WebServer);
            builder.Services.AddTransient<IOidcConfig>(x => configManager.WebServer.Oidc);
            builder.Services.AddSingleton<IOidcKeyCache>(oidcKeyCache);
            builder.Services.AddSingleton<IOidcClient, OidcClient>();
            builder.Services.AddSingleton<IJwtSecurityTokenProvider, JwtSecurityTokenProvider>();
            builder.Services.AddTransient<IAuthorizationConfig>(x => configManager.Authorization);
            builder.Services.AddTransient<IActiveDirectoryConfig>(x => configManager.WebServer.ActiveDirectory);
            builder.Services.AddTransient<IActiveDirectoryUserAuthenticator, ActiveDirectoryUserAuthenticator>();
            builder.Services.AddTransient<IJwtTokenAuthenticationConfig>(x => configManager.WebServer.JwtTokenAuthentication);
            builder.Services.AddSingleton<ISystemManagerSelector, SystemManagerSelector>();
            builder.Services.AddSingleton<IServiceManager, ServiceManager>();

            var key = configManager.WebServer.JwtTokenAuthentication.GetTokenSigningKey();
            builder.Services.AddAuthentication(auth =>
            {
                auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
             .AddJwtBearer(token =>
             {
                 token.RequireHttpsMetadata = false;
                 token.SaveToken = true;
                 token.TokenValidationParameters = new TokenValidationParameters
                 {
                     ValidateIssuerSigningKey = true,
                     IssuerSigningKey = new SymmetricSecurityKey(key),
                     ValidateIssuer = true,
                     ValidIssuer = configManager.WebServer.JwtTokenAuthentication.WebSiteDomain,
                     ValidateAudience = true,
                     ValidAudience = configManager.WebServer.JwtTokenAuthentication.WebSiteDomain,
                     RequireExpirationTime = true,
                     ValidateLifetime = true,
                     ClockSkew = TimeSpan.Zero
                 };
             });

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = configManager.WebServer.SessionIdleTimeout;
            });

            builder.Services.AddHttpClient();

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var loggingConfig = new ConfigurationBuilder()
               .AddJsonFile("Config\\logging.json", optional: false, reloadOnChange: true).Build();

            builder.Host.UseSerilog((ctx, lc) => lc
                .ReadFrom.Configuration(loggingConfig));

            var app = builder.Build();

            var logger = app.Services.GetService<ILogger<Program>>();

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            logger?.LogInformation("FileManager Starting {config}", configManager);
            
            if (configManager != null && configManager.WebServer.UseHttpsRedirection)
            {
                app.UseHttpsRedirection();
            }

            app.UseStaticFiles();

            app.UseSession();
            app.UseRouting();

            app.Use(async (context, next) =>
            {
                if (!string.IsNullOrWhiteSpace(configManager?.WebServer.PathBase))
                    context.Request.PathBase = configManager.WebServer.PathBase;

                var jwtToken = context.Session.GetString("jwtToken");
                if (!string.IsNullOrEmpty(jwtToken))
                {
                    context.Request.Headers.Append("Authorization", "Bearer " + jwtToken);
                }
                await next();
            });

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}