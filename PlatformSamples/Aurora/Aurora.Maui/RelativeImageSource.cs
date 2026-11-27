using System.Globalization;
using Microsoft.Maui.Controls;

namespace Aurora.Maui;

public sealed class RelativeImageSourceConverter : IValueConverter
{
    public static string? DocumentBaseUri { get; set; }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var path = value as string ?? string.Empty;
        var uri = Uri.TryCreate(RelativeImageSourceConverter.DocumentBaseUri, UriKind.Absolute, out var baseUri)
            ? new Uri(baseUri, path)
            : new Uri(path, UriKind.RelativeOrAbsolute);
        return ImageSource.FromUri(uri);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
