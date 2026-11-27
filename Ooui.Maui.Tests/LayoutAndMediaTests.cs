using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Layouts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ooui.Maui.Tests;

using Label = Microsoft.Maui.Controls.Label;
using Colors = Microsoft.Maui.Graphics.Colors;

[TestClass]
public sealed class LayoutAndMediaTests
{
    static MauiApp app = null!;

    [ClassInitialize]
    public static void Initialize(TestContext context)
    {
        CultureInfo.CurrentCulture = new CultureInfo("de-DE");
        app = MauiApp.CreateBuilder().UseMauiApp<Application>().UseOoui().Build();
    }

    static IMauiContext CreateContext(out IServiceScope scope)
    {
        scope = app.Services.CreateScope();
        return new MauiContext(scope.ServiceProvider);
    }

    [TestMethod]
    public void GridMapsTracksAndAttachedChildPlacement()
    {
        using var scope = app.Services.CreateScope();
        var context = new MauiContext(scope.ServiceProvider);
        var label = new Label { Text = "Kind" };
        var grid = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition { Width = GridLength.Auto }, new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) } },
            RowDefinitions = { new RowDefinition { Height = new GridLength(40) } },
            ColumnSpacing = 8.5,
            RowSpacing = 4,
            Children = { label },
        };
        Grid.SetColumn(label, 1);
        Grid.SetRowSpan(label, 2);
        var element = (Div)grid.ToOouiElement(context);

        Assert.AreEqual("grid", element.Style.Display);
        Assert.AreEqual("auto 2fr", element.Style["grid-template-columns"]);
        Assert.AreEqual("40px", element.Style["grid-template-rows"]);
        Assert.AreEqual("8.5px", element.Style["column-gap"]);
        var child = (Span)element.Children.Single();
        Assert.AreEqual(2, child.Style["grid-column-start"]);
        Assert.AreEqual("span 2", child.Style["grid-row-end"]);

        var second = new Label { Text = "Zweite Karte" };
        grid.Children.Add(second);
        Assert.AreEqual(1, ((Span)element.Children[1]).Style["grid-column-start"]);
        Assert.AreEqual(2, element.Children.Count);

        Grid.SetColumn(label, 0);
        Assert.AreEqual(1, child.Style["grid-column-start"]);
        grid.ColumnDefinitions[0].Width = new GridLength(100);
        Assert.AreEqual("100px 2fr", element.Style["grid-template-columns"]);
    }

    [TestMethod]
    public void GridAutomaticallyPlacesChildrenWithoutOverridingExplicitOrigin()
    {
        using var scope = app.Services.CreateScope();
        var context = new MauiContext(scope.ServiceProvider);
        var first = new Label();
        var second = new Label();
        var grid = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(), new ColumnDefinition() },
            Children = { first, second },
        };
        var element = (Div)grid.ToOouiElement(context);
        Assert.AreEqual(2, ((Span)element.Children[1]).Style["grid-column-start"]);
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        Assert.AreEqual("1fr 1fr 1fr", element.Style["grid-template-columns"]);
        Assert.AreEqual(2, ((Span)element.Children[1]).Style["grid-column-start"]);

        var third = new Label();
        grid.Children.Add(third);
        Assert.AreEqual(3, ((Span)element.Children[2]).Style["grid-column-start"]);

        grid.Children.Remove(first);
        Assert.AreEqual(1, ((Span)element.Children[0]).Style["grid-column-start"]);
        Assert.AreEqual(2, ((Span)element.Children[1]).Style["grid-column-start"]);

        Grid.SetRow(second, 0);
        Grid.SetColumn(second, 0);
        Assert.AreEqual(1, ((Span)element.Children[0]).Style["grid-column-start"]);
        Grid.SetColumn(second, 1);
        Assert.AreEqual(2, ((Span)element.Children[0]).Style["grid-column-start"]);
        Assert.AreEqual(1, ((Span)element.Children[1]).Style["grid-column-start"]);
    }

    [TestMethod]
    public void GridMapsChildAlignmentAndMargin()
    {
        using var scope = app.Services.CreateScope();
        var context = new MauiContext(scope.ServiceProvider);
        var badge = new Label
        {
            Text = "Badge",
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.End,
            Margin = new Thickness(16, 8, 0, 24),
        };
        var grid = new Grid { Children = { badge } };
        var element = (Div)grid.ToOouiElement(context);
        var child = (Span)element.Children.Single();

        Assert.AreEqual("start", child.Style["justify-self"]);
        Assert.AreEqual("end", child.Style["align-self"]);
        Assert.AreEqual("16px", child.Style["margin-left"]);
        Assert.AreEqual("8px", child.Style["margin-top"]);
        Assert.AreEqual("0px", child.Style["margin-right"]);
        Assert.AreEqual("24px", child.Style["margin-bottom"]);

        badge.HorizontalOptions = LayoutOptions.Fill;
        Assert.AreEqual("stretch", child.Style["justify-self"]);
    }

    [TestMethod]
    public void StackMapsCrossAxisAlignment()
    {
        using var scope = app.Services.CreateScope();
        var context = new MauiContext(scope.ServiceProvider);
        var row = new HorizontalStackLayout { Children = { new Label { Text = "A", VerticalOptions = LayoutOptions.Center } } };
        var rowElement = (Div)row.ToOouiElement(context);
        Assert.AreEqual("center", ((Span)rowElement.Children.Single()).Style["align-self"]);

        var column = new VerticalStackLayout { Children = { new Label { Text = "B", HorizontalOptions = LayoutOptions.End } } };
        var columnElement = (Div)column.ToOouiElement(context);
        Assert.AreEqual("end", ((Span)columnElement.Children.Single()).Style["align-self"]);
    }

    [TestMethod]
    public void FlexLayoutMapsDirectionAndAttachedChildProperties()
    {
        using var scope = app.Services.CreateScope();
        var context = new MauiContext(scope.ServiceProvider);
        var child = new Label();
        var flex = new FlexLayout
        {
            Direction = FlexDirection.RowReverse,
            Wrap = FlexWrap.Wrap,
            JustifyContent = FlexJustify.SpaceBetween,
            Children = { child },
        };
        FlexLayout.SetGrow(child, 2);
        FlexLayout.SetBasis(child, new FlexBasis(120));
        var element = (Div)flex.ToOouiElement(context);
        var childElement = (Span)element.Children.Single();

        Assert.AreEqual("row-reverse", element.Style["flex-direction"]);
        Assert.AreEqual("wrap", element.Style["flex-wrap"]);
        Assert.AreEqual("space-between", element.Style["justify-content"]);
        Assert.AreEqual(2f, childElement.Style.FlexGrow);
        Assert.AreEqual("120px", childElement.Style["flex-basis"]);

        FlexLayout.SetGrow(child, 3);
        Assert.AreEqual(3f, childElement.Style.FlexGrow);
    }

    [TestMethod]
    public void ScrollViewReplacesContentAndForwardsOffsetsAndRequests()
    {
        using var scope = app.Services.CreateScope();
        var context = new MauiContext(scope.ServiceProvider);
        var scroll = new ScrollView { Orientation = ScrollOrientation.Vertical, HeightRequest = 265, Content = new Label { Text = "Alt" } };
        var element = (Div)scroll.ToOouiElement(context);
        Assert.AreEqual("hidden", element.Style["overflow-x"]);
        Assert.AreEqual("auto", element.Style["overflow-y"]);
        Assert.AreEqual("265px", element.Style.Height);

        Message? request = null;
        element.MessageSent += message => { if (message.MessageType == MessageType.Call) request = message; };
        _ = scroll.ScrollToAsync(0, 120, false);
        Assert.IsNotNull(request);
        Assert.AreEqual("scrollTo", request.Key);
        var options = ((object[])request.Value)[0];
        var top = options.GetType().GetProperty("top")?.GetValue(options);
        Assert.AreEqual(120d, top);

        var scrollJson = Newtonsoft.Json.JsonConvert.SerializeObject(Message.Event(element.Id, "scroll", new { scrollLeft = 0, scrollTop = 80, scrollWidth = 300, scrollHeight = 900, clientWidth = 300, clientHeight = 400 }));
        element.Receive(Message.FromJson(scrollJson));
        Assert.AreEqual(80, scroll.ScrollY);
        scroll.Content = new Label { Text = "Neu" };
        Assert.AreEqual("Neu", element.Text);
        Assert.AreEqual(1, element.Children.Count);
    }

    [TestMethod]
    public void BorderAndBoxViewMapSolidColorsAndCorners()
    {
        using var scope = app.Services.CreateScope();
        var context = new MauiContext(scope.ServiceProvider);
        var border = new Border
        {
            Stroke = Colors.Red,
            StrokeThickness = 2,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(4, 6, 8, 10) },
            Content = new Label { Text = "Inhalt" },
        };
        var borderElement = (Div)border.ToOouiElement(context);
        Assert.AreEqual("#FF0000FF", borderElement.Style.BorderColor);
        Assert.AreEqual("2px", borderElement.Style.BorderWidth);
        Assert.AreEqual("4px 6px 10px 8px", borderElement.Style.BorderRadius);
        Assert.AreEqual("Inhalt", borderElement.Text);
        Assert.AreEqual("hidden", borderElement.Style.Overflow);
        border.StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(4, 6, 8, 10) };
        Assert.AreEqual("Inhalt", borderElement.Text);

        var box = new BoxView { Color = Colors.Blue, CornerRadius = new CornerRadius(3) };
        var boxElement = (Div)box.ToOouiElement(context);
        Assert.AreEqual("#0000FFFF", boxElement.Style.BackgroundColor);
        Assert.AreEqual("3px 3px 3px 3px", boxElement.Style.BorderRadius);
    }

    [TestMethod]
    public void LinearGradientBecomesACssGradientAndClearsThePreviousPaint()
    {
        using var scope = app.Services.CreateScope();
        var context = new MauiContext(scope.ServiceProvider);
        var border = new Border
        {
            Background = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0),
                GradientStops =
                {
                    new GradientStop(Colors.Blue, 1),
                    new GradientStop(Colors.Red, 0),
                    new GradientStop(Colors.Lime.WithAlpha(0.5f), 0.5f),
                },
            },
        };
        var element = (Div)border.ToOouiElement(context);
        Assert.AreEqual(
            "linear-gradient(90deg, #FF0000FF 0%, #00FF007F 50%, #0000FFFF 100%)",
            element.Style["background-image"]);
        Assert.AreEqual("inherit", (string)element.Style.BackgroundColor);

        // A horizontal gradient pointing to the left must not become a top-to-bottom one.
        border.Background = new LinearGradientBrush(new GradientStopCollection
        {
            new GradientStop(Colors.Red, 0),
            new GradientStop(Colors.Blue, 1),
        }, new Point(1, 0), new Point(0, 0));
        StringAssert.StartsWith((string)element.Style["background-image"], "linear-gradient(270deg,");

        border.Background = new SolidColorBrush(Colors.Blue);
        Assert.AreEqual("#0000FFFF", element.Style.BackgroundColor);
        Assert.AreEqual("inherit", (string)element.Style["background-image"]);

        border.Background = null;
        Assert.AreEqual("inherit", (string)element.Style.BackgroundColor);

        var box = new BoxView
        {
            Fill = new RadialGradientBrush(new GradientStopCollection
            {
                new GradientStop(Colors.White, 0),
                new GradientStop(Colors.Black, 1),
            }),
        };
        var boxElement = (Div)box.ToOouiElement(context);
        Assert.AreEqual(
            "radial-gradient(circle at 50% 50%, #FFFFFFFF 0%, #000000FF 100%)",
            boxElement.Style["background-image"]);
    }

    [TestMethod]
    public void ShadowBecomesBoxShadowAndFollowsTheControl()
    {
        using var scope = app.Services.CreateScope();
        var context = new MauiContext(scope.ServiceProvider);
        var border = new Border
        {
            Shadow = new Shadow
            {
                Brush = Brush.Black,
                Opacity = 0.5f,
                Radius = 12,
                Offset = new Point(0, 6),
            },
        };
        var element = (Div)border.ToOouiElement(context);
        Assert.AreEqual("0px 6px 12px rgba(0, 0, 0, 0.5)", element.Style["box-shadow"]);

        border.Shadow = new Shadow { Brush = Brush.Red, Radius = 4, Opacity = 1 };
        Assert.AreEqual("0px 0px 4px rgba(255, 0, 0, 1)", element.Style["box-shadow"]);

        border.Shadow = null;
        Assert.AreEqual("inherit", (string)element.Style["box-shadow"]);
    }

    [TestMethod]
    public void ImageAndFeedbackControlsMapBrowserProperties()
    {
        using var scope = app.Services.CreateScope();
        var context = new MauiContext(scope.ServiceProvider);
        var image = new Microsoft.Maui.Controls.Image { Source = "https://example.com/logo.png", Aspect = Aspect.AspectFit };
        var imageElement = (Ooui.Image)image.ToOouiElement(context);
        Assert.AreEqual("https://example.com/logo.png", imageElement.Source);
        Assert.AreEqual("contain", imageElement.Style["object-fit"]);

        var progress = new ProgressBar { Progress = 0.25, ProgressColor = Colors.Green };
        var progressElement = (Progress)progress.ToOouiElement(context);
        Assert.AreEqual("0.25", progressElement.Value);
        Assert.AreEqual(1d, progressElement.Maximum);

        var activity = new ActivityIndicator { IsRunning = true, Color = Colors.Purple };
        var activityElement = (Span)activity.ToOouiElement(context);
        Assert.AreEqual("inline-block", activityElement.Style.Display);
        StringAssert.Contains((string)activityElement.Style["animation"], "ooui-spin");
        activity.IsRunning = false;
        Assert.AreEqual("none", activityElement.Style.Display);
    }

    [TestMethod]
    public void BrowserScrollJsonUpdatesMauiScrollPosition()
    {
        using var scope = app.Services.CreateScope();
        var context = new MauiContext(scope.ServiceProvider);
        var scroll = new ScrollView { Content = new Label() };
        var element = (Div)scroll.ToOouiElement(context);
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(Message.Event(element.Id, "scroll", new { scrollLeft = 2.5, scrollTop = 7.5, scrollWidth = 100, scrollHeight = 200 }));
        element.Receive(Message.FromJson(json));
        Assert.AreEqual(2.5, scroll.ScrollX);
        Assert.AreEqual(7.5, scroll.ScrollY);
    }
}
