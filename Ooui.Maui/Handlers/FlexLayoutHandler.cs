using Microsoft.Maui.Handlers;
using Microsoft.Maui.Layouts;
using ILayout = Microsoft.Maui.ILayout;

namespace Ooui.Maui.Handlers;

public class FlexLayoutHandler : LayoutHandler
{
    public new static readonly IPropertyMapper<ILayout, FlexLayoutHandler> Mapper =
        new PropertyMapper<ILayout, FlexLayoutHandler>(LayoutHandler.Mapper)
        {
            [nameof(IFlexLayout.Direction)] = MapFlex,
            [nameof(IFlexLayout.Wrap)] = MapFlex,
            [nameof(IFlexLayout.JustifyContent)] = MapFlex,
            [nameof(IFlexLayout.AlignItems)] = MapFlex,
            [nameof(IFlexLayout.AlignContent)] = MapFlex,
        };

    public FlexLayoutHandler() : base(Mapper) { }

    protected override void UpdateLayout()
    {
        base.UpdateLayout();
        if (VirtualView is not IFlexLayout flex) return;
        PlatformView.Style["flex-direction"] = Css(flex.Direction);
        PlatformView.Style["flex-wrap"] = Css(flex.Wrap);
        PlatformView.Style["justify-content"] = Css(flex.JustifyContent);
        PlatformView.Style["align-items"] = Css(flex.AlignItems);
        PlatformView.Style["align-content"] = Css(flex.AlignContent);
        foreach (var child in flex) UpdateChild(child);
    }

    protected override void UpdateChild(IView view)
    {
        if (VirtualView is not IFlexLayout flex || view.Handler is not IElementHandler { PlatformView: Element element }) return;
        element.Style.Order = flex.GetOrder(view);
        element.Style.FlexGrow = flex.GetGrow(view);
        element.Style.FlexShrink = flex.GetShrink(view);
        element.Style.AlignSelf = Css(flex.GetAlignSelf(view));
        var basis = flex.GetBasis(view);
        element.Style["flex-basis"] = basis == FlexBasis.Auto ? "auto" : FormattableString.Invariant($"{basis.Length}px");
    }

    protected override void OnChildPropertyChanged(IView view, string propertyName)
    {
        if (propertyName is "Order" or "Grow" or "Shrink" or "AlignSelf" or "Basis" or
            nameof(Microsoft.Maui.Controls.FlexLayout.OrderProperty) or
            nameof(Microsoft.Maui.Controls.FlexLayout.GrowProperty) or
            nameof(Microsoft.Maui.Controls.FlexLayout.ShrinkProperty) or
            nameof(Microsoft.Maui.Controls.FlexLayout.AlignSelfProperty) or
            nameof(Microsoft.Maui.Controls.FlexLayout.BasisProperty))
            UpdateChild(view);
    }

    public static void MapFlex(FlexLayoutHandler handler, ILayout layout) => handler.UpdateLayout();

    static string Css<T>(T value) where T : struct =>
        System.Text.RegularExpressions.Regex.Replace(value.ToString() ?? "", "([a-z])([A-Z])", "$1-$2").ToLowerInvariant();
}
