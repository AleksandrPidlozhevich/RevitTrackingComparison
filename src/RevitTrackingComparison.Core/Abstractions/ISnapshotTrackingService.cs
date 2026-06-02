using RevitTrackingComparison.Core.Domain;
using RevitTrackingComparison.Core.Domain.Diff;

namespace RevitTrackingComparison.Core.Abstractions;

public interface ISnapshotTrackingService
{
    SnapshotDiff? CaptureAndCompare();

    SnapshotDiff Compare(DocumentSnapshot from, DocumentSnapshot to);

    IReadOnlyList<DocumentSnapshot> GetHistory(string documentKey);
}