using Autodesk.Revit.DB;
using RevitTrackingComparison.Core.Abstractions;
using RevitTrackingComparison.Core.Domain;
using RevitTrackingComparison.Revit.Infrastructure;

namespace RevitTrackingComparison.Revit.Snapshots;

/// <summary>
/// Captures a <see cref="DocumentSnapshot"/> from a Revit document, recording only the categories
/// and parameters listed in the capture configuration.
/// </summary>
public sealed class RevitSnapshotProvider
{
    private readonly ICaptureSettingsStore _settingsStore;

    public RevitSnapshotProvider(ICaptureSettingsStore settingsStore)
    {
        _settingsStore = settingsStore;
    }

    /// <summary>Captures the given document. Reads the model, so it must run on the Revit API thread.</summary>
    public DocumentSnapshot? Capture(Document? doc)
    {
        if (doc is null)
            return null;

        var settings = _settingsStore.Load();

        var elements = new FilteredElementCollector(doc)
            .WhereElementIsNotElementType()
            .Where(e => e.Category is not null && settings.IncludesCategory(e.Category.Name))
            .Select(e => Map(e, settings))
            .ToList();

        return new DocumentSnapshot
        {
            DocumentKey = RevitDocumentKey.Compute(doc),
            Title = doc.Title,
            CapturedAt = DateTime.Now,
            Elements = elements
        };
    }

    private static ElementSnapshot Map(Element element, CaptureSettings settings)
    {
        var category = element.Category?.Name ?? string.Empty;
        return new ElementSnapshot
        {
            UniqueId = element.UniqueId,
            ElementId = element.Id.Value,
            Category = category,
            Name = SafeName(element),
            Parameters = ReadParameters(element, settings.ParametersFor(category))
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

    private static Dictionary<string, string> ReadParameters(Element element, IReadOnlyList<string> names)
    {
        var result = new Dictionary<string, string>();
        foreach (var name in names)
        {
            if (result.ContainsKey(name))
                continue;

            var parameter = element.LookupParameter(name);
            if (parameter is null || !parameter.HasValue)
                continue;

            var value = parameter.AsValueString() ?? parameter.AsString();
            if (value is not null)
                result[name] = value;
        }

        return result;
    }
}
