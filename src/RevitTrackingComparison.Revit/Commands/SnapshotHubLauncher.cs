using Autodesk.Revit.UI;
using RevitTrackingComparison.Core.Abstractions;
using RevitTrackingComparison.Revit.Infrastructure;
using RevitTrackingComparison.UI.Services;

namespace RevitTrackingComparison.Revit.Commands;

public sealed class SnapshotHubLauncher
{
    private readonly RevitContext _context;
    private readonly ISnapshotHubView _view;
    private readonly IPluginLogger<SnapshotHubLauncher> _logger;

    public SnapshotHubLauncher(RevitContext context, ISnapshotHubView view, IPluginLogger<SnapshotHubLauncher> logger)
    {
        _context = context;
        _view = view;
        _logger = logger;
    }

    // Returns null on success, or a user-facing error message.
    public string? Open(UIApplication uiApplication)
    {
        _context.Attach(uiApplication);

        var doc = uiApplication.ActiveUIDocument?.Document;
        if (doc is null)
            return "No active document.";

        var project = RevitDocumentKey.Compute(doc);
        try
        {
            _logger.Info($"Opening snapshot hub for project '{project}'.");
            _view.Show(project);
            return null;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to open the snapshot hub.");
            return "Could not open the snapshot hub.";
        }
    }
}