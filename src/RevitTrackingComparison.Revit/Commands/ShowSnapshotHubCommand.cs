using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Microsoft.Extensions.DependencyInjection;
using RevitTrackingComparison.Revit.Application;
using RevitTrackingComparison.Revit.Infrastructure;
using RevitTrackingComparison.UI.Services;

namespace RevitTrackingComparison.Revit.Commands;

/// <summary>
/// Ribbon command: opens the snapshot hub (take snapshot / settings / compare) for the active project.
/// </summary>
[Transaction(TransactionMode.ReadOnly)]
[Regeneration(RegenerationOption.Manual)]
public sealed class ShowSnapshotHubCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
    {
        var services = RevitTrackingApplication.Services;
        if (services is null)
        {
            message = "Plugin is not initialized.";
            return Result.Failed;
        }

        try
        {
            services.GetRequiredService<RevitContext>().UiApplication = commandData.Application;

            var doc = commandData.Application.ActiveUIDocument?.Document;
            if (doc is null)
            {
                message = "No active document.";
                return Result.Failed;
            }

            var project = RevitDocumentKey.Compute(doc);
            services.GetRequiredService<ISnapshotHubView>().Show(project);
            return Result.Succeeded;
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, "Failed to open the snapshot hub.");
            message = ex.Message;
            return Result.Failed;
        }
    }
}
