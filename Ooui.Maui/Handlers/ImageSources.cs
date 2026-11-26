using Microsoft.Maui.Controls;

namespace Ooui.Maui.Handlers;

static class ImageSources
{
    public static string? ToUrl(ImageSource? source) => source switch
    {
        UriImageSource uri => uri.Uri.ToString(),
        FileImageSource file when Uri.TryCreate(file.File, UriKind.Absolute, out var uri) && uri.IsAbsoluteUri => uri.ToString(),
        FileImageSource file => "/" + file.File.TrimStart('/'),
        _ => null,
    };
}
