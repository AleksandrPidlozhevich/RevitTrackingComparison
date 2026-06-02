using RevitTrackingComparison.Core.Domain.Diff;
using RevitTrackingComparison.UI.ViewModels;
using RevitTrackingComparison.UI.Views;

namespace RevitTrackingComparison.UI.Services;

public sealed class ComparisonView : IComparisonView
{
    public void Show(SnapshotDiff? diff)
    {
        var viewModel = new ComparisonViewModel();
        viewModel.Load(diff);
        var window = new ComparisonWindow(viewModel);
        window.ShowDialog();
    }
}