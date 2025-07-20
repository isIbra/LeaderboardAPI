using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Leaderboard.API.Infrastructure.Extensions;

public class SwaggerConfigureOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;
    public SwaggerConfigureOptions(IApiVersionDescriptionProvider provider)
    {
        _provider = provider;
    }
    public void Configure(SwaggerGenOptions option)
    {
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            option.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
        }

        option.EnableAnnotations();
        option.CustomSchemaIds(x => x.FullName);
        option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter a valid token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "Bearer"
        });
        option.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                        new OpenApiSecurityScheme
                                {
                                    Reference = new OpenApiReference
                                    {
                                        Type=ReferenceType.SecurityScheme,
                                        Id="Bearer"
                                    }
                                    },
                                        new string[]{}
                                }
                    });

    }
    private OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
    {
        var info = new OpenApiInfo
        {
            Title = "Leaderboard Game APIs",
            Version = description.ApiVersion.ToString(),
            Description = "APIs to manage Leaderboard Game."
        };
        return info;
    }
}
