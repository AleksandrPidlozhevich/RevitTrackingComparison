using System.IO;
using System.Reflection;
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
            var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            var logDirectory = Path.Combine(assemblyDir, "logs");

            var fileTarget = new FileTarget("file")
            {
                FileName = Path.Combine(logDirectory, "revit-tracking-${shortdate}.log"),
                Layout = "${longdate} [${level:uppercase=true}] ${message}"
                         + "${onexception:${newline}${exception:format=tostring}}",
                KeepFileOpen = false,
                ConcurrentWrites = true,
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