using RevitTrackingComparison.Core.Abstractions;

namespace RevitTrackingComparison.Revit.Infrastructure;

public sealed class NLogPluginLoggerFactory : IPluginLoggerFactory
{
    public IPluginLogger<T> CreateLogger<T>()
    {
        return new NLogPluginLogger<T>();
    }
}