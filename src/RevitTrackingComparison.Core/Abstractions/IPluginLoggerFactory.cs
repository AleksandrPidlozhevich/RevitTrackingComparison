namespace RevitTrackingComparison.Core.Abstractions;

/// <summary>
/// Creates category-bearing loggers for components built outside DI (e.g. view-models that take a
/// runtime argument). DI-built components should inject <see cref="IPluginLogger{T}"/> directly.
/// </summary>
public interface IPluginLoggerFactory
{
    IPluginLogger<T> CreateLogger<T>();
}