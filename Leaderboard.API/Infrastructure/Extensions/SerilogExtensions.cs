using System;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;

namespace Leaderboard.API.Infrastructure.Extensions;

public static class SerilogExtensions
{
    private static IDictionary<string, Action<LoggerConfiguration>> LogKeys { get; } =
        new Dictionary<string, Action<LoggerConfiguration>>
        {
                { "LogContext", (loggerConfiguration) => loggerConfiguration.Enrich.FromLogContext() },
                { "ExceptionDetails", (loggerConfiguration) => loggerConfiguration.Enrich.WithExceptionDetails() },
                { "ThreadId", (loggerConfiguration) => loggerConfiguration.Enrich.WithThreadId() }
        };


    public static LoggerConfiguration ConfigureLogging(this LoggerConfiguration loggerConfiguration,
        WebApplicationBuilder builder)
        => loggerConfiguration
            .SetMinimumLevel()
            .AddLogKeys()
            .ConfigureSink();

    private static LoggerConfiguration SetMinimumLevel(this LoggerConfiguration loggerConfiguration)
    {

        string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        bool isDevelopment = string.Equals(environment, Environments.Development, StringComparison.OrdinalIgnoreCase);

        return isDevelopment
            ? loggerConfiguration
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Debug)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Debug)
                .MinimumLevel.Override("System", LogEventLevel.Debug)
            : loggerConfiguration
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Information);
    }

    private static LoggerConfiguration AddLogKeys(this LoggerConfiguration loggerConfiguration, IConfiguration configuration = null)
    {
        foreach (Action<LoggerConfiguration> LogKey in LogKeys.Values)
            LogKey(loggerConfiguration);

        return loggerConfiguration;
    }

    private static LoggerConfiguration ConfigureSink(this LoggerConfiguration loggerConfiguration)
    {

        return loggerConfiguration
                .WriteTo
                .Async(a => a
                .Console(outputTemplate:
                    "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj} {Properties:j}{NewLine}{Exception}"));
    }
}
