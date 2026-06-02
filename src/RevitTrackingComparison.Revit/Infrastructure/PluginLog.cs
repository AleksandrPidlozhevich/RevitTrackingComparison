using System.IO;
using System.Threading;
using NLog;
using NLog.Config;
using NLog.Targets;
using RevitTrackingComparison.Core.Abstractions;
using RevitTrackingComparison.Core.Configuration;

namespace RevitTrackingComparison.Revit.Infrastructure;

// Owns NLog configuration (one file per day under %AppData%\TrackingComparison\log) and hands out
// named loggers. Application code injects IPluginLogger<T>; static infrastructure that cannot use DI
// calls PluginLog.For(category). Configuration is idempotent and self-healing: any entry point
// triggers it, so loggers obtained before Initialize() still write to the file target.
public static class PluginLog
{
    private const string Layout =
        "${longdate} [${level:uppercase=true}] ${logger:shortName=true} - ${message}"
        + "${onexception:${newline}${exception:format=tostring}}";

    private static int _configured;

    public static void Initialize()
    {
        EnsureConfigured();
    }

    public static IPluginLogger For(string category)
    {
        EnsureConfigured();
        return new NLogPluginLogger(LogManager.GetLogger(category));
    }

    internal static Logger LoggerFor(Type type)
    {
        EnsureConfigured();
        return LogManager.GetLogger(type.FullName ?? type.Name);
    }

    private static void EnsureConfigured()
    {
        if (Interlocked.Exchange(ref _configured, 1) == 1)
            return;

        try
        {
            Apply(new FileTarget("file")
            {
                FileName = Path.Combine(TrackingDataPaths.LogDirectory, "revit-tracking-${shortdate}.log")
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"RevitTrackingComparison: failed to configure file logging: {ex.Message}");
            TryConfigureFallbackTarget();
        }
    }

    private static void TryConfigureFallbackTarget()
    {
        try
        {
            var fallbackPath = Path.Combine(Path.GetTempPath(), "RevitTrackingComparison.log");
            Apply(new FileTarget("fallback") { FileName = fallbackPath });
            System.Diagnostics.Debug.WriteLine($"RevitTrackingComparison: logging to fallback file '{fallbackPath}'.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"RevitTrackingComparison: fallback logging failed: {ex.Message}");
        }
    }

    private static void Apply(FileTarget target)
    {
        target.Layout = Layout;
        target.KeepFileOpen = false;
        target.Encoding = System.Text.Encoding.UTF8;

        var config = new LoggingConfiguration();
        config.AddTarget(target);
        config.AddRule(LogLevel.Info, LogLevel.Fatal, target);
        LogManager.Configuration = config;
    }
}