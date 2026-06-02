using System.Collections.Concurrent;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using RevitTrackingComparison.Core.Abstractions;
using RevitTrackingComparison.Core.Domain;
using RevitTrackingComparison.Revit.Infrastructure;

namespace RevitTrackingComparison.Revit.Snapshots;

// Manual snapshots. The API is single-threaded, so the model is read on the API thread — but in small
// slices on the Idling event (SetRaiseWithoutDelay resumes the next tick), so the UI stays responsive
// instead of freezing on one long read. The LiteDB write is then offloaded to a background thread.
internal sealed class SnapshotExternalEventHandler : IExternalEventHandler
{
    private const int BatchSize = 250;

    private readonly RevitSnapshotProvider _provider;
    private readonly ISnapshotStore _store;
    private readonly IPluginLogger<SnapshotExternalEventHandler> _logger;
    private readonly ConcurrentQueue<SnapshotRequest> _queue = new();

    private readonly record struct SnapshotRequest(
        TaskCompletionSource<SnapshotResult> Completion,
        IProgress<SnapshotProgress>? Progress);

    // State of the capture currently being driven by the Idling event (null when idle).
    private UIApplication? _app;
    private SnapshotRequest _request;
    private CaptureSession? _session;

    public SnapshotExternalEventHandler(
        RevitSnapshotProvider provider,
        ISnapshotStore store,
        IPluginLogger<SnapshotExternalEventHandler> logger)
    {
        _provider = provider;
        _store = store;
        _logger = logger;
    }

    public void Enqueue(TaskCompletionSource<SnapshotResult> completion, IProgress<SnapshotProgress>? progress)
    {
        _queue.Enqueue(new SnapshotRequest(completion, progress));
    }

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
                _logger.Warn("Manual snapshot skipped: no active document.");
                request.Completion.TrySetResult(SnapshotResult.Fail("No active document."));
                return;
            }

            _session = _provider.BeginCapture(doc);
            if (_session is null)
            {
                _logger.Warn("Manual snapshot failed: capture session could not be started.");
                request.Completion.TrySetResult(SnapshotResult.Fail("Nothing was captured."));
                return;
            }

            if (_session.Total == 0)
                _logger.Warn($"Manual snapshot for '{RevitDocumentKey.Compute(doc)}' matched zero elements.");

            request.Progress?.Report(new SnapshotProgress(SnapshotProgressPhase.Capturing, 0, _session.Total));

            // Read the model in slices on Idling so the API thread (= Revit UI) is released between them.
            _app = app;
            app.Idling += OnIdling;
        }
        catch (Exception ex)
        {
            EndSession();
            _logger.Error(ex, "Failed to start manual snapshot capture.");
            request.Completion.TrySetResult(SnapshotResult.Fail("Snapshot capture failed."));
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
            _ = PersistAsync(snapshot, completion); // SaveAsync offloads the write off the API thread
        }
        catch (Exception ex)
        {
            var completion = _request.Completion;
            EndSession();
            _logger.Error(ex, "Failed during manual snapshot capture.");
            completion.TrySetResult(SnapshotResult.Fail("Snapshot capture failed."));
        }
    }

    private void EndSession()
    {
        if (_app is not null)
            _app.Idling -= OnIdling;
        _app = null;
        _session = null;
    }

    public string GetName()
    {
        return "RevitTrackingComparison.TakeSnapshot";
    }

    private async Task PersistAsync(DocumentSnapshot snapshot, TaskCompletionSource<SnapshotResult> completion)
    {
        try
        {
            var info = await _store.SaveAsync(snapshot.DocumentKey, snapshot).ConfigureAwait(false);
            completion.TrySetResult(SnapshotResult.Ok(info));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to persist manual snapshot.");
            completion.TrySetResult(SnapshotResult.Fail("Could not save snapshot."));
        }
    }
}