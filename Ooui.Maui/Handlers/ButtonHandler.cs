using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace Ooui.Maui.Handlers;

public class ButtonHandler : OouiViewHandler<IButton, Button>, IButtonHandler
{
    public static readonly IPropertyMapper<IButton, ButtonHandler> Mapper =
        new PropertyMapper<IButton, ButtonHandler>(OouiViewMapper.Mapper)
        {
            [nameof(IText.Text)] = MapText,
            [nameof(ITextStyle.TextColor)] = (handler, button) => OouiStyle.SetTextColor((Element)handler.PlatformView!, button as ITextStyle),
            [nameof(ITextStyle.Font)] = (handler, button) => OouiStyle.SetFont((Element)handler.PlatformView!, button as ITextStyle),
            [nameof(IButton.Padding)] = MapPadding,
            [nameof(IButtonStroke.StrokeThickness)] = MapStroke,
            [nameof(IButtonStroke.StrokeColor)] = MapStroke,
            [nameof(IButtonStroke.CornerRadius)] = MapStroke,
        };

    public ImageSourcePartLoader ImageSourceLoader { get; }

    IButton IButtonHandler.VirtualView => VirtualView;
    object IButtonHandler.PlatformView => PlatformView;
    ImageSourcePartLoader IButtonHandler.ImageSourceLoader => ImageSourceLoader;

    public ButtonHandler() : base(Mapper)
    {
        ImageSourceLoader = new ImageSourcePartLoader(new ImageSourceSetter(this));
    }

    sealed class ImageSourceSetter(ButtonHandler handler) : IImageSourcePartSetter
    {
        public IElementHandler Handler => handler;
        public IImageSourcePart? ImageSourcePart => handler.VirtualView as IImageSourcePart;
        public void SetImageSource(object? platformImage) { }
    }

    protected override Button CreatePlatformView() => new();

    protected override void ConnectHandler(Button platformView)
    {
        base.ConnectHandler(platformView);
        platformView.Click += OnClick;
    }

    protected override void DisconnectHandler(Button platformView)
    {
        platformView.Click -= OnClick;
        base.DisconnectHandler(platformView);
    }

    void OnClick(object? sender, TargetEventArgs args)
    {
        VirtualView.Pressed();
        VirtualView.Released();
        VirtualView.Clicked();
    }

    public static void MapText(ButtonHandler handler, IButton button) => handler.PlatformView.Text = (button as IText)?.Text;

    public static void MapPadding(ButtonHandler handler, IButton button) => OouiStyle.SetThickness(handler.PlatformView.Style, button.Padding);

    public static void MapStroke(ButtonHandler handler, IButton button)
    {
        handler.PlatformView.Style.BorderWidth = button.StrokeThickness;
        handler.PlatformView.Style.BorderStyle = button.StrokeThickness > 0 ? "solid" : null;
        handler.PlatformView.Style.BorderColor = OouiStyle.ToCss(button.StrokeColor as Microsoft.Maui.Graphics.Color);
        handler.PlatformView.Style.BorderRadius = button.CornerRadius;
    }
}
