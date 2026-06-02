using LiteDB;

namespace RevitTrackingComparison.Persistence.Entities;

internal sealed class SnapshotEntity
{
    [BsonId] public Guid Id { get; set; } = Guid.NewGuid();
    public string DocumentKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime CapturedAt { get; set; }
    public List<ElementEntity> Elements { get; set; } = new();
}

internal sealed class ElementEntity
{
    public string UniqueId { get; set; } = string.Empty;
    public long ElementId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, string> Parameters { get; set; } = new();
}