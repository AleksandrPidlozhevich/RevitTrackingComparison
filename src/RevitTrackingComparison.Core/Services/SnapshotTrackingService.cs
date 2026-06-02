using RevitTrackingComparison.Core.Abstractions;
using RevitTrackingComparison.Core.Domain;
using RevitTrackingComparison.Core.Domain.Diff;

namespace RevitTrackingComparison.Core.Services;

/// <inheritdoc cref="ISnapshotTrackingService"/>
public sealed class SnapshotTrackingService : ISnapshotTrackingService
{
    private readonly IRevitSnapshotProvider _snapshotProvider;
    private readonly ISnapshotStore _store;
    private readonly ISnapshotComparer _comparer;

    public SnapshotTrackingService(
        IRevitSnapshotProvider snapshotProvider,
        ISnapshotStore store,
        ISnapshotComparer comparer)
    {
        _snapshotProvider = snapshotProvider;
        _store = store;
        _comparer = comparer;
    }

    public SnapshotDiff? CaptureAndCompare()
    {
        var current = _snapshotProvider.CaptureActiveDocument();
        if (current is null)
            return null;

        var previous = _store.GetLatestSnapshot(current.DocumentKey);
        _store.SaveSnapshot(current.DocumentKey, current);

        return previous is null ? null : _comparer.Compare(previous, current);
    }

    public SnapshotDiff Compare(DocumentSnapshot from, DocumentSnapshot to)
    {
        return _comparer.Compare(from, to);
    }

    public IReadOnlyList<DocumentSnapshot> GetHistory(string documentKey)
    {
        return _store.GetSnapshots(documentKey);
    }
}