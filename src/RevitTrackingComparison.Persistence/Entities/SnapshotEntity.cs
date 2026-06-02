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

internal sealed class WarningEntity
{
    [BsonId] public Guid Id { get; set; } = Guid.NewGuid();
    public string DocumentKey { get; set; } = string.Empty;
    public string DefinitionGuid { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> FailingElements { get; set; } = new();
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

internal sealed class SessionEntity
{
    [BsonId] public Guid Id { get; set; } = Guid.NewGuid();
    public string DocumentKey { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
}