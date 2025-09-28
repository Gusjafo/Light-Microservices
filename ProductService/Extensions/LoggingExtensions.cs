using Microsoft.AspNetCore.Builder;
using Serilog;
using Serilog.Events;
using System.IO;

namespace ProductService.Extensions;

public static class LoggingExtensions
{
    public static void ConfigureSerilogLogging(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((_, _, configuration) =>
        {
            var logDirectory = Path.Combine(@"C:\\Logs", "ProductService");
            Directory.CreateDirectory(logDirectory);

            configuration
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Information)
                    .WriteTo.File(Path.Combine(logDirectory, "info.log"), rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7))
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Warning)
                    .WriteTo.File(Path.Combine(logDirectory, "warnings.log"), rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7))
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e => e.Level >= LogEventLevel.Error)
                    .WriteTo.File(Path.Combine(logDirectory, "errors.log"), rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7));
        });
    }
}
