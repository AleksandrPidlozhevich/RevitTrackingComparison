using Autodesk.Revit.DB;
using RevitTrackingComparison.Core.Abstractions;
using RevitTrackingComparison.Core.Domain;
using RevitTrackingComparison.Revit.Infrastructure;

namespace RevitTrackingComparison.Revit.Snapshots;

public sealed class RevitSnapshotProvider : IRevitSnapshotProvider
{
    private readonly RevitContext _context;

    public RevitSnapshotProvider(RevitContext context)
    {
        _context = context;
    }

    public DocumentSnapshot? CaptureActiveDocument()
    {
        var doc = _context.ActiveDocument;
        if (doc is null)
            return null;

        var elements = new FilteredElementCollector(doc)
            .WhereElementIsNotElementType()
            .Where(e => e.Category is not null)
            .Select(Map)
            .ToList();

        return new DocumentSnapshot
        {
            DocumentKey = RevitDocumentKey.Compute(doc),
            Title = doc.Title,
            CapturedAt = DateTime.Now,
            Elements = elements
        };
    }

    private static ElementSnapshot Map(Element element)
    {
        return new ElementSnapshot
        {
            UniqueId = element.UniqueId,
            ElementId = element.Id.Value,
            Category = element.Category?.Name ?? string.Empty,
            Name = SafeName(element),
            Parameters = ReadParameters(element)
        };
    }

    private static string SafeName(Element element)
    {
        try
        {
            return element.Name ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static Dictionary<string, string> ReadParameters(Element element)
    {
        var result = new Dictionary<string, string>();
        foreach (Parameter parameter in element.Parameters)
        {
            if (!parameter.HasValue)
                continue;

            var name = parameter.Definition?.Name;
            if (string.IsNullOrEmpty(name) || result.ContainsKey(name))
                continue;

            var value = parameter.AsValueString() ?? parameter.AsString();
            if (value is not null)
                result[name] = value;
        }

        return result;
    }
}