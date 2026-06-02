using System.Windows;
using RevitTrackingComparison.UI.Themes;
using RevitTrackingComparison.UI.ViewModels;

namespace RevitTrackingComparison.UI.Views;

public partial class SnapshotExportWindow : Window
{
    public SnapshotExportWindow(SnapshotExportViewModel viewModel)
    {
        WindowTheme.Apply(this);
        InitializeComponent();
        DataContext = viewModel;
    }
}
