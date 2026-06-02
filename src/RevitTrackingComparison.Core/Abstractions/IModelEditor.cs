namespace RevitTrackingComparison.Core.Abstractions;

// Writes a parameter value back into the Revit model. The implementation dispatches to a valid API
// context (ExternalEvent); UI awaits the returned task and stays Revit-free.
public interface IModelEditor
{
    Task<ParameterEditResult> SetParameterValueAsync(string uniqueId, string parameterName, string value);
}