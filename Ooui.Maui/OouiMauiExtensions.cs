using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Hosting;
using Ooui.Maui.Handlers;
using MauiButton = Microsoft.Maui.Controls.Button;
using MauiContentView = Microsoft.Maui.Controls.ContentView;
using MauiLabel = Microsoft.Maui.Controls.Label;
using MauiLayout = Microsoft.Maui.Controls.Layout;

namespace Ooui.Maui;

public static class OouiMauiExtensions
{
    /// <summary>Registers the experimental Ooui handlers for a server-hosted MAUI application.</summary>
    public static MauiAppBuilder UseOoui(this MauiAppBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        var dispatcher = new OouiDispatcher();
        DispatcherProvider.SetCurrent(dispatcher);
        builder.Services.TryAddSingleton<IDispatcher>(dispatcher);
        builder.Services.ConfigureMauiHandlers(handlers =>
        {
            handlers.AddHandler<MauiLabel, LabelHandler>();
            handlers.AddHandler<MauiButton, ButtonHandler>();
            handlers.AddHandler<MauiLayout, LayoutHandler>();
            handlers.AddHandler<VerticalStackLayout, LayoutHandler>();
            handlers.AddHandler<HorizontalStackLayout, LayoutHandler>();
            handlers.AddHandler<Entry, EntryHandler>();
            handlers.AddHandler<Editor, EditorHandler>();
            handlers.AddHandler<Microsoft.Maui.Controls.CheckBox, CheckBoxHandler>();
            handlers.AddHandler<Microsoft.Maui.Controls.Switch, SwitchHandler>();
            handlers.AddHandler<Slider, SliderHandler>();
            handlers.AddHandler<Stepper, StepperHandler>();
            handlers.AddHandler<Picker, PickerHandler>();
            handlers.AddHandler<DatePicker, DatePickerHandler>();
            handlers.AddHandler<TimePicker, TimePickerHandler>();
            handlers.AddHandler<SearchBar, SearchBarHandler>();
            handlers.AddHandler<Grid, GridHandler>();
            handlers.AddHandler<FlexLayout, FlexLayoutHandler>();
            handlers.AddHandler<ScrollView, ScrollViewHandler>();
            handlers.AddHandler<CollectionView, CollectionViewHandler>();
            handlers.AddHandler<Border, BorderHandler>();
            handlers.AddHandler<Microsoft.Maui.Controls.Image, ImageHandler>();
            handlers.AddHandler<Microsoft.Maui.Controls.RadioButton, RadioButtonHandler>();
            handlers.AddHandler<ImageButton, ImageButtonHandler>();
            handlers.AddHandler<ProgressBar, ProgressBarHandler>();
            handlers.AddHandler<ActivityIndicator, ActivityIndicatorHandler>();
            handlers.AddHandler<BoxView, BoxViewHandler>();
            handlers.AddHandler<ContentPage, ContentViewHandler>();
            handlers.AddHandler<MauiContentView, ContentViewHandler>();
            handlers.AddHandler<IContentView, ContentViewHandler>();
        });
        return builder;
    }
}
