using RevitTrackingComparison.Core.Domain;

namespace RevitTrackingComparison.Core.Abstractions;

/// <summary>
/// Stores snapshots as one file per snapshot, grouped by project. A "project" is the document
/// file name (see RevitDocumentKey).
/// </summary>
public interface ISnapshotStore
{
    /// <summary>True if the project already has at least one stored snapshot.</summary>
    bool HasSnapshots(string project);

    /// <summary>Writes <paramref name="snapshot"/> to a new timestamped file and returns its handle.</summary>
    SnapshotInfo Save(string project, DocumentSnapshot snapshot);

    /// <summary>Lists the project's snapshots (handles only, no element data), newest first.</summary>
    IReadOnlyList<SnapshotInfo> List(string project);

    /// <summary>Loads the full snapshot referenced by <paramref name="info"/>.</summary>
    DocumentSnapshot? Load(SnapshotInfo info);
}
