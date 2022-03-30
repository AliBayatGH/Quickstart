using Microsoft.IdentityModel.Tokens;

namespace WebApplication1.Extensions;

internal static class AuthenticationServiceCollectionExtensions
{
    // Add services to the container.
    public static IServiceCollection AddCustomAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                //options.Authority = "https://demo.duendesoftware.com";
                options.Authority = "https://localhost:5001";
                options.Audience = "api1";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "email"
                };
            });

        return services;
    }
}