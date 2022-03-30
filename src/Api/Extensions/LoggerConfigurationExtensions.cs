using Serilog.Exceptions.Core;
using Serilog;
using System.Reflection;
using System.Security.Claims;
using Serilog.Exceptions;
using Serilog.Sinks.MSSqlServer;
using Serilog.Enrichers.AspnetcoreHttpcontext;
using Serilog.Exceptions.EntityFrameworkCore.Destructurers;
using System.Data.SqlClient;
using Serilog.Filters;
using Serilog.Sinks.Elasticsearch;
using Serilog.Enrichers.Span;

namespace WebApplication1.Extensions;
public static class LoggerConfigurationExtensions
{
    public static LoggerConfiguration WithCustomConfiguration(this LoggerConfiguration loggerConfig, IServiceProvider serviceProvider, IConfiguration config)
    {
        string rollingFileName = config["Logging:RollingFileName"];
        string sqlServerConnectionString = config["Logging:SQLServerConnectionString"];
        string elasticsearchUri = config["Logging:ElasticsearchUri"];
        string elasticIndexRoot = config["Logging:ElasticIndexFormatRoot"];
        string elasticBufferRoot = config["Logging:ElasticBufferRoot"];

        ColumnOptions columnOptions = new();
        columnOptions.Properties.DataType = System.Data.SqlDbType.Xml;

        string? assemblyName = Assembly.GetEntryAssembly()?.GetName().Name;

        loggerConfig.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
        .Enrich.FromLogContext()
        .ReadFrom.Configuration(config) // minimum levels defined per project in json files 
        .Enrich.With<ActivityEnricher>()
        .Enrich.WithMachineName()
        .Enrich.WithProperty("Assembly", assemblyName);
        //if (serviceProvider is not null)
        //    loggerConfig.Enrich.WithAspnetcoreHttpcontext(serviceProvider, GetContextInfo);
        loggerConfig.Enrich.WithExceptionDetails(new DestructuringOptionsBuilder()
        .WithDefaultDestructurers()
        .WithDestructurers(new[] { new DbUpdateExceptionDestructurer() }));


        if (!string.IsNullOrWhiteSpace(rollingFileName))
            loggerConfig.WriteTo.File(rollingFileName);
        if (!string.IsNullOrWhiteSpace(sqlServerConnectionString))
            loggerConfig.WriteTo.MSSqlServer(connectionString: GetConnectionString(sqlServerConnectionString), sinkOptions: new MSSqlServerSinkOptions { TableName = "LogEvents", AutoCreateSqlTable = true }, columnOptions: columnOptions);

        if (!string.IsNullOrWhiteSpace(elasticsearchUri))
            loggerConfig.WriteTo.Logger(lc =>
                    lc.Filter.ByExcluding(Matching.WithProperty<bool>("Security", p => p))
                        .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticsearchUri))
                        {
                            AutoRegisterTemplate = true,
                            AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv6,
                            IndexFormat = elasticIndexRoot + "-{0:yyyy.MM.dd}",
                            InlineFields = true
                        }))
                .WriteTo.Logger(lc =>
                    lc.Filter.ByIncludingOnly(Matching.WithProperty<bool>("Security", p => p))
                        .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticsearchUri))
                        {
                            AutoRegisterTemplate = true,
                            AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv6,
                            IndexFormat = "security-{0:yyyy.MM.dd}",
                            InlineFields = true
                        }));

        return loggerConfig;
    }

    private static ContextInformation GetContextInfo(IHttpContextAccessor httpContextAccessor)
    {
        HttpContext httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return null;
        }

        return new ContextInformation
        {
            RemoteIpAddress = httpContext.Connection.RemoteIpAddress.ToString(),
            Host = httpContext.Request.Host.ToString(),
            Method = httpContext.Request.Method,
            Protocol = httpContext.Request.Protocol,
            UserInfo = GetUserInfo(httpContext.User),
        };
    }

    private static UserInformation GetUserInfo(ClaimsPrincipal claimsPrincipal)
    {
        System.Security.Principal.IIdentity user = claimsPrincipal.Identity;
        if (user?.IsAuthenticated != true)
        {
            return null;
        }

        List<string> excludedClaims = new()
        {
            "nbf",
            "exp",
            "auth_time",
            "amr",
            "sub",
            "at_hash",
            "s_hash",
            "sid",
            "name",
            "preferred_username"
        };

        const string userIdClaimType = "sub";
        const string userNameClaimType = "name";

        var userInformation = new UserInformation
        {
            UserId = claimsPrincipal.Claims.FirstOrDefault(a => a.Type == userIdClaimType)?.Value,
            UserName = claimsPrincipal.Claims.FirstOrDefault(a => a.Type == userNameClaimType)?.Value,
            UserClaims = new Dictionary<string, List<string>>()
        };

        foreach (string distinctClaimType in claimsPrincipal.Claims
            .Where(claim => excludedClaims.All(ex => ex != claim.Type))
            .Select(claim => claim.Type)
            .Distinct())
        {
            userInformation.UserClaims[distinctClaimType] = claimsPrincipal.Claims
                .Where(a => a.Type == distinctClaimType)
                .Select(c => c.Value)
                .ToList();
        }

        return userInformation;
    }

    private static string GetConnectionString(string sqlServerConnectionString)
    {

        SqlConnectionStringBuilder connBuilder = new()
        {
            ConnectionString = sqlServerConnectionString
        };

        string dbName = connBuilder.InitialCatalog;

        var masterConnection = sqlServerConnectionString.Replace(dbName, "master");

        using (SqlConnection connection = new(masterConnection))
        {
            connection.Open();
            using var command = new SqlCommand($@"IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'{dbName}') create database [{dbName}];", connection);
            command.ExecuteNonQuery();
        }

        sqlServerConnectionString = masterConnection.Replace("master", dbName);

        return sqlServerConnectionString;
    }
}

public class ContextInformation
{
    public string Host { get; set; }
    public string Method { get; set; }
    public string RemoteIpAddress { get; set; }
    public string Protocol { get; set; }
    public UserInformation UserInfo { get; set; }
}

public class UserInformation
{
    public string UserId { get; set; }
    public string UserName { get; set; }
    public Dictionary<string, List<string>> UserClaims { get; set; }
}
