using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitTrackingComparison.Revit.Infrastructure;

public sealed class RevitContext
{
    public UIApplication? UiApplication { get; private set; }

    public Document? ActiveDocument => UiApplication?.ActiveUIDocument?.Document;

    // Single writer: refreshed on ViewActivated and at command start. Touched on the UI/API thread only.
    public void Attach(UIApplication uiApplication) => UiApplication = uiApplication;
}
