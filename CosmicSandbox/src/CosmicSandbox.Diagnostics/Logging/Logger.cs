using Serilog;
using Serilog.Events;

namespace CosmicSandbox.Diagnostics.Logging;

/// <summary>
/// Centralized logging system
/// </summary>
public static class Logger
{
    private static ILogger? _logger;

    public static void Initialize(string? logPath = null)
    {
        logPath ??= Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CosmicSandbox",
            "Logs",
            $"CosmicSandbox_{DateTime.Now:yyyyMMdd_HHmmss}.log"
        );

        Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);

        _logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information)
            .WriteTo.File(
                logPath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
            )
            .CreateLogger();

        _logger.Information("Cosmic Sandbox logging initialized");
        _logger.Information("Log file: {LogPath}", logPath);
    }

    public static void Debug(string message) => _logger?.Debug(message);

    public static void Information(string message) => _logger?.Information(message);

    public static void Warning(string message) => _logger?.Warning(message);

    public static void Error(string message) => _logger?.Error(message);

    public static void Error(Exception exception, string message) => _logger?.Error(exception, message);

    public static void Fatal(Exception? exception, string message) => _logger?.Fatal(exception, message);

    public static void Shutdown()
    {
        _logger?.Information("Shutting down logging");
        Log.CloseAndFlush();
    }
}
