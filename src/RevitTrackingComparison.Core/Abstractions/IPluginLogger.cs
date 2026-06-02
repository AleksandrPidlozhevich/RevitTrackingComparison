namespace RevitTrackingComparison.Core.Abstractions;

/// <summary>
/// Cross-layer logging port. Implemented in the Revit project (NLog file target); injected
/// wherever Core or Persistence need to record failures without referencing NLog.
/// </summary>
public interface IPluginLogger
{
    void Info(string message);

    void Warn(string message);

    void Error(Exception? exception, string message);
}
