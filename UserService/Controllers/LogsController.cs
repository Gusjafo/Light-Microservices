using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserService.Logging;

namespace UserService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LogsController(LogFileOptions options, ILogger<LogsController> logger) : ControllerBase
{
    private static readonly IReadOnlyDictionary<string, string> LevelFileNames =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["info"] = "info.log",
            ["information"] = "info.log",
            ["warn"] = "warnings.log",
            ["warning"] = "warnings.log",
            ["error"] = "errors.log",
            ["errors"] = "errors.log"
        };

    private readonly string _logDirectory = options.Directory;
    private readonly ILogger<LogsController> _logger = logger;

    [HttpGet("{level}")]
    public async Task<IActionResult> GetLogs(string level, CancellationToken ct)
    {
        if (!LevelFileNames.TryGetValue(level, out var fileName))
        {
            _logger.LogWarning("Unsupported log level requested: {Level}", level);
            return BadRequest(new { error = "Unsupported log level. Use info, warning or error." });
        }

        var filePath = Path.Combine(_logDirectory, fileName);
        if (!System.IO.File.Exists(filePath))
        {
            _logger.LogInformation("Log file {File} not found. Returning empty response.", filePath);
            return Content(string.Empty, "text/plain");
        }

        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(stream);
        ct.ThrowIfCancellationRequested();
        var content = await reader.ReadToEndAsync();

        return Content(content, "text/plain");
    }
}
