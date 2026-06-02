namespace RevitTrackingComparison.Core.Domain;

public sealed class DocumentSnapshot
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string DocumentKey { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public DateTime CapturedAt { get; init; } = DateTime.Now;

    public IReadOnlyList<ElementSnapshot> Elements { get; init; } = Array.Empty<ElementSnapshot>();
}