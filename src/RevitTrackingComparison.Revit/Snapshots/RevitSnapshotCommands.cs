using Autodesk.Revit.UI;
using RevitTrackingComparison.Core.Abstractions;

namespace RevitTrackingComparison.Revit.Snapshots;

// Raised via an ExternalEvent so the capture runs in a valid API context even when triggered
// from the modeless hub window.
public sealed class RevitSnapshotCommands : ISnapshotCommands, IDisposable
{
    private readonly SnapshotExternalEventHandler _handler;
    private readonly ExternalEvent _externalEvent;

    public RevitSnapshotCommands(RevitSnapshotProvider provider, ISnapshotStore store, IPluginLogger logger)
    {
        _handler = new SnapshotExternalEventHandler(provider, store, logger);
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
