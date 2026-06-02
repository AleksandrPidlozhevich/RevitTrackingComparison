using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using RevitTrackingComparison.Core.Abstractions;
using RevitTrackingComparison.Core.Domain;
using RevitTrackingComparison.Revit.Infrastructure;
using RevitTrackingComparison.Revit.Snapshots;

namespace RevitTrackingComparison.Revit.Events;

public sealed class DocumentEventRouter : IDisposable
{
    private readonly ISnapshotTrackingService _tracking;
    private readonly ISnapshotStore _store;

    private ControlledApplication? _application;

    public DocumentEventRouter(ISnapshotTrackingService tracking, ISnapshotStore store)
    {
        _tracking = tracking;
        _store = store;
    }

    public void Subscribe(ControlledApplication application)
    {
        _application = application;
        application.DocumentOpened += OnDocumentOpened;
        application.DocumentSaved += OnDocumentSaved;
        application.DocumentSynchronizedWithCentral += OnDocumentSynchronized;
        application.DocumentClosing += OnDocumentClosing;
    }

    public void Dispose()
    {
        if (_application is null)
            return;

        _application.DocumentOpened -= OnDocumentOpened;
        _application.DocumentSaved -= OnDocumentSaved;
        _application.DocumentSynchronizedWithCentral -= OnDocumentSynchronized;
        _application.DocumentClosing -= OnDocumentClosing;
        _application = null;
    }

    private void OnDocumentOpened(object? sender, DocumentOpenedEventArgs e)
    {
        Safe(() =>
        {
            var doc = e.Document;
            if (doc is null) return;
            _store.SaveSession(RevitDocumentKey.Compute(doc), new WorkSession { StartTime = DateTime.Now });
        });
    }

    private void OnDocumentSaved(object? sender, DocumentSavedEventArgs e)
    {
        CaptureAndPersist(e.Document);
    }

    private void OnDocumentSynchronized(object? sender, DocumentSynchronizedWithCentralEventArgs e)
    {
        CaptureAndPersist(e.Document);
    }

    private void OnDocumentClosing(object? sender, DocumentClosingEventArgs e)
    {
        Safe(() =>
        {
            var doc = e.Document;
            if (doc is null) return;
            var key = RevitDocumentKey.Compute(doc);
            _store.SaveSession(key, new WorkSession { StartTime = DateTime.Now, EndTime = DateTime.Now });
        });
    }

    private void CaptureAndPersist(Document? doc)
    {
        Safe(() =>
        {
            if (doc is null) return;
            var key = RevitDocumentKey.Compute(doc);

            // Snapshot and compare with the previous one (core business logic)
            var diff = _tracking.CaptureAndCompare();
            if (diff is { HasChanges: true })
                PluginLog.Info(
                    $"Document '{doc.Title}': +{diff.Added.Count} -{diff.Removed.Count} ~{diff.Modified.Count}");

            // Model warnings
            var warnings = doc.GetWarnings().Select(w => WarningMapper.Map(w, doc)).ToList();
            if (warnings.Count > 0)
                _store.SaveWarnings(key, warnings);
        });
    }

    private static void Safe(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, "Error while handling document event.");
        }
    }
}