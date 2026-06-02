using Autodesk.Revit.DB;

namespace RevitTrackingComparison.Revit.Infrastructure;

public static class RevitDocumentKey
{
    public static string Compute(Document doc)
    {
        var fileName = doc.Title.Replace("_" + Environment.UserName, string.Empty);
        if (doc.IsWorkshared)
            fileName += "_shared";
        return fileName;
    }
}