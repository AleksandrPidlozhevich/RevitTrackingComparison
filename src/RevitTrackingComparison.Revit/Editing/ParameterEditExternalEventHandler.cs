using System.Collections.Concurrent;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitTrackingComparison.Core.Abstractions;
using RevitTrackingComparison.Revit.Infrastructure;

namespace RevitTrackingComparison.Revit.Editing;

// Applies queued parameter edits on the API thread inside RevitTransaction.Run (which suppresses our
// own warnings and rolls back on error). Result is returned via the request's TaskCompletionSource.
internal sealed class ParameterEditExternalEventHandler : IExternalEventHandler
{
    private readonly IPluginLogger<ParameterEditExternalEventHandler> _logger;
    private readonly ConcurrentQueue<EditRequest> _queue = new();

    public ParameterEditExternalEventHandler(IPluginLogger<ParameterEditExternalEventHandler> logger)
    {
        _logger = logger;
    }

    private readonly record struct EditRequest(
        string UniqueId,
        string ParameterName,
        string Value,
        TaskCompletionSource<ParameterEditResult> Completion);

    public void Enqueue(
        string uniqueId, string parameterName, string value,
        TaskCompletionSource<ParameterEditResult> completion)
    {
        _queue.Enqueue(new EditRequest(uniqueId, parameterName, value, completion));
    }

    public void Execute(UIApplication app)
    {
        while (_queue.TryDequeue(out var request))
            try
            {
                request.Completion.TrySetResult(Apply(app, request));
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    $"Failed to edit parameter '{request.ParameterName}' on element '{request.UniqueId}'.");
                request.Completion.TrySetResult(ParameterEditResult.Fail("Parameter edit failed."));
            }
    }

    public string GetName()
    {
        return "RevitTrackingComparison.EditParameter";
    }

    private ParameterEditResult Apply(UIApplication app, EditRequest request)
    {
        var doc = app.ActiveUIDocument?.Document;
        if (doc is null)
            return ParameterEditResult.Fail("No active document.");

        var element = doc.GetElement(request.UniqueId);
        if (element is null)
            return ParameterEditResult.Fail("Element not found in the active document.");

        var parameter = element.LookupParameter(request.ParameterName);
        if (parameter is null)
            return ParameterEditResult.Fail($"Parameter '{request.ParameterName}' not found.");

        if (parameter.IsReadOnly)
            return ParameterEditResult.Fail("Parameter is read-only.");

        var applied = false;
        var committed = RevitTransaction.Run(
            doc,
            $"Edit '{request.ParameterName}'",
            _ => applied = TrySet(parameter, request.Value));

        if (committed && applied)
            return ParameterEditResult.Ok();

        _logger.Warn(
            $"Revit rejected parameter '{request.ParameterName}' on element '{request.UniqueId}' (value '{request.Value}').");
        return ParameterEditResult.Fail("Revit rejected the value.");
    }

    // Symmetric with the snapshot read: doubles go through SetValueString (display units), others typed.
    private static bool TrySet(Parameter parameter, string value)
    {
        return parameter.StorageType switch
        {
            StorageType.Double => parameter.SetValueString(value),
            StorageType.Integer => int.TryParse(value, out var i) && parameter.Set(i),
            StorageType.String => parameter.Set(value),
            StorageType.ElementId => long.TryParse(value, out var id) && parameter.Set(new ElementId(id)),
            _ => false
        };
    }
}