using System;
using System.Globalization;
using System.Windows.Data;

namespace SmbExplorerCompanion.WPF.Converters;

public class InverseBooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        if (value is not bool boolean)
            throw new ArgumentException("Value must be a boolean", nameof(value));

        return !boolean ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
    }

    public object ConvertBack(object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        if (value is not System.Windows.Visibility visibility)
            throw new ArgumentException("Value must be a Visibility", nameof(value));

        return visibility != System.Windows.Visibility.Visible;
    }
}