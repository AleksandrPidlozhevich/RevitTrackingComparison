using System.IO;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace RevitTrackingComparison.Revit.Infrastructure;

public static class PluginLog
{
    private static readonly Logger Logger = Configure();

    public static void Info(string message)
    {
        Logger.Info(message);
    }

    public static void Warn(string message)
    {
        Logger.Warn(message);
    }

    public static void Error(Exception? ex, string message)
    {
        if (ex is null)
            Logger.Error(message);
        else
            Logger.Error(ex, message);
    }

    private static Logger Configure()
    {
        try
        {
            // %AppData%\TrackingComparison\log — same data root as snapshots and capture config.
            var logDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "TrackingComparison",
                "log");

            var fileTarget = new FileTarget("file")
            {
                FileName = Path.Combine(logDirectory, "revit-tracking-${shortdate}.log"),
                Layout = "${longdate} [${level:uppercase=true}] ${message}"
                         + "${onexception:${newline}${exception:format=tostring}}",
                KeepFileOpen = false,
                Encoding = System.Text.Encoding.UTF8
            };

            var config = new LoggingConfiguration();
            config.AddTarget(fileTarget);
            config.AddRule(LogLevel.Info, LogLevel.Fatal, fileTarget);
            LogManager.Configuration = config;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"RevitTrackingComparison: failed to configure file logging: {ex.Message}");
            TryConfigureFallbackTarget();
        }

        return LogManager.GetCurrentClassLogger();
    }

    private static void TryConfigureFallbackTarget()
    {
        try
        {
            var fallbackPath = Path.Combine(Path.GetTempPath(), "RevitTrackingComparison.log");
            var fileTarget = new FileTarget("fallback")
            {
                FileName = fallbackPath,
                Layout = "${longdate} [${level:uppercase=true}] ${message}"
                         + "${onexception:${newline}${exception:format=tostring}}",
                KeepFileOpen = false,
                Encoding = System.Text.Encoding.UTF8
            };

            var config = new LoggingConfiguration();
            config.AddTarget(fileTarget);
            config.AddRule(LogLevel.Info, LogLevel.Fatal, fileTarget);
            LogManager.Configuration = config;
            System.Diagnostics.Debug.WriteLine($"RevitTrackingComparison: logging to fallback file '{fallbackPath}'.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"RevitTrackingComparison: fallback logging failed: {ex.Message}");
        }
    }
}