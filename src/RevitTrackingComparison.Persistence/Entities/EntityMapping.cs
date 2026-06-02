using RevitTrackingComparison.Core.Domain;

namespace RevitTrackingComparison.Persistence.Entities;

internal static class EntityMapping
{
    public static SnapshotEntity ToEntity(this DocumentSnapshot snapshot)
    {
        return new SnapshotEntity
        {
            Id = snapshot.Id,
            DocumentKey = snapshot.DocumentKey,
            Title = snapshot.Title,
            CapturedAt = snapshot.CapturedAt,
            Elements = snapshot.Elements.Select(e => new ElementEntity
            {
                UniqueId = e.UniqueId,
                ElementId = e.ElementId,
                Category = e.Category,
                Name = e.Name,
                Parameters = new Dictionary<string, string>(e.Parameters)
            }).ToList()
        };
    }

    public static DocumentSnapshot ToDomain(this SnapshotEntity entity)
    {
        return new DocumentSnapshot
        {
            Id = entity.Id,
            DocumentKey = entity.DocumentKey,
            Title = entity.Title,
            CapturedAt = entity.CapturedAt,
            Elements = entity.Elements.Select(e => new ElementSnapshot
            {
                UniqueId = e.UniqueId,
                ElementId = e.ElementId,
                Category = e.Category,
                Name = e.Name,
                Parameters = new Dictionary<string, string>(e.Parameters)
            }).ToList()
        };
    }
}
