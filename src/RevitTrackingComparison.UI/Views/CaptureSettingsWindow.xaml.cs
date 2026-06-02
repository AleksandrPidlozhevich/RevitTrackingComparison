using System.Windows;
using RevitTrackingComparison.UI.Themes;
using RevitTrackingComparison.UI.ViewModels;

namespace RevitTrackingComparison.UI.Views;

public partial class CaptureSettingsWindow : Window
{
    public CaptureSettingsWindow(CaptureSettingsViewModel viewModel)
    {
        WindowTheme.Apply(this);
        InitializeComponent();
        DataContext = viewModel;

        // Read the category/parameter catalog from the model once the window is shown.
        Loaded += async (_, _) => await viewModel.LoadFromModelAsync();
    }
}
