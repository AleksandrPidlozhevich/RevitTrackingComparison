using Autodesk.Revit.DB;
using RevitTrackingComparison.Core.Abstractions;
using RevitTrackingComparison.Core.Domain;

namespace RevitTrackingComparison.Revit.Snapshots;

/// <summary>
/// Captures a <see cref="DocumentSnapshot"/> from a Revit document, recording only the categories
/// and parameters listed in the capture configuration.
/// </summary>
public sealed class RevitSnapshotProvider
{
    private readonly ICaptureSettingsStore _settingsStore;

    public RevitSnapshotProvider(ICaptureSettingsStore settingsStore)
    {
        _settingsStore = settingsStore;
    }

    /// <summary>
    /// Begins an incremental capture: the configured elements are listed up front, then read in slices
    /// via <see cref="CaptureSession.ProcessBatch"/> so the API thread can be yielded between slices.
    /// Reads the model, so it must run on the Revit API thread. Returns null if there is no document.
    /// </summary>
    public CaptureSession? BeginCapture(Document? doc)
    {
        if (doc is null)
            return null;

        var settings = _settingsStore.Load();

        var ids = new FilteredElementCollector(doc)
            .WhereElementIsNotElementType()
            .Where(e => e.Category is not null && settings.IncludesCategory(e.Category.Name))
            .Select(e => e.Id)
            .ToList();

        return new CaptureSession(doc, settings, ids);
    }

    /// <summary>
    /// Captures the whole document in a single pass. Convenience for non-interactive callers (e.g. the
    /// open trigger) where slicing isn't needed. Reads the model, so it must run on the API thread.
    /// </summary>
    public DocumentSnapshot? Capture(Document? doc)
    {
        var session = BeginCapture(doc);
        if (session is null)
            return null;

        while (!session.IsComplete)
            session.ProcessBatch(int.MaxValue);

        return session.BuildSnapshot();
    }
}
