using System.Collections.Concurrent;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitTrackingComparison.Core.Abstractions;

namespace RevitTrackingComparison.Revit.Metadata;

/// <summary>
/// ExternalEvent handler that builds the "category -> available parameters" catalog from the active
/// document. Runs on the Revit API thread (required for reading the model).
/// </summary>
internal sealed class ModelMetadataExternalEventHandler : IExternalEventHandler
{
    // Cap elements scanned per category: instance parameters are consistent across instances, so a
    // sample is enough to gather the available names without scanning very large models in full.
    private const int MaxElementsPerCategory = 200;

    private readonly IPluginLogger _logger;
    private readonly ConcurrentQueue<TaskCompletionSource<IReadOnlyDictionary<string, IReadOnlyList<string>>>> _queue = new();

    public ModelMetadataExternalEventHandler(IPluginLogger logger) => _logger = logger;

    public void Enqueue(TaskCompletionSource<IReadOnlyDictionary<string, IReadOnlyList<string>>> completion)
        => _queue.Enqueue(completion);

    public void Execute(UIApplication app)
    {
        while (_queue.TryDequeue(out var completion))
        {
            try
            {
                var doc = app.ActiveUIDocument?.Document;
                if (doc is null)
                {
                    _logger.Warn("Model metadata read skipped: no active document.");
                    completion.TrySetResult(Empty());
                    return;
                }

                completion.TrySetResult(BuildCatalog(doc));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to read model metadata.");
                completion.TrySetException(ex);
            }
        }
    }

    public string GetName() => "RevitTrackingComparison.ModelMetadata";

    private static IReadOnlyDictionary<string, IReadOnlyList<string>> Empty()
        => new Dictionary<string, IReadOnlyList<string>>();

    private static IReadOnlyDictionary<string, IReadOnlyList<string>> BuildCatalog(Document doc)
    {
        var parameters = new Dictionary<string, SortedSet<string>>(StringComparer.OrdinalIgnoreCase);
        var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var element in new FilteredElementCollector(doc).WhereElementIsNotElementType())
        {
            var category = element.Category?.Name;
            if (string.IsNullOrEmpty(category))
                continue;

            counts.TryGetValue(category, out var seen);
            if (seen >= MaxElementsPerCategory)
                continue;
            counts[category] = seen + 1;

            if (!parameters.TryGetValue(category, out var names))
            {
                names = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
                parameters[category] = names;
            }

            foreach (Parameter parameter in element.Parameters)
            {
                var name = parameter.Definition?.Name;
                if (!string.IsNullOrEmpty(name))
                    names.Add(name);
            }
        }

        return parameters.ToDictionary(
            kv => kv.Key,
            kv => (IReadOnlyList<string>)kv.Value.ToList(),
            StringComparer.OrdinalIgnoreCase);
    }
}
