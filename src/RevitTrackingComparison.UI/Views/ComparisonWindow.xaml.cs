using System.Windows;
using RevitTrackingComparison.UI.ViewModels;

namespace RevitTrackingComparison.UI.Views;

public partial class ComparisonWindow : Window
{
    public ComparisonWindow(ComparisonViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}