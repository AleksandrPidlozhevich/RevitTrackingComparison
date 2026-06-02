using System.Windows;

namespace RevitTrackingComparison.UI.Themes;

public static class WindowTheme
{
    private static readonly Uri ThemeUri = new(
        "pack://application:,,,/RevitTrackingComparison.UI;component/Themes/UiResources.xaml",
        UriKind.Absolute);

    public static void Apply(Window window)
    {
        var theme = new ResourceDictionary { Source = ThemeUri };
        window.Resources.MergedDictionaries.Insert(0, theme);
        window.SetResourceReference(Window.StyleProperty, "AppWindow");
    }
}
