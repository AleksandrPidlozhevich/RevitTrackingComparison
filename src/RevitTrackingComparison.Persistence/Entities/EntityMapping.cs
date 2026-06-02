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

    public static WarningEntity ToEntity(this WarningRecord warning, string documentKey)
    {
        return new WarningEntity
        {
            Id = warning.Id,
            DocumentKey = documentKey,
            DefinitionGuid = warning.DefinitionGuid,
            Description = warning.Description,
            FailingElements = warning.FailingElements.ToList(),
            CreatedBy = warning.CreatedBy,
            CreatedAt = warning.CreatedAt
        };
    }

    public static WarningRecord ToDomain(this WarningEntity entity)
    {
        return new WarningRecord
        {
            Id = entity.Id,
            DefinitionGuid = entity.DefinitionGuid,
            Description = entity.Description,
            FailingElements = entity.FailingElements.ToList(),
            CreatedBy = entity.CreatedBy,
            CreatedAt = entity.CreatedAt
        };
    }

    public static SessionEntity ToEntity(this WorkSession session, string documentKey)
    {
        return new SessionEntity
        {
            Id = session.Id,
            DocumentKey = documentKey,
            StartTime = session.StartTime,
            EndTime = session.EndTime
        };
    }

    public static WorkSession ToDomain(this SessionEntity entity)
    {
        return new WorkSession
        {
            Id = entity.Id,
            StartTime = entity.StartTime,
            EndTime = entity.EndTime
        };
    }
}