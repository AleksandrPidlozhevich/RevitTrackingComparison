namespace RevitTrackingComparison.Core.Domain.Diff;

public sealed class SnapshotDiff
{
    public required Guid FromSnapshotId { get; init; }
    public required Guid ToSnapshotId { get; init; }

    public IReadOnlyList<ElementChange> Added { get; init; } = Array.Empty<ElementChange>();
    public IReadOnlyList<ElementChange> Removed { get; init; } = Array.Empty<ElementChange>();
    public IReadOnlyList<ElementChange> Modified { get; init; } = Array.Empty<ElementChange>();

    public bool HasChanges => Added.Count > 0 || Removed.Count > 0 || Modified.Count > 0;

    public IEnumerable<ElementChange> AllChanges => Added.Concat(Removed).Concat(Modified);
}