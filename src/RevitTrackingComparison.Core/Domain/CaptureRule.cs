namespace RevitTrackingComparison.Core.Domain;

/// <summary>
/// One entry of the capture configuration: which parameters to record for a given category.
/// Different categories have different parameter sets.
/// </summary>
public sealed class CaptureRule
{
    public string Category { get; init; } = string.Empty;

    public IReadOnlyList<string> Parameters { get; init; } = Array.Empty<string>();
}