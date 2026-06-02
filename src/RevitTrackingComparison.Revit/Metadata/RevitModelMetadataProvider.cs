using Autodesk.Revit.UI;
using RevitTrackingComparison.Core.Abstractions;

namespace RevitTrackingComparison.Revit.Metadata;

/// <summary>
/// Revit implementation of <see cref="IModelMetadataProvider"/>: reads the category/parameter
/// catalog via an ExternalEvent so it works from the modeless settings window.
/// </summary>
public sealed class RevitModelMetadataProvider : IModelMetadataProvider, IDisposable
{
    private readonly ModelMetadataExternalEventHandler _handler;
    private readonly ExternalEvent _externalEvent;

    public RevitModelMetadataProvider(IPluginLogger logger)
    {
        _handler = new ModelMetadataExternalEventHandler(logger);
        _externalEvent = ExternalEvent.Create(_handler);
    }

    public Task<IReadOnlyDictionary<string, IReadOnlyList<string>>> GetCategoryParametersAsync()
    {
        var completion = new TaskCompletionSource<IReadOnlyDictionary<string, IReadOnlyList<string>>>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        _handler.Enqueue(completion);
        _externalEvent.Raise();
        return completion.Task;
    }

    public void Dispose() => _externalEvent.Dispose();
}
