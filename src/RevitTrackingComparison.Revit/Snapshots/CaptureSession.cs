using Autodesk.Revit.DB;
using RevitTrackingComparison.Core.Domain;
using RevitTrackingComparison.Revit.Infrastructure;

namespace RevitTrackingComparison.Revit.Snapshots;

// Reads a document in slices so the (single-threaded) API thread can be yielded between batches and
// the UI stays responsive. Element ids are listed up front; the expensive parameter reads are sliced.
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

    public int Total => _ids.Count;
    public int Processed => _index;
    public bool IsComplete => _index >= _ids.Count;

    // Runs on the API thread; keep the batch small so the thread is released quickly between calls.
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

    public DocumentSnapshot BuildSnapshot()
    {
        return new DocumentSnapshot
        {
            DocumentKey = RevitDocumentKey.Compute(_doc),
            Title = _doc.Title,
            CapturedAt = _capturedAt,
            Elements = _elements
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