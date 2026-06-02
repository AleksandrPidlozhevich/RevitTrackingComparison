namespace RevitTrackingComparison.Core.Abstractions;

/// <summary>
/// Reads metadata from the active model for the capture-settings UI: the categories present and
/// the parameter names available for each. The Revit implementation runs on a valid API context
/// (ExternalEvent), so callers await the result.
/// </summary>
public interface IModelMetadataProvider
{
    /// <summary>Maps each category in the active document to its available parameter names.</summary>
    Task<IReadOnlyDictionary<string, IReadOnlyList<string>>> GetCategoryParametersAsync();
}