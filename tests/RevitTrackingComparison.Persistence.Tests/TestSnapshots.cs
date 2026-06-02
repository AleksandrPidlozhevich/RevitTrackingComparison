using RevitTrackingComparison.Core.Domain;

namespace RevitTrackingComparison.Persistence.Tests;

internal static class TestSnapshots
{
    // No sub-second component: the store derives the file name (and parsed list timestamp) to second
    // precision, so tests stay deterministic.
    public static readonly DateTime DefaultCapturedAt = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    public static DocumentSnapshot Create(params ElementSnapshot[] elements)
        => Create(DefaultCapturedAt, "test.rvt", elements);

    public static DocumentSnapshot Create(DateTime capturedAt, params ElementSnapshot[] elements)
        => Create(capturedAt, "test.rvt", elements);

    public static DocumentSnapshot Create(DateTime capturedAt, string documentKey, params ElementSnapshot[] elements)
        => new()
        {
            Id = Guid.NewGuid(),
            DocumentKey = documentKey,
            Title = "Test",
            CapturedAt = capturedAt,
            Elements = elements
        };

    public static ElementSnapshot Element(
        string uniqueId = "uid-1",
        long elementId = 1001,
        string category = "Walls",
        string name = "Basic Wall",
        IReadOnlyDictionary<string, string>? parameters = null)
        => new()
        {
            UniqueId = uniqueId,
            ElementId = elementId,
            Category = category,
            Name = name,
            Parameters = parameters ?? new Dictionary<string, string>()
        };
}
