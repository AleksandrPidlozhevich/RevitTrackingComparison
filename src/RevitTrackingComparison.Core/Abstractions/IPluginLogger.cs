namespace RevitTrackingComparison.Core.Abstractions;

/// <summary>
/// Cross-layer logging port. Implemented in the Revit project (NLog file target) so Core, Persistence
/// and UI can log without referencing NLog. Inject the generic <see cref="IPluginLogger{T}"/> so each
/// log line is tagged with its source component.
/// </summary>
public interface IPluginLogger
{
    void Info(string message);

    void Warn(string message);

    void Error(Exception? exception, string message);
}

/// <summary>
/// Category-bearing logger: <typeparamref name="T"/> identifies the source component in the log output.
/// DI-built components inject this directly; components built by hand get one from
/// <see cref="IPluginLoggerFactory"/>.
/// </summary>
public interface IPluginLogger<out T> : IPluginLogger;