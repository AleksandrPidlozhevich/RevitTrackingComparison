using Autodesk.Revit.UI;
using RevitTrackingComparison.Core.Abstractions;

namespace RevitTrackingComparison.Revit.Snapshots;

/// <summary>
/// Revit implementation of <see cref="ISnapshotCommands"/>: a manual snapshot raised via an
/// ExternalEvent so it runs in a valid API context even when triggered from the modeless hub window.
/// </summary>
public sealed class RevitSnapshotCommands : ISnapshotCommands, IDisposable
{
    private readonly SnapshotExternalEventHandler _handler;
    private readonly ExternalEvent _externalEvent;

    public RevitSnapshotCommands(RevitSnapshotProvider provider, ISnapshotStore store)
    {
        _handler = new SnapshotExternalEventHandler(provider, store);
        _externalEvent = ExternalEvent.Create(_handler);
    }

    public Task<SnapshotResult> TakeSnapshotAsync(IProgress<SnapshotProgress>? progress = null)
    {
        var completion = new TaskCompletionSource<SnapshotResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        _handler.Enqueue(completion, progress);
        _externalEvent.Raise();
        return completion.Task;
    }

    public void Dispose() => _externalEvent.Dispose();
}
