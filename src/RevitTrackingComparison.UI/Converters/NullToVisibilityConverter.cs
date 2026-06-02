using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RevitTrackingComparison.UI.Converters;

public sealed class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var visibleWhenNull = string.Equals(parameter as string, "invert", StringComparison.OrdinalIgnoreCase);
        var isNull = value is null;
        var visible = visibleWhenNull ? isNull : !isNull;
        return visible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}