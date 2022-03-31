namespace WebApplication1.Extensions;

internal static class HealthCheckServiceCollectionExtensions
{
    public static IServiceCollection AddCustomHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddSqlServer(configuration["ConnectionStrings:DefaultConnection"]);

        return services;
    }
}