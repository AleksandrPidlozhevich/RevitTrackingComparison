using Autodesk.Revit.UI;
using RevitTrackingComparison.Core.Abstractions;

namespace RevitTrackingComparison.Revit.Editing;

public sealed class RevitModelEditor : IModelEditor, IDisposable
{
    private readonly ParameterEditExternalEventHandler _handler;
    private readonly ExternalEvent _externalEvent;

    public RevitModelEditor(IPluginLogger logger)
    {
        _handler = new ParameterEditExternalEventHandler(logger);
        _externalEvent = ExternalEvent.Create(_handler);
    }

    public Task<ParameterEditResult> SetParameterValueAsync(string uniqueId, string parameterName, string value)
    {
        var completion = new TaskCompletionSource<ParameterEditResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        _handler.Enqueue(uniqueId, parameterName, value, completion);
        _externalEvent.Raise();
        return completion.Task;
    }

    public void Dispose() => _externalEvent.Dispose();
}
