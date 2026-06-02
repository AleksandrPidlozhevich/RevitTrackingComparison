using RevitTrackingComparison.Core.Domain;

namespace RevitTrackingComparison.Core.Abstractions;

public interface ISnapshotStore
{
    void SaveSnapshot(string documentKey, DocumentSnapshot snapshot);

    DocumentSnapshot? GetLatestSnapshot(string documentKey);

    IReadOnlyList<DocumentSnapshot> GetSnapshots(string documentKey);

    void SaveWarnings(string documentKey, IEnumerable<WarningRecord> warnings);

    IReadOnlyList<WarningRecord> GetWarnings(string documentKey);

    void SaveSession(string documentKey, WorkSession session);

    IReadOnlyList<WorkSession> GetSessions(string documentKey);

    void Clear(string documentKey);
}