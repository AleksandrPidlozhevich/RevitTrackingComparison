using RevitTrackingComparison.Core.Domain.Diff;

namespace RevitTrackingComparison.UI.Services;

public interface IComparisonView
{
    void Show(SnapshotDiff? diff);
}