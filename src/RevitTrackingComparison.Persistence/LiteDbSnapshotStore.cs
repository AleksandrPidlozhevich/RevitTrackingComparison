using RevitTrackingComparison.Core.Abstractions;
using RevitTrackingComparison.Core.Domain;
using RevitTrackingComparison.Persistence.Entities;

namespace RevitTrackingComparison.Persistence;

public sealed class LiteDbSnapshotStore : ISnapshotStore
{
    private const string Snapshots = "snapshots";
    private const string Warnings = "warnings";
    private const string Sessions = "sessions";

    private readonly ILiteDbConnectionFactory _connectionFactory;

    public LiteDbSnapshotStore(ILiteDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public void SaveSnapshot(string documentKey, DocumentSnapshot snapshot)
    {
        using var db = _connectionFactory.Open(documentKey);
        db.GetCollection<SnapshotEntity>(Snapshots).Insert(snapshot.ToEntity());
    }

    public DocumentSnapshot? GetLatestSnapshot(string documentKey)
    {
        using var db = _connectionFactory.Open(documentKey);
        var entity = db.GetCollection<SnapshotEntity>(Snapshots)
            .Query()
            .OrderByDescending(x => x.CapturedAt)
            .FirstOrDefault();
        return entity?.ToDomain();
    }

    public IReadOnlyList<DocumentSnapshot> GetSnapshots(string documentKey)
    {
        using var db = _connectionFactory.Open(documentKey);
        return db.GetCollection<SnapshotEntity>(Snapshots)
            .Query()
            .OrderBy(x => x.CapturedAt)
            .ToList()
            .Select(e => e.ToDomain())
            .ToList();
    }

    public void SaveWarnings(string documentKey, IEnumerable<WarningRecord> warnings)
    {
        using var db = _connectionFactory.Open(documentKey);
        var entities = warnings.Select(w => w.ToEntity(documentKey)).ToList();
        if (entities.Count > 0)
            db.GetCollection<WarningEntity>(Warnings).InsertBulk(entities);
    }

    public IReadOnlyList<WarningRecord> GetWarnings(string documentKey)
    {
        using var db = _connectionFactory.Open(documentKey);
        return db.GetCollection<WarningEntity>(Warnings)
            .FindAll()
            .Select(e => e.ToDomain())
            .ToList();
    }

    public void SaveSession(string documentKey, WorkSession session)
    {
        using var db = _connectionFactory.Open(documentKey);
        db.GetCollection<SessionEntity>(Sessions).Insert(session.ToEntity(documentKey));
    }

    public IReadOnlyList<WorkSession> GetSessions(string documentKey)
    {
        using var db = _connectionFactory.Open(documentKey);
        return db.GetCollection<SessionEntity>(Sessions)
            .FindAll()
            .Select(e => e.ToDomain())
            .ToList();
    }

    public void Clear(string documentKey)
    {
        using var db = _connectionFactory.Open(documentKey);
        db.GetCollection<SnapshotEntity>(Snapshots).DeleteAll();
        db.GetCollection<WarningEntity>(Warnings).DeleteAll();
        db.GetCollection<SessionEntity>(Sessions).DeleteAll();
    }
}