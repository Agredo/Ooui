using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;

namespace Ooui.Maui.Handlers;

public class ActivityIndicatorHandler : OouiViewHandler<IActivityIndicator, Span>, IActivityIndicatorHandler
{
    public static readonly IPropertyMapper<IActivityIndicator, ActivityIndicatorHandler> Mapper =
        new PropertyMapper<IActivityIndicator, ActivityIndicatorHandler>(OouiViewMapper.Mapper)
        {
            [nameof(IActivityIndicator.IsRunning)] = MapRunning,
            [nameof(IActivityIndicator.Color)] = MapColor,
        };

    public ActivityIndicatorHandler() : base(Mapper) { }

    IActivityIndicator IActivityIndicatorHandler.VirtualView => VirtualView;
    object IActivityIndicatorHandler.PlatformView => PlatformView;

    protected override Span CreatePlatformView() => new() { Text = "◌", ClassName = "ooui-activity-indicator" };

    public static void MapRunning(ActivityIndicatorHandler handler, IActivityIndicator indicator)
    {
        handler.PlatformView.Style.Display = indicator.IsRunning ? "inline-block" : "none";
        handler.PlatformView.Style["animation"] = indicator.IsRunning ? "ooui-spin 1s linear infinite" : null;
        if (indicator.IsRunning)
        {
            UI.Styles["@keyframes ooui-spin"]["from"] = "transform: rotate(0deg)";
            UI.Styles["@keyframes ooui-spin"]["to"] = "transform: rotate(360deg)";
        }
    }

    public static void MapColor(ActivityIndicatorHandler handler, IActivityIndicator indicator) =>
        handler.PlatformView.Style.Color = OouiStyle.ToCss(indicator.Color);
}
