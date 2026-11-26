using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using ILayout = Microsoft.Maui.ILayout;
using IImage = Microsoft.Maui.IImage;
using Image = global::Ooui.Image;
using Microsoft.Maui.Platform;

namespace Ooui.Maui.Handlers;

public class ImageHandler : OouiViewHandler<IImage, Image>, IImageHandler
{
    public static readonly IPropertyMapper<IImage, ImageHandler> Mapper =
        new PropertyMapper<IImage, ImageHandler>(OouiViewMapper.Mapper)
        {
            [nameof(IImage.Aspect)] = MapAspect,
            [nameof(IImage.Source)] = MapSource,
        };

    public ImageSourcePartLoader SourceLoader { get; }
    ImageSourcePartLoader IImageHandler.SourceLoader => SourceLoader;
    IImage IImageHandler.VirtualView => VirtualView;
    object IImageHandler.PlatformView => PlatformView;

    public ImageHandler() : base(Mapper) => SourceLoader = new ImageSourcePartLoader(new SourceSetter(this));

    sealed class SourceSetter(ImageHandler handler) : IImageSourcePartSetter
    {
        public IElementHandler Handler => handler;
        public IImageSourcePart? ImageSourcePart => handler.VirtualView;
        public void SetImageSource(object? platformImage) { }
    }

    protected override Image CreatePlatformView() => new();

    public static void MapAspect(ImageHandler handler, IImage image) =>
        handler.PlatformView.Style["object-fit"] = image.Aspect switch { Aspect.AspectFit => "contain", Aspect.AspectFill => "cover", _ => "fill" };

    public static void MapSource(ImageHandler handler, IImage image) =>
        handler.PlatformView.Source = ImageSources.ToUrl(image.Source as ImageSource);
}
