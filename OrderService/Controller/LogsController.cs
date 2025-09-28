using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrderService.Logging;

namespace OrderService.Controllers;

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

    private static readonly Regex LogLineRegex = new(
        "^(?<timestamp>\\d{4}-\\d{2}-\\d{2} \\d{2}:\\d{2}:\\d{2}\\.\\d{3} [+-]\\d{2}:\\d{2}) \\\[(?<level>[A-Z]{3})\\\] (?<message>.*)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly IReadOnlyDictionary<string, string> LevelDisplayNames =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["INF"] = "Information",
            ["WRN"] = "Warning",
            ["ERR"] = "Error",
            ["FTL"] = "Fatal",
            ["DBG"] = "Debug",
            ["VRB"] = "Verbose"
        };

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
            return Ok(Array.Empty<LogEntry>());
        }

        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(stream);
        ct.ThrowIfCancellationRequested();
        var content = await reader.ReadToEndAsync();

        var entries = ParseLogEntries(content);

        return Ok(entries);
    }

    private static IReadOnlyList<LogEntry> ParseLogEntries(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return Array.Empty<LogEntry>();
        }

        var entries = new List<LogEntry>();
        using var reader = new StringReader(content);
        string? line;

        while ((line = reader.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var match = LogLineRegex.Match(line);
            if (!match.Success)
            {
                entries.Add(new LogEntry(null, null, line));
                continue;
            }

            var timestampText = match.Groups["timestamp"].Value;
            DateTimeOffset? timestamp = null;
            if (DateTimeOffset.TryParse(
                    timestampText,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out var parsedTimestamp))
            {
                timestamp = parsedTimestamp;
            }

            var levelCode = match.Groups["level"].Value;
            var levelDisplay = LevelDisplayNames.TryGetValue(levelCode, out var display)
                ? display
                : levelCode;

            var message = match.Groups["message"].Value;

            entries.Add(new LogEntry(timestamp, levelDisplay, message));
        }

        return entries;
    }

    private sealed record LogEntry(DateTimeOffset? Timestamp, string? Level, string Message);
}
