using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;

namespace Ooui.Maui.Handlers;

public class ProgressBarHandler : OouiViewHandler<IProgress, Progress>, IProgressBarHandler
{
    public static readonly IPropertyMapper<IProgress, ProgressBarHandler> Mapper =
        new PropertyMapper<IProgress, ProgressBarHandler>(OouiViewMapper.Mapper)
        {
            [nameof(IProgress.Progress)] = MapProgress,
            [nameof(IProgress.ProgressColor)] = MapProgressColor,
        };

    public ProgressBarHandler() : base(Mapper) { }

    IProgress IProgressBarHandler.VirtualView => VirtualView;
    object IProgressBarHandler.PlatformView => PlatformView;

    protected override Progress CreatePlatformView() => new() { Maximum = 1 };

    public static void MapProgress(ProgressBarHandler handler, IProgress progress) =>
        handler.PlatformView.Value = progress.Progress.ToString(CultureInfo.InvariantCulture);

    public static void MapProgressColor(ProgressBarHandler handler, IProgress progress) =>
        handler.PlatformView.Style["accent-color"] = OouiStyle.ToCss(progress.ProgressColor);
}
