namespace RevitTrackingComparison.Core.Domain;

/// <summary>
/// The capture configuration: the set of categories (and their parameters) recorded into snapshots.
/// Loaded from / saved to an external config file via <c>ICaptureSettingsStore</c>.
/// </summary>
public sealed class CaptureSettings
{
    public IReadOnlyList<CaptureRule> Rules { get; init; } = Array.Empty<CaptureRule>();

    public bool IncludesCategory(string category)
    {
        return Rules.Any(r => string.Equals(r.Category, category, StringComparison.OrdinalIgnoreCase));
    }

    public IReadOnlyList<string> ParametersFor(string category)
    {
        return Rules.FirstOrDefault(r => string.Equals(r.Category, category, StringComparison.OrdinalIgnoreCase))
            ?.Parameters ?? Array.Empty<string>();
    }
}