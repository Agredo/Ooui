using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;

namespace Ooui.Maui.Handlers;

public abstract class OouiViewHandler<TVirtualView, TPlatformView> : ViewHandler<TVirtualView, TPlatformView>
    where TVirtualView : class, IView
    where TPlatformView : Element
{
    protected OouiViewHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null)
        : base(mapper, commandMapper ?? OouiViewMapper.CommandMapper)
    {
    }

    public override bool NeedsContainer => false;

    public new TPlatformView PlatformView => base.PlatformView;

    protected override void SetupContainer() => SetContainerView(null);

    protected override void RemoveContainer() => SetContainerView(null);

    public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
    {
        var width = VirtualView.Width;
        var height = VirtualView.Height;
        return new Size(
            double.IsNaN(width) ? Math.Min(widthConstraint, 100) : width,
            double.IsNaN(height) ? Math.Min(heightConstraint, 44) : height);
    }

    public override void PlatformArrange(Rect frame)
    {
        OouiStyle.SetDimension(PlatformView.Style, "width", frame.Width);
        OouiStyle.SetDimension(PlatformView.Style, "height", frame.Height);
    }

    protected override void ConnectHandler(TPlatformView platformView)
    {
        base.ConnectHandler(platformView);
        platformView.Style["box-sizing"] = "border-box";
    }

}
