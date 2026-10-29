using System;
using System.Globalization;
using System.Windows.Data;

namespace OoBDev.UiDefinitions.Wpf;

/// <summary>Moves a marker's centre coordinate to the top-left offset a canvas wants (half the 5 unit marker).</summary>
public sealed class CentreConverter : IValueConverter
{
    private const double HalfMarker = 2.5;

    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is double centre ? centre - HalfMarker : 0d;

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
