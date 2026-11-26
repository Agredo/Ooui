using System.Globalization;
using Microsoft.Maui.Controls;

namespace ConsoleNet11;

/// <summary>Maps 0…100 to the 0…1 range of a <see cref="ProgressBar"/>.</summary>
public sealed class PercentageConverter : IValueConverter
{
    public object Convert (object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is double number ? number / 100 : 0;

    public object ConvertBack (object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException ();
}
