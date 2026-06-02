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

    private ControlledApplication? _application;

    public DocumentEventRouter(RevitSnapshotProvider provider, ISnapshotStore store)
    {
        _provider = provider;
        _store = store;
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
                return;

            // Capture on the API thread (required); persist off-thread so Revit stays responsive.
            var snapshot = _provider.Capture(doc);
            if (snapshot is not null)
                Task.Run(() => Persist(project, snapshot));
        });
    }

    private void Persist(string project, DocumentSnapshot snapshot)
    {
        try
        {
            _store.Save(project, snapshot);
            PluginLog.Info($"Initial snapshot stored for '{project}' ({snapshot.Elements.Count} elements).");
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, "Failed to store initial snapshot.");
        }
    }

    private static void Safe(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, "Error while handling document open.");
        }
    }
}
