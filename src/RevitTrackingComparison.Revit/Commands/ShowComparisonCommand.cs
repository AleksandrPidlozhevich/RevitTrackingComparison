using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Microsoft.Extensions.DependencyInjection;
using RevitTrackingComparison.Core.Abstractions;
using RevitTrackingComparison.Revit.Application;
using RevitTrackingComparison.Revit.Infrastructure;
using RevitTrackingComparison.UI.Services;

namespace RevitTrackingComparison.Revit.Commands;

[Transaction(TransactionMode.ReadOnly)]
[Regeneration(RegenerationOption.Manual)]
public sealed class ShowComparisonCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
    {
        var services = RevitTrackingApplication.Services;
        if (services is null)
        {
            message = "The add-in is not initialized.";
            return Result.Failed;
        }

        try
        {
            services.GetRequiredService<RevitContext>().UiApplication = commandData.Application;

            var tracking = services.GetRequiredService<ISnapshotTrackingService>();
            var view = services.GetRequiredService<IComparisonView>();

            var diff = tracking.CaptureAndCompare();
            view.Show(diff);

            return Result.Succeeded;
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, "Failed to execute comparison command.");
            message = ex.Message;
            return Result.Failed;
        }
    }
}