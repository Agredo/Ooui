using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using MauiImageButton = Microsoft.Maui.Controls.ImageButton;

namespace Ooui.Maui.Handlers;

/// <summary>Renders an <see cref="MauiImageButton"/> as a button that contains an image.</summary>
public class ImageButtonHandler : OouiViewHandler<MauiImageButton, Button>
{
    public static readonly IPropertyMapper<MauiImageButton, ImageButtonHandler> Mapper =
        new PropertyMapper<MauiImageButton, ImageButtonHandler>(OouiViewMapper.Mapper)
        {
            [nameof(MauiImageButton.Source)] = MapSource,
            [nameof(MauiImageButton.Aspect)] = MapAspect,
            [nameof(MauiImageButton.Padding)] = MapPadding,
            [nameof(MauiImageButton.CornerRadius)] = MapStroke,
            [nameof(MauiImageButton.BorderWidth)] = MapStroke,
            [nameof(MauiImageButton.BorderColor)] = MapStroke,
        };

    readonly Image image = new();

    public ImageButtonHandler() : base(Mapper) { }

    protected override Button CreatePlatformView()
    {
        var button = new Button();
        button.Style["background"] = "transparent";
        button.Style["cursor"] = "pointer";
        image.Style["display"] = "block";
        image.Style["width"] = "100%";
        image.Style["height"] = "100%";
        button.AppendChild(image);
        return button;
    }

    protected override void ConnectHandler(Button platformView)
    {
        base.ConnectHandler(platformView);
        platformView.Click += OnClick;
        MapSource(this, VirtualView);
        MapAspect(this, VirtualView);
        MapPadding(this, VirtualView);
        MapStroke(this, VirtualView);
    }

    protected override void DisconnectHandler(Button platformView)
    {
        platformView.Click -= OnClick;
        base.DisconnectHandler(platformView);
    }

    void OnClick(object? sender, TargetEventArgs args)
    {
        ((IButton)VirtualView).Pressed();
        ((IButton)VirtualView).Released();
        ((IButton)VirtualView).Clicked();
    }

    public static void MapSource(ImageButtonHandler handler, MauiImageButton button) =>
        handler.image.Source = ImageSources.ToUrl(button.Source);

    public static void MapAspect(ImageButtonHandler handler, MauiImageButton button) =>
        handler.image.Style["object-fit"] = button.Aspect switch
        {
            Aspect.AspectFit => "contain",
            Aspect.AspectFill => "cover",
            _ => "fill",
        };

    public static void MapPadding(ImageButtonHandler handler, MauiImageButton button) =>
        OouiStyle.SetThickness(handler.PlatformView.Style, button.Padding);

    public static void MapStroke(ImageButtonHandler handler, MauiImageButton button)
    {
        handler.PlatformView.Style.BorderWidth = button.BorderWidth;
        handler.PlatformView.Style.BorderStyle = button.BorderWidth > 0 ? "solid" : null;
        handler.PlatformView.Style.BorderColor = OouiStyle.ToCss(button.BorderColor);
        handler.PlatformView.Style.BorderRadius = button.CornerRadius;
    }
}
