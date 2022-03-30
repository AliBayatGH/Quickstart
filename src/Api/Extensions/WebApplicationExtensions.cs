using Serilog;

namespace WebApplication1.Extensions;

internal static class WebApplicationExtensions
{
    // Configure the HTTP request pipeline.
    public static WebApplication Configure(this WebApplication app)
    {
        app.UseSerilogRequestLogging();
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.OAuthClientId("interactive.public.short");
                options.OAuthAppName("CarvedRock API");
                options.OAuthUsePkce();
            });
        }

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        return app;
    }
}