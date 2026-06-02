using System.Windows;
using RevitTrackingComparison.UI.Themes;
using RevitTrackingComparison.UI.ViewModels;

namespace RevitTrackingComparison.UI.Views;

public partial class SnapshotCompareWindow : Window
{
    public SnapshotCompareWindow(SnapshotCompareViewModel viewModel)
    {
        WindowTheme.Apply(this);
        InitializeComponent();
        DataContext = viewModel;
        Loaded += async (_, _) => await viewModel.InitializeAsync();
    }
}