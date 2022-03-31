using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace WebApplication1.Extensions;

internal static class HealthCheckServiceCollectionExtensions
{
    public static IServiceCollection AddCustomHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddSqlServer(configuration["ConnectionStrings:DefaultConnection"], failureStatus:HealthStatus.Degraded);

        return services;
    }
}