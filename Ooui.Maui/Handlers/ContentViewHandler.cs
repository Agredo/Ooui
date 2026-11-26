using Microsoft.Maui.Handlers;

namespace Ooui.Maui.Handlers;

public class ContentViewHandler : OouiViewHandler<IContentView, Div>, IContentViewHandler
{
    public static readonly IPropertyMapper<IContentView, ContentViewHandler> Mapper =
        new PropertyMapper<IContentView, ContentViewHandler>(OouiViewMapper.Mapper)
        {
            [nameof(IContentView.Content)] = MapContent,
            [nameof(IPadding.Padding)] = MapPadding,
            [nameof(ITitledElement.Title)] = MapTitle,
        };

    Element? content;

    public ContentViewHandler() : base(Mapper) { }

    protected ContentViewHandler(IPropertyMapper<IContentView, ContentViewHandler> mapper) : base(mapper) { }

    IContentView IContentViewHandler.VirtualView => VirtualView;
    object IContentViewHandler.PlatformView => PlatformView;

    protected override Div CreatePlatformView() => new();

    public static void MapContent(ContentViewHandler handler, IContentView view) => handler.UpdateContent();

    public static void MapPadding(ContentViewHandler handler, IContentView view) => OouiStyle.SetThickness(handler.PlatformView.Style, view.Padding);

    public static void MapTitle(ContentViewHandler handler, IContentView view)
    {
        var title = (view as ITitledElement)?.Title;
        if (string.IsNullOrEmpty(title)) handler.PlatformView.RemoveAttribute("aria-label");
        else handler.PlatformView.SetAttribute("aria-label", title);
    }

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
