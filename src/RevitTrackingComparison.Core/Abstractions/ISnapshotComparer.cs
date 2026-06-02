using RevitTrackingComparison.Core.Domain;
using RevitTrackingComparison.Core.Domain.Diff;

namespace RevitTrackingComparison.Core.Abstractions;

public interface ISnapshotComparer
{
    SnapshotDiff Compare(DocumentSnapshot from, DocumentSnapshot to);
}