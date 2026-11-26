using Microsoft.Maui.Handlers;

namespace Ooui.Maui.Handlers;

public class ScrollViewHandler : OouiViewHandler<IScrollView, Div>, IScrollViewHandler
{
    public static readonly IPropertyMapper<IScrollView, ScrollViewHandler> Mapper =
        new PropertyMapper<IScrollView, ScrollViewHandler>(OouiViewMapper.Mapper)
        {
            [nameof(IScrollView.Content)] = MapContent,
            [nameof(IScrollView.Orientation)] = MapOrientation,
            [nameof(IScrollView.HorizontalScrollBarVisibility)] = MapOrientation,
            [nameof(IScrollView.VerticalScrollBarVisibility)] = MapOrientation,
        };

    Element? content;

    public ScrollViewHandler() : base(Mapper) { }

    IScrollView IScrollViewHandler.VirtualView => VirtualView;
    object IScrollViewHandler.PlatformView => PlatformView;

    protected override Div CreatePlatformView() => new();

    protected override void ConnectHandler(Div platformView)
    {
        base.ConnectHandler(platformView);
        if (VirtualView is Microsoft.Maui.Controls.ScrollView scrollView)
            scrollView.ScrollToRequested += OnScrollToRequested;
        platformView.AddEventListener("scroll", OnScrolled);
    }

    protected override void DisconnectHandler(Div platformView)
    {
        if (VirtualView is Microsoft.Maui.Controls.ScrollView scrollView)
            scrollView.ScrollToRequested -= OnScrollToRequested;
        platformView.RemoveEventListener("scroll", OnScrolled);
        content = null;
        base.DisconnectHandler(platformView);
    }

    void OnScrollToRequested(object? sender, Microsoft.Maui.Controls.ScrollToRequestedEventArgs args)
    {
        PlatformView.Call("scrollTo", new { left = args.ScrollX, top = args.ScrollY, behavior = args.ShouldAnimate ? "smooth" : "instant" });
        VirtualView.ScrollFinished();
    }

    void OnScrolled(object sender, TargetEventArgs args)
    {
        if (VirtualView is Microsoft.Maui.Controls.ScrollView scrollView)
        {
            if (args.ScrollX != scrollView.ScrollX || args.ScrollY != scrollView.ScrollY)
                scrollView.SetScrolledPosition(args.ScrollX, args.ScrollY);
        }
    }

    public static void MapContent(ScrollViewHandler handler, IScrollView scrollView)
    {
        if (handler.content is not null) handler.PlatformView.RemoveChild(handler.content);
        handler.content = scrollView.PresentedContent?.ToOouiElement(handler.MauiContext!);
        if (handler.content is not null) handler.PlatformView.AppendChild(handler.content);
    }

    public static void MapOrientation(ScrollViewHandler handler, IScrollView scrollView)
    {
        handler.PlatformView.Style.Overflow = "auto";
        handler.PlatformView.Style["overflow-x"] = scrollView.Orientation is ScrollOrientation.Vertical or ScrollOrientation.Neither ? "hidden" : "auto";
        handler.PlatformView.Style["overflow-y"] = scrollView.Orientation is ScrollOrientation.Horizontal or ScrollOrientation.Neither ? "hidden" : "auto";
    }
}
