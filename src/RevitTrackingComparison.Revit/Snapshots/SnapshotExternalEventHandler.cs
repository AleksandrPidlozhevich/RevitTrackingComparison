using System.Collections.Concurrent;
using Autodesk.Revit.UI;
using RevitTrackingComparison.Core.Abstractions;
using RevitTrackingComparison.Core.Domain;
using RevitTrackingComparison.Revit.Infrastructure;

namespace RevitTrackingComparison.Revit.Snapshots;

/// <summary>
/// ExternalEvent handler for manual snapshots. Captures the active document on the Revit API thread
/// (required), then offloads the LiteDB write to a background thread so Revit is not blocked.
/// </summary>
internal sealed class SnapshotExternalEventHandler : IExternalEventHandler
{
    private readonly RevitSnapshotProvider _provider;
    private readonly ISnapshotStore _store;
    private readonly ConcurrentQueue<SnapshotRequest> _queue = new();

    private readonly record struct SnapshotRequest(
        TaskCompletionSource<SnapshotResult> Completion,
        IProgress<SnapshotProgress>? Progress);

    public SnapshotExternalEventHandler(RevitSnapshotProvider provider, ISnapshotStore store)
    {
        _provider = provider;
        _store = store;
    }

    public void Enqueue(TaskCompletionSource<SnapshotResult> completion, IProgress<SnapshotProgress>? progress) =>
        _queue.Enqueue(new SnapshotRequest(completion, progress));

    public void Execute(UIApplication app)
    {
        while (_queue.TryDequeue(out var request))
        {
            var completion = request.Completion;
            var progress = request.Progress;
            try
            {
                var doc = app.ActiveUIDocument?.Document;
                if (doc is null)
                {
                    completion.TrySetResult(SnapshotResult.Fail("No active document."));
                    continue;
                }

                progress?.Report(new SnapshotProgress(SnapshotProgressPhase.Capturing));

                var snapshot = _provider.Capture(doc);
                if (snapshot is null)
                {
                    completion.TrySetResult(SnapshotResult.Fail("Nothing was captured."));
                    continue;
                }

                progress?.Report(new SnapshotProgress(SnapshotProgressPhase.Saving, snapshot.Elements.Count));

                Task.Run(() => Persist(snapshot, completion));
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, "Failed to capture snapshot.");
                completion.TrySetResult(SnapshotResult.Fail(ex.Message));
            }
        }
    }

    public string GetName() => "RevitTrackingComparison.TakeSnapshot";

    private void Persist(DocumentSnapshot snapshot, TaskCompletionSource<SnapshotResult> completion)
    {
        try
        {
            var info = _store.Save(snapshot.DocumentKey, snapshot);
            completion.TrySetResult(SnapshotResult.Ok(info));
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, "Failed to persist snapshot.");
            completion.TrySetResult(SnapshotResult.Fail(ex.Message));
        }
    }
}
