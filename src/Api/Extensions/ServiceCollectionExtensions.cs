using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using WebApplication1.Extensions;

namespace WebApplication1.Extensions;

internal static class ServiceCollectionExtensions
{
    // Add services to the container.
    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        services.AddCustomAuthentication();

        services.AddControllers();
        services.AddEndpointsApiExplorer();

        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, SwaggerOptions>();
        services.AddSwaggerGen();

        //// Alternative way
        //services.AddSwaggerGen(options =>
        //{
        //    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Protected API", Version = "v1" });

        //    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
        //    {
        //        Type = SecuritySchemeType.OAuth2,
        //        Flows = new OpenApiOAuthFlows
        //        {
        //            AuthorizationCode = new OpenApiOAuthFlow
        //            {
        //                AuthorizationUrl = new Uri("https://localhost:5001/connect/authorize"),
        //                TokenUrl = new Uri("https://localhost:5001/connect/token"),
        //                Scopes = new Dictionary<string, string>
        //        {
        //            {"api1", "Demo API - full access"}
        //        }
        //            }
        //        }
        //    });

        //    //options.OperationFilter<AuthorizeCheckOperationFilter>();
        //});

        services.AddHealthChecks();

        return services;
    }
}
