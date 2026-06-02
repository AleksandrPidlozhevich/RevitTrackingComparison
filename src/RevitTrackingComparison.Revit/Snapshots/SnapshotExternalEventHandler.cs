using System.Collections.Concurrent;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using RevitTrackingComparison.Core.Abstractions;
using RevitTrackingComparison.Core.Domain;
using RevitTrackingComparison.Revit.Infrastructure;

namespace RevitTrackingComparison.Revit.Snapshots;

/// <summary>
/// ExternalEvent handler for manual snapshots. The Revit API is single-threaded, so the model must be
/// read on the API thread — but reading it all at once would freeze the UI. Instead the capture is run
/// in small slices on the <see cref="UIApplication.Idling"/> event: each tick reads one batch and then
/// returns control to Revit (<see cref="IdlingEventArgs.SetRaiseWithoutDelay"/> requests the next tick),
/// so the interface stays responsive throughout. The LiteDB write is then offloaded to a background thread.
/// </summary>
internal sealed class SnapshotExternalEventHandler : IExternalEventHandler
{
    private const int BatchSize = 250;

    private readonly RevitSnapshotProvider _provider;
    private readonly ISnapshotStore _store;
    private readonly ConcurrentQueue<SnapshotRequest> _queue = new();

    private readonly record struct SnapshotRequest(
        TaskCompletionSource<SnapshotResult> Completion,
        IProgress<SnapshotProgress>? Progress);

    // State of the capture currently being driven by the Idling event (null when idle).
    private UIApplication? _app;
    private SnapshotRequest _request;
    private CaptureSession? _session;

    public SnapshotExternalEventHandler(RevitSnapshotProvider provider, ISnapshotStore store)
    {
        _provider = provider;
        _store = store;
    }

    public void Enqueue(TaskCompletionSource<SnapshotResult> completion, IProgress<SnapshotProgress>? progress) =>
        _queue.Enqueue(new SnapshotRequest(completion, progress));

    public void Execute(UIApplication app)
    {
        // A capture is already in progress (driven by Idling); the queued request will start afterwards.
        if (_session is not null)
            return;

        if (!_queue.TryDequeue(out var request))
            return;

        _request = request;
        try
        {
            var doc = app.ActiveUIDocument?.Document;
            if (doc is null)
            {
                request.Completion.TrySetResult(SnapshotResult.Fail("No active document."));
                return;
            }

            _session = _provider.BeginCapture(doc);
            if (_session is null)
            {
                request.Completion.TrySetResult(SnapshotResult.Fail("Nothing was captured."));
                return;
            }

            request.Progress?.Report(new SnapshotProgress(SnapshotProgressPhase.Capturing, 0, _session.Total));

            // Read the model in slices on Idling so the API thread (= Revit UI) is released between them.
            _app = app;
            app.Idling += OnIdling;
        }
        catch (Exception ex)
        {
            EndSession();
            PluginLog.Error(ex, "Failed to start snapshot capture.");
            request.Completion.TrySetResult(SnapshotResult.Fail(ex.Message));
        }
    }

    private void OnIdling(object? sender, IdlingEventArgs e)
    {
        var session = _session;
        if (session is null)
            return;

        try
        {
            session.ProcessBatch(BatchSize);

            if (!session.IsComplete)
            {
                _request.Progress?.Report(
                    new SnapshotProgress(SnapshotProgressPhase.Capturing, session.Processed, session.Total));
                e.SetRaiseWithoutDelay(); // resume as soon as Revit is idle again
                return;
            }

            // Capture finished: stop listening, then persist off the API thread.
            var snapshot = session.BuildSnapshot();
            var completion = _request.Completion;
            var progress = _request.Progress;
            EndSession();

            progress?.Report(new SnapshotProgress(SnapshotProgressPhase.Saving, snapshot.Elements.Count));
            Task.Run(() => Persist(snapshot, completion));
        }
        catch (Exception ex)
        {
            var completion = _request.Completion;
            EndSession();
            PluginLog.Error(ex, "Failed during snapshot capture.");
            completion.TrySetResult(SnapshotResult.Fail(ex.Message));
        }
    }

    private void EndSession()
    {
        if (_app is not null)
            _app.Idling -= OnIdling;
        _app = null;
        _session = null;
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
