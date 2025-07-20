using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerUI;
using Leaderboard.API.Infrastructure.Util;

namespace Leaderboard.API.Infrastructure.Extensions
{
    public static class BuilderExtensions
    {
        public static WebApplicationBuilder ConfigureCustomWebHost(this WebApplicationBuilder builder, string basePath)
        {
            builder.WebHost.LoadConfigurations(basePath);
            return builder;
        }

        public static WebApplicationBuilder ConfigureLocalization(this WebApplicationBuilder builder)
        {
            builder.Services.AddLocalization(o => { o.ResourcesPath = "Resources"; });

            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                CultureInfo arCulture = new("ar");
                CultureInfo enCulture = new("en");
                CultureInfo[] supportedCultures = { arCulture, enCulture };
                foreach (CultureInfo supportedCulture in supportedCultures)
                {
                    supportedCulture.NumberFormat = enCulture.NumberFormat;
                }

                options.AddInitialRequestCultureProvider(new AcceptLanguageHeaderRequestCultureProvider());
                options.DefaultRequestCulture = new RequestCulture(supportedCultures[0]);
                options.SupportedCultures = new List<CultureInfo>(supportedCultures);
                options.SupportedUICultures = supportedCultures;
                options.ApplyCurrentCultureToResponseHeaders = true;
            });

            return builder;
        }

        public static WebApplicationBuilder ConfigureLogger(this WebApplicationBuilder builder)
        {
            builder.Host.UseSerilog();
            Log.Logger = new LoggerConfiguration()
            .ConfigureLogging(builder)
            .CreateLogger();
            return builder;
        }

        public static WebApplicationBuilder ConfigureApiVersioning(this WebApplicationBuilder builder, int majorVersion,
            int minorVersion)
        {
            builder.Services.AddApiVersioning(config =>
            {
                config.DefaultApiVersion = new ApiVersion(majorVersion, minorVersion); // default version
                config.AssumeDefaultVersionWhenUnspecified = true;
                config.ReportApiVersions = true;
            });

            builder.Services.AddVersionedApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            return builder;
        }

        public static WebApplicationBuilder ConfigureAuthentication(this WebApplicationBuilder builder)
        {
            var configSettings = builder.Configuration.GetSection("ConfigSettings").Get<ConfigSettings>();
            var key = Encoding.ASCII.GetBytes(configSettings.AuthSettings.SecretKey);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = !string.IsNullOrEmpty(configSettings.AuthSettings.Issuer),
                    ValidateAudience = !string.IsNullOrEmpty(configSettings.AuthSettings.Audience),
                    ValidIssuer = configSettings.AuthSettings.Issuer,
                    ValidAudience = configSettings.AuthSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

            return builder;
        }


        public static void ConfigureSwaggerApiVersioning(this IApplicationBuilder app)
        {
            var provider = app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();

            app.UseSwagger(options =>
            {
                options.PreSerializeFilters.Add((swagger, req) =>
                {
                    swagger.Servers = new List<OpenApiServer> { new() { Url = $"https://{req.Host}" } };
                });
            });

            app.UseSwaggerUI(options =>
            {
                foreach (ApiVersionDescription desc in provider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint($"../swagger/{desc.GroupName}/swagger.json", desc.ApiVersion.ToString());
                    options.DefaultModelsExpandDepth(-1);
                    options.DocExpansion(DocExpansion.None);
                }
            });
        }


        public static void ConfigureHealthChecks(this IApplicationBuilder app)
        {
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                app.UseHealthChecks("/healthz",
                    new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("service"),
                        ResponseWriter = WriteResponse
                    });
                app.UseHealthChecks("/ready",
                    new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("service"),
                        ResponseWriter = WriteResponse
                    });
            });
        }

        private static Task WriteResponse(HttpContext context, HealthReport result)
        {
            context.Response.ContentType = "application/json";

            JObject json = new JObject(
                new JProperty("healthy", result.Status.Equals(HealthStatus.Healthy)),
                new JProperty("healthchecks", new JObject(result.Entries.Select(pair =>
                    new JProperty(pair.Key, pair.Value.Status.Equals(HealthStatus.Healthy))))));

            return context.Response.WriteAsync(
                json.ToString(Formatting.Indented));
        }
        private static IWebHostBuilder LoadConfigurations(this IWebHostBuilder builder, string basePath)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile(
                    $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json",
                    false)
                .AddEnvironmentVariables()
                .Build();

            builder.UseConfiguration(config);

            return builder;
        }
    }
}
