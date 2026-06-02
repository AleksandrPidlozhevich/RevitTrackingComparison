using RevitTrackingComparison.Core.Abstractions;

namespace RevitTrackingComparison.Revit.Infrastructure;

public sealed class NLogPluginLogger : IPluginLogger
{
    public void Info(string message) => PluginLog.Info(message);

    public void Warn(string message) => PluginLog.Warn(message);

    public void Error(Exception? exception, string message) => PluginLog.Error(exception, message);
}
