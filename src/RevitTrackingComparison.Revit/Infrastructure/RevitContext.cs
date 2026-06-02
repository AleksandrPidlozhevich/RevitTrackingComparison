using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitTrackingComparison.Revit.Infrastructure;

public sealed class RevitContext
{
    public UIApplication? UiApplication { get; set; }

    public Document? ActiveDocument => UiApplication?.ActiveUIDocument?.Document;
}