using NLog;
using RevitTrackingComparison.Core.Abstractions;

namespace RevitTrackingComparison.Revit.Infrastructure;

// Adapts an NLog logger to the cross-layer IPluginLogger port.
public class NLogPluginLogger : IPluginLogger
{
    private readonly Logger _logger;

    public NLogPluginLogger(Logger logger)
    {
        _logger = logger;
    }

    public void Info(string message)
    {
        _logger.Info(message);
    }

    public void Warn(string message)
    {
        _logger.Warn(message);
    }

    public void Error(Exception? exception, string message)
    {
        if (exception is null)
            _logger.Error(message);
        else
            _logger.Error(exception, message);
    }
}

// Resolved by DI as the open generic IPluginLogger<>; logs under the category named after T.
public sealed class NLogPluginLogger<T> : NLogPluginLogger, IPluginLogger<T>
{
    public NLogPluginLogger() : base(PluginLog.LoggerFor(typeof(T)))
    {
    }
}