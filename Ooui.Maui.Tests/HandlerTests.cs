using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ooui.Maui.Tests;

using Button = Microsoft.Maui.Controls.Button;
using Label = Microsoft.Maui.Controls.Label;

[TestClass]
public sealed class HandlerTests
{
    static MauiApp app = null!;

    [ClassInitialize]
    public static void Initialize(TestContext context)
    {
        app = MauiApp.CreateBuilder()
            .UseMauiApp<TestApplication>()
            .UseOoui()
            .Build();
    }

    static (IMauiContext Context, IServiceScope Scope) CreateContext()
    {
        var scope = app.Services.CreateScope();
        return (new MauiContext(scope.ServiceProvider), scope);
    }

    [TestMethod]
    public void LabelMapsPropertiesAndUpdates()
    {
        var (context, scope) = CreateContext();
        using (scope)
        {
            var label = new Label
            {
                Text = "Erster Text",
                TextColor = Microsoft.Maui.Graphics.Colors.Red,
                FontSize = 24,
                CharacterSpacing = 1.5,
                WidthRequest = 180,
                Padding = new Thickness(3, 4, 5, 6),
            };
            var element = (Span)label.ToOouiElement(context);

            Assert.AreEqual("Erster Text", element.Text);
            Assert.AreEqual("#FF0000FF", element.Style.Color);
            Assert.AreEqual("24px", element.Style.FontSize);
            Assert.AreEqual("1.5px", element.Style["letter-spacing"]);
            Assert.AreEqual("180px", element.Style.Width);
            Assert.AreEqual("4px", element.Style.PaddingTop);
            Assert.AreEqual("3px", element.Style.PaddingLeft);

            label.Text = "Aktualisiert";
            Assert.AreEqual("Aktualisiert", element.Text);
        }
    }

    [TestMethod]
    public void VerticalStackLayoutSupportsChildOperations()
    {
        var (context, scope) = CreateContext();
        using (scope)
        {
            var first = new Label { Text = "Erstes" };
            var third = new Label { Text = "Drittes" };
            var layout = new VerticalStackLayout { Spacing = 12, Children = { first, third } };
            var element = (Div)layout.ToOouiElement(context);

            Assert.AreEqual("column", element.Style["flex-direction"]);
            Assert.AreEqual("12px", element.Style["row-gap"]);
            CollectionAssert.AreEqual(new[] { "Erstes", "Drittes" }, element.Children.Select(child => child.Text).ToArray());

            var second = new Label { Text = "Zweites" };
            layout.Insert(1, second);
            CollectionAssert.AreEqual(new[] { "Erstes", "Zweites", "Drittes" }, element.Children.Select(child => child.Text).ToArray());

            layout.Remove(third);
            CollectionAssert.AreEqual(new[] { "Erstes", "Zweites" }, element.Children.Select(child => child.Text).ToArray());
        }
    }

    [TestMethod]
    public void ContentViewReplacesContent()
    {
        var (context, scope) = CreateContext();
        using (scope)
        {
            var contentView = new ContentView { Content = new Label { Text = "Alt" } };
            var element = (Div)contentView.ToOouiElement(context);
            Assert.AreEqual("Alt", element.Text);

            contentView.Content = new Label { Text = "Neu" };
            Assert.AreEqual(1, element.Children.Count);
            Assert.AreEqual("Neu", element.Text);
        }
    }

    [TestMethod]
    public void ButtonForwardsClickAndRaisesLifecycleEvents()
    {
        var (context, scope) = CreateContext();
        using (scope)
        {
            var button = new Button { Text = "Klick" };
            var pressed = 0;
            var released = 0;
            var clicked = 0;
            button.Pressed += (_, _) => pressed++;
            button.Released += (_, _) => released++;
            button.Clicked += (_, _) => clicked++;
            var element = (Ooui.Button)button.ToOouiElement(context);

            element.Receive(Message.Event(element.Id, "click"));

            Assert.AreEqual(1, pressed);
            Assert.AreEqual(1, released);
            Assert.AreEqual(1, clicked);
        }
    }

    [TestMethod]
    public void ApplicationSessionsOwnIndependentContentAndCleanUpWindows()
    {
        var application = (IApplication)app.Services.GetService(typeof(IApplication))!;
        var (firstContext, firstScope) = CreateContext();
        var (secondContext, secondScope) = CreateContext();
        using (firstScope)
        using (secondScope)
        {
            var first = application.ToOouiElement(firstContext);
            var second = application.ToOouiElement(secondContext);
            Assert.AreNotSame(first, second);
            Assert.IsInstanceOfType<IDisposable>(first);
            Assert.IsInstanceOfType<IDisposable>(second);
            Assert.AreEqual(2, application.Windows.Count);
            var firstWindow = application.Windows[0];
            var secondWindow = application.Windows[1];

            ((IDisposable)first).Dispose();
            Assert.AreEqual(1, application.Windows.Count);
            Assert.IsNull(firstWindow.Content?.Handler);
            Assert.IsNotNull(secondWindow.Content?.Handler);
            ((IDisposable)second).Dispose();
            Assert.AreEqual(0, application.Windows.Count);
            Assert.IsNull(secondWindow.Content?.Handler);
        }
    }

    sealed class TestApplication : Application
    {
        protected override Microsoft.Maui.Controls.Window CreateWindow(IActivationState? activationState) =>
            new(new ContentPage { Content = new Label { Text = "Sitzung" } });
    }
}
