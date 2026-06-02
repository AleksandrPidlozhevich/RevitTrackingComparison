using System.Windows;
using RevitTrackingComparison.UI.ViewModels;

namespace RevitTrackingComparison.UI.Views;

public partial class ProgressBarWindow : Window
{
    public ProgressBarWindow(ProgressBarViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.CloseAction = Close;
    }
}