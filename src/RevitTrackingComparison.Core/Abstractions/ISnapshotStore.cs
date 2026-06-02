using RevitTrackingComparison.Core.Domain;

namespace RevitTrackingComparison.Core.Abstractions;

/// <summary>
/// Stores snapshots as one file per snapshot, grouped by project. A "project" is the document
/// file name (see RevitDocumentKey). Reads/writes are async because the backing store is file I/O;
/// callers awaiting them keep the UI thread free.
/// </summary>
public interface ISnapshotStore
{
    /// <summary>
    /// True if the project already has at least one stored snapshot. Synchronous on purpose: it is a
    /// cheap metadata check called from the synchronous <c>DocumentOpened</c> Revit event, which must
    /// stay on the API thread (an async continuation would resume off-thread).
    /// </summary>
    bool HasSnapshots(string project);

    /// <summary>Writes <paramref name="snapshot"/> to a new timestamped file and returns its handle.</summary>
    Task<SnapshotInfo> SaveAsync(string project, DocumentSnapshot snapshot, CancellationToken cancellationToken = default);

    /// <summary>Lists the project's snapshots (handles only, no element data), newest first.</summary>
    Task<IReadOnlyList<SnapshotInfo>> ListAsync(string project, CancellationToken cancellationToken = default);

    /// <summary>Loads the full snapshot referenced by <paramref name="info"/>.</summary>
    Task<DocumentSnapshot?> LoadAsync(SnapshotInfo info, CancellationToken cancellationToken = default);
}
