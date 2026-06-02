namespace RevitTrackingComparison.Core.Abstractions;

/// <summary>
/// UI-facing port to trigger a manual snapshot. The Revit implementation marshals the capture onto
/// a valid API context (ExternalEvent) and offloads the write so Revit is not blocked.
/// </summary>
public interface ISnapshotCommands
{
    /// <param name="progress">
    /// Optional callback for capture vs. persist phases. When created on the UI thread
    /// (e.g. <see cref="Progress{T}"/>), handlers are marshalled back to that context.
    /// </param>
    Task<SnapshotResult> TakeSnapshotAsync(IProgress<SnapshotProgress>? progress = null);
}