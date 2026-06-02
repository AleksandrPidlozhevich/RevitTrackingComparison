using Autodesk.Revit.DB;
using RevitTrackingComparison.Core.Abstractions;
using RevitTrackingComparison.Core.Domain;

namespace RevitTrackingComparison.Revit.Snapshots;

public sealed class RevitSnapshotProvider
{
    private readonly ICaptureSettingsStore _settingsStore;

    public RevitSnapshotProvider(ICaptureSettingsStore settingsStore)
    {
        _settingsStore = settingsStore;
    }

    // Lists the configured elements up front; the caller reads them in slices. Must run on the API thread.
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

    // Single-pass capture for non-interactive callers (open trigger), where slicing isn't needed.
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