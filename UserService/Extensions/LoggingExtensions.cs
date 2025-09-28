using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using UserService.Logging;

namespace UserService.Extensions;

public static class LoggingExtensions
{
    public static void ConfigureSerilogLogging(this WebApplicationBuilder builder)
    {
        var logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
        Directory.CreateDirectory(logDirectory);

        builder.Services.AddSingleton(new LogFileOptions(logDirectory));

        builder.Host.UseSerilog((_, _, configuration) =>
        {
            configuration
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Information)
                    .WriteTo.File(Path.Combine(logDirectory, "info.log"), rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7, shared: true))
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Warning)
                    .WriteTo.File(Path.Combine(logDirectory, "warnings.log"), rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7, shared: true))
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e => e.Level >= LogEventLevel.Error)
                    .WriteTo.File(Path.Combine(logDirectory, "errors.log"), rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7, shared: true));
        });
    }
}
