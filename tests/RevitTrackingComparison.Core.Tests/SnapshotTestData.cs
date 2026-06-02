using RevitTrackingComparison.Core.Domain;

namespace RevitTrackingComparison.Core.Tests;

internal static class SnapshotTestData
{
    public const long DefaultElementId = 665246;
    public const string DefaultUniqueId = "bdb1b609-ac6e-4ba6-b32d-4a65ac0ef4ab-000a269e";
    public const string DefaultName = "Foundation - 24\" Concrete";
    public const string AddedElementUniqueId = "bdb1b609-ac6e-4ba6-b32d-4a65ac0ef4ab-000a26a1";
    public const string AddedElementName = "Foundation - 30\" Concrete";

    public static DocumentSnapshot CreateSnapshot(params ElementSnapshot[] elements)
    {
        return CreateSnapshot(null, elements);
    }

    public static DocumentSnapshot CreateSnapshot(Guid? id, params ElementSnapshot[] elements)
    {
        return new DocumentSnapshot
        {
            Id = id ?? Guid.NewGuid(),
            DocumentKey = "test.rvt",
            Title = "Test",
            CapturedAt = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            Elements = elements
        };
    }

    public static ElementSnapshot Element(
        string? uniqueId = null,
        string category = "Structural Foundations",
        long elementId = DefaultElementId,
        string name = DefaultName,
        IReadOnlyDictionary<string, string>? parameters = null)
    {
        return new ElementSnapshot
        {
            UniqueId = uniqueId ?? DefaultUniqueId,
            ElementId = elementId,
            Category = category,
            Name = name,
            Parameters = parameters ?? new Dictionary<string, string>()
        };
    }
}