using RevitTrackingComparison.Core.Domain;

namespace RevitTrackingComparison.Core.Abstractions;

/// <summary>
/// Stores snapshots grouped by project (a "project" is the document file name, see RevitDocumentKey).
/// </summary>
/// <remarks>
/// Error policy: an absent result is returned, never thrown — <see cref="LoadAsync"/> yields null for a
/// missing snapshot and <see cref="ListAsync"/> yields an empty list for a project with none.
/// Unexpected failures (I/O errors, corruption) propagate as exceptions; the store does not log them —
/// the caller, which has the operation and user context, logs and surfaces them.
/// </remarks>
public interface ISnapshotStore
{
    bool HasSnapshots(string project);

    /// <summary>Writes <paramref name="snapshot"/> to a new timestamped file and returns its handle.</summary>
    Task<SnapshotInfo> SaveAsync(string project, DocumentSnapshot snapshot,
        CancellationToken cancellationToken = default);

    /// <summary>Lists the project's snapshots (handles only, no element data), newest first.</summary>
    Task<IReadOnlyList<SnapshotInfo>> ListAsync(string project, CancellationToken cancellationToken = default);

    /// <summary>Loads the full snapshot referenced by <paramref name="info"/>.</summary>
    Task<DocumentSnapshot?> LoadAsync(SnapshotInfo info, CancellationToken cancellationToken = default);
}