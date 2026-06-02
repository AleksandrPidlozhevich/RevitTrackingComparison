using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using RevitTrackingComparison.Revit.Infrastructure;

namespace RevitTrackingComparison.Revit.Commands;

[Transaction(TransactionMode.ReadOnly)]
[Regeneration(RegenerationOption.Manual)]
public sealed class ShowSnapshotHubCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
    {
        return CommandHost.Run<SnapshotHubLauncher>(ref message, launcher => launcher.Open(commandData.Application));
    }
}