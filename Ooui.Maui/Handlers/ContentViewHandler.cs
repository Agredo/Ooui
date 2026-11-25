using Microsoft.Maui.Handlers;

namespace Ooui.Maui.Handlers;

public class ContentViewHandler : OouiViewHandler<IContentView, Div>, IContentViewHandler
{
    public static readonly IPropertyMapper<IContentView, ContentViewHandler> Mapper =
        new PropertyMapper<IContentView, ContentViewHandler>(ViewMapper)
        {
            [nameof(IView.AutomationId)] = MapAutomationId,
            [nameof(IView.Background)] = MapBackground,
            [nameof(IView.Width)] = MapWidth,
            [nameof(IView.Height)] = MapHeight,
            [nameof(IView.IsEnabled)] = MapIsEnabled,
            [nameof(IView.Opacity)] = MapOpacity,
            [nameof(IView.Visibility)] = MapVisibility,
            [nameof(IView.InputTransparent)] = MapInputTransparent,
            [nameof(IContentView.Content)] = MapContent,
            [nameof(ITitledElement.Title)] = MapTitle,
        };

    Element? content;

    public ContentViewHandler() : base(Mapper) { }

    IContentView IContentViewHandler.VirtualView => VirtualView;
    object IContentViewHandler.PlatformView => PlatformView;

    protected override Div CreatePlatformView() => new();

    public static void MapContent(ContentViewHandler handler, IContentView view) => handler.UpdateContent();

    public static void MapTitle(ContentViewHandler handler, IContentView view) =>
        handler.PlatformView.SetAttribute("aria-label", (view as ITitledElement)?.Title);

    void UpdateContent()
    {
        if (content is not null && PlatformView.Children.Contains(content))
            PlatformView.RemoveChild(content);
        content = null;
        if (VirtualView.PresentedContent is IView view && MauiContext is not null)
        {
            content = view.ToOouiElement(MauiContext);
            PlatformView.AppendChild(content);
        }
    }

    protected override void DisconnectHandler(Div platformView)
    {
        content = null;
        base.DisconnectHandler(platformView);
    }
}
