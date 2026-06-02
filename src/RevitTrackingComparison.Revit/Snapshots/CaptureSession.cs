using Autodesk.Revit.DB;
using RevitTrackingComparison.Core.Domain;
using RevitTrackingComparison.Revit.Infrastructure;

namespace RevitTrackingComparison.Revit.Snapshots;

/// <summary>
/// An in-progress capture of a Revit document. The element list is collected up front (cheap), then
/// the expensive per-element parameter reads are done in slices via <see cref="ProcessBatch"/>. Because
/// the Revit API is single-threaded, this all runs on the API thread — but the caller can yield that
/// thread between slices (e.g. on <c>Idling</c>) so the UI stays responsive during a large capture.
/// </summary>
public sealed class CaptureSession
{
    private readonly Document _doc;
    private readonly CaptureSettings _settings;
    private readonly IReadOnlyList<ElementId> _ids;
    private readonly List<ElementSnapshot> _elements;
    private readonly DateTime _capturedAt;
    private int _index;

    internal CaptureSession(Document doc, CaptureSettings settings, IReadOnlyList<ElementId> ids)
    {
        _doc = doc;
        _settings = settings;
        _ids = ids;
        _elements = new List<ElementSnapshot>(ids.Count);
        _capturedAt = DateTime.Now; // point-in-time is the start of the capture
    }

    /// <summary>Total number of elements to read.</summary>
    public int Total => _ids.Count;

    /// <summary>Number of elements read so far.</summary>
    public int Processed => _index;

    /// <summary>True once every element has been read.</summary>
    public bool IsComplete => _index >= _ids.Count;

    /// <summary>
    /// Reads up to <paramref name="batchSize"/> more elements into the snapshot. Runs on the API thread;
    /// keep the batch small so the thread is released quickly between calls.
    /// </summary>
    public void ProcessBatch(int batchSize)
    {
        var take = Math.Min(Math.Max(batchSize, 1), _ids.Count - _index);
        var end = _index + take;
        for (; _index < end; _index++)
        {
            var element = _doc.GetElement(_ids[_index]);
            if (element is not null)
                _elements.Add(Map(element, _settings));
        }
    }

    /// <summary>Builds the finished snapshot. Call once <see cref="IsComplete"/> is true.</summary>
    public DocumentSnapshot BuildSnapshot() => new()
    {
        DocumentKey = RevitDocumentKey.Compute(_doc),
        Title = _doc.Title,
        CapturedAt = _capturedAt,
        Elements = _elements
    };

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
