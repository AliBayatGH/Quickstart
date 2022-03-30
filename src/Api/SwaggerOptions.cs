using IdentityModel.Client;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebApplication1;

public class SwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IConfiguration _config;
    private readonly ILogger<SwaggerOptions> _logger;

    public SwaggerOptions(IConfiguration config, ILogger<SwaggerOptions> logger)
    {
        _config = config;
        _logger = logger;
    }

    public void Configure(SwaggerGenOptions options)
    {
        try
        {
            var disco = GetDiscoveryDocument();
            var oauthScopes = new Dictionary<string, string>
            {
                { "api1", "Resource access: api1" },
                //{ "openid", "OpenID information"},
                //{ "profile", "User profile information" },
                //{ "email", "User email address" }
            };
            options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri(disco.AuthorizeEndpoint),
                        TokenUrl = new Uri(disco.TokenEndpoint),
                        Scopes = oauthScopes
                    }
                }
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "oauth2"
                        }
                    },
                    oauthScopes.Keys.ToArray()
                }
            });

            options.OperationFilter<AuthorizeCheckOperationFilter>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error loading discovery document for Swagger UI");
        }
    }

    private DiscoveryDocumentResponse GetDiscoveryDocument()
    {
        var client = new HttpClient();
        //var authority = "https://demo.duendesoftware.com";
        var authority = "https://localhost:5001";
        return client.GetDiscoveryDocumentAsync(authority)
            .GetAwaiter()
            .GetResult();
    }
}