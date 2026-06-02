using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using RevitTrackingComparison.Core.Abstractions;
using RevitTrackingComparison.Core.Domain;
using RevitTrackingComparison.Revit.Infrastructure;
using RevitTrackingComparison.Revit.Snapshots;

namespace RevitTrackingComparison.Revit.Events;

/// <summary>
/// Creates the first snapshot automatically when a document is opened and the project has none yet.
/// Subsequent snapshots are taken manually from the hub. Capture runs on the API thread (this event);
/// the write is offloaded so Revit is not blocked.
/// </summary>
public sealed class DocumentEventRouter : IDisposable
{
    private readonly RevitSnapshotProvider _provider;
    private readonly ISnapshotStore _store;
    private readonly IPluginLogger<DocumentEventRouter> _logger;

    private ControlledApplication? _application;

    public DocumentEventRouter(
        RevitSnapshotProvider provider,
        ISnapshotStore store,
        IPluginLogger<DocumentEventRouter> logger)
    {
        _provider = provider;
        _store = store;
        _logger = logger;
    }

    public void Subscribe(ControlledApplication application)
    {
        _application = application;
        application.DocumentOpened += OnDocumentOpened;
    }

    public void Dispose()
    {
        if (_application is null)
            return;

        _application.DocumentOpened -= OnDocumentOpened;
        _application = null;
    }

    private void OnDocumentOpened(object? sender, DocumentOpenedEventArgs e)
    {
        Safe(() =>
        {
            var doc = e.Document;
            if (doc is null)
                return;

            var project = RevitDocumentKey.Compute(doc);
            if (_store.HasSnapshots(project))
            {
                _logger.Info($"Initial snapshot skipped for '{project}': snapshots already exist.");
                return;
            }

            _logger.Info($"Creating initial snapshot for '{project}'…");
            var snapshot = _provider.Capture(doc);
            if (snapshot is null)
            {
                _logger.Warn($"Initial snapshot for '{project}' produced no data.");
                return;
            }

            if (snapshot.Elements.Count == 0)
                _logger.Warn($"Initial snapshot for '{project}' captured zero elements.");

            // Capture above ran on the API thread (required). The write is offloaded by SaveAsync,
            // so fire-and-forget here keeps the document-open event from blocking.
            _ = PersistAsync(project, snapshot);
        });
    }

    private async Task PersistAsync(string project, DocumentSnapshot snapshot)
    {
        try
        {
            await _store.SaveAsync(project, snapshot).ConfigureAwait(false);
            _logger.Info($"Initial snapshot stored for '{project}' ({snapshot.Elements.Count} elements).");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Failed to store initial snapshot for '{project}'.");
        }
    }

    private void Safe(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error while handling document open.");
        }
    }
}