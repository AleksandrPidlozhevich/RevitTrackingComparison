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

    public static void Error(Exception ex, string message)
    {
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
        catch
        {
        }

        return LogManager.GetCurrentClassLogger();
    }
}