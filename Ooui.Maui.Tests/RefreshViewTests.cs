using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ooui.Maui.Handlers;

namespace Ooui.Maui.Tests;

using RefreshView = Microsoft.Maui.Controls.RefreshView;
using MauiLabel = Microsoft.Maui.Controls.Label;

[TestClass]
public sealed class RefreshViewTests
{
    static MauiApp app = null!;

    [ClassInitialize]
    public static void Initialize(TestContext context)
    {
        app = MauiApp.CreateBuilder().UseMauiApp<Application>().UseOoui().Build();
    }

    static (Div Root, Div Content, Button Button, Span Status) Render(RefreshView view)
    {
        var context = new MauiContext(app.Services);
        var root = (Div)view.ToOouiElement(context);
        var bar = root.Children.OfType<Div>().Single(c => c.ClassName == RefreshViewHandler.BarClassName);
        var content = root.Children.OfType<Div>().Single(c => c.ClassName == RefreshViewHandler.ContentClassName);
        var button = bar.Children.OfType<Button>().Single();
        var status = bar.Children.OfType<Span>().Single();
        return (root, content, button, status);
    }

    [TestMethod]
    public void ContentIsRenderedBelowTheRefreshBar()
    {
        var view = new RefreshView { Content = new MauiLabel { Text = "Inhalt" } };

        var (root, content, _, _) = Render(view);

        Assert.AreEqual(2, root.Children.Count);
        Assert.AreEqual("Inhalt", ((Span)content.Children.Single()).Text);
    }

    [TestMethod]
    public void ClickingRefreshRunsTheCommandOnce()
    {
        var count = 0;
        var view = new RefreshView { Content = new MauiLabel { Text = "Inhalt" } };
        view.Command = new Command(() =>
        {
            count++;
            view.IsRefreshing = false;
        });

        var (_, _, button, status) = Render(view);
        button.Receive(Message.Event(button.Id, "click"));

        Assert.AreEqual(1, count);
        Assert.IsFalse(view.IsRefreshing);
        Assert.IsFalse(button.IsDisabled);
        Assert.AreEqual("", status.Text);
    }

    [TestMethod]
    public void RefreshingDisablesTheButtonAndShowsTheStatus()
    {
        var view = new RefreshView { Content = new MauiLabel { Text = "Inhalt" } };

        var (root, _, button, status) = Render(view);
        Assert.IsFalse(button.IsDisabled);
        Assert.AreEqual("", status.Text);

        view.IsRefreshing = true;

        Assert.IsTrue(button.IsDisabled);
        Assert.AreEqual("Refreshing…", status.Text);
        Assert.AreEqual("true", root.GetStringAttribute("aria-busy", ""));

        view.IsRefreshing = false;

        Assert.IsFalse(button.IsDisabled);
        Assert.AreEqual("", status.Text);
        Assert.AreEqual("", root.GetStringAttribute("aria-busy", ""));
    }

    [TestMethod]
    public void DisabledRefreshViewCannotStartRefreshing()
    {
        var view = new RefreshView { Content = new MauiLabel { Text = "Inhalt" }, IsRefreshEnabled = false };

        var (_, _, button, _) = Render(view);
        Assert.IsTrue(button.IsDisabled);

        button.Receive(Message.Event(button.Id, "click"));

        Assert.IsFalse(view.IsRefreshing);
    }

    [TestMethod]
    public void RefreshColorColorsTheStatusText()
    {
        var view = new RefreshView
        {
            Content = new MauiLabel { Text = "Inhalt" },
            RefreshColor = Microsoft.Maui.Graphics.Colors.Red,
        };

        var (_, _, _, status) = Render(view);

        Assert.AreEqual("#FF0000FF", status.Style.Color);
    }
}
