namespace RevitTrackingComparison.Core.Domain;

public sealed class ElementSnapshot
{
    public string UniqueId { get; init; } = string.Empty;

    public long ElementId { get; init; }

    public string Category { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public IReadOnlyDictionary<string, string> Parameters { get; init; }
        = new Dictionary<string, string>();
}