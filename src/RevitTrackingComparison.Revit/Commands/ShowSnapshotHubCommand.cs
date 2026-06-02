using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using RevitTrackingComparison.Revit.Infrastructure;

namespace RevitTrackingComparison.Revit.Commands;

/// <summary>
/// Ribbon command: opens the snapshot hub (take snapshot / settings / compare) for the active project.
/// Revit creates this by reflection, so it resolves its one collaborator via <see cref="CommandHost"/>.
/// </summary>
[Transaction(TransactionMode.ReadOnly)]
[Regeneration(RegenerationOption.Manual)]
public sealed class ShowSnapshotHubCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        => CommandHost.Run<SnapshotHubLauncher>(ref message, launcher => launcher.Open(commandData.Application));
}
