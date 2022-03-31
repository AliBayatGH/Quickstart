using Serilog;
using WebApplication1.Extensions;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up");

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Logging.ClearProviders();
    builder.Host.UseSerilog((ctx, sp, lc) => lc.WithCustomConfiguration(sp, ctx.Configuration));
    builder.Services.ConfigureServices(builder.Configuration);
    var app = builder.Build();
    app.Configure();

    Log.Information(" Runs the ({ApplicationContext}) and block the calling thread until host shutdown.", Program.AppName);
    app.Run();
}
catch (Exception ex) when (ex.GetType().Name is not "StopTheHostException")
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}
public partial class Program
{
    public static string AppName = "Webapplication1";
}