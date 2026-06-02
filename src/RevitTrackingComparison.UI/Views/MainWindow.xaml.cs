using System.Windows;
using RevitTrackingComparison.UI.Themes;
using RevitTrackingComparison.UI.ViewModels;

namespace RevitTrackingComparison.UI.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        WindowTheme.Apply(this);
        InitializeComponent();
        DataContext = viewModel;
    }
}