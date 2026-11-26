using System.Diagnostics;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ooui.Maui.Tests;

using MauiLabel = Microsoft.Maui.Controls.Label;

[TestClass]
public sealed class TabbedPageTests
{
    static MauiApp app = null!;

    [ClassInitialize]
    public static void Initialize(TestContext context)
    {
        app = MauiApp.CreateBuilder().UseMauiApp<Application>().UseOoui().Build();
    }

    static (IMauiContext Context, IServiceScope Scope) CreateContext()
    {
        var scope = app.Services.CreateScope();
        return (new MauiContext(scope.ServiceProvider), scope);
    }

    static ContentPage CreatePage(string text, string title) =>
        new() { Title = title, Content = new MauiLabel { Text = text } };

    static Div GetTabBar(Div root) => (Div)root.Children[0];

    static Div GetContent(Div root) => (Div)root.Children[1];

    static Ooui.Button[] GetTabs(Div root) =>
        GetTabBar(root).Children.OfType<Ooui.Button>().ToArray();

    static void WaitUntil(Func<bool> condition, string message)
    {
        var watch = Stopwatch.StartNew();
        while (watch.Elapsed < TimeSpan.FromSeconds(5))
        {
            if (condition())
                return;
            Thread.Sleep(10);
        }
        Assert.Fail(message);
    }

    [TestMethod]
    public void TabsRenderWithTitlesAndTheCurrentPage()
    {
        var (context, scope) = CreateContext();
        using (scope)
        {
            var tabbed = new TabbedPage();
            tabbed.Children.Add(CreatePage("Home content", "Home"));
            tabbed.Children.Add(CreatePage("Settings content", "Settings"));

            var root = (Div)tabbed.ToOouiElement(context);

            var tabs = GetTabs(root);
            CollectionAssert.AreEqual(new[] { "Home", "Settings" }, tabs.Select(t => t.Text).ToArray());
            Assert.AreEqual("true", GetTabs(root)[0].GetAttribute("aria-selected"));
            Assert.AreEqual("false", GetTabs(root)[1].GetAttribute("aria-selected"));
            Assert.AreEqual("Home content", GetContent(root).Text);
        }
    }

    [TestMethod]
    public void ClickingATabShowsItsPage()
    {
        var (context, scope) = CreateContext();
        using (scope)
        {
            var tabbed = new TabbedPage();
            var home = CreatePage("Home content", "Home");
            var settings = CreatePage("Settings content", "Settings");
            tabbed.Children.Add(home);
            tabbed.Children.Add(settings);

            var root = (Div)tabbed.ToOouiElement(context);
            var settingsTab = GetTabs(root)[1];

            settingsTab.Receive(Message.Event(settingsTab.Id, "click", ""));

            WaitUntil(() => ReferenceEquals(tabbed.CurrentPage, settings), "The click did not select the second tab.");
            WaitUntil(() => GetContent(root).Text == "Settings content", "The second page was not rendered.");
            Assert.AreEqual("true", GetTabs(root)[1].GetAttribute("aria-selected"));
            Assert.AreEqual("false", GetTabs(root)[0].GetAttribute("aria-selected"));
        }
    }

    [TestMethod]
    public void AddingAPageRebuildsTheTabBar()
    {
        var (context, scope) = CreateContext();
        using (scope)
        {
            var tabbed = new TabbedPage();
            tabbed.Children.Add(CreatePage("Home content", "Home"));

            var root = (Div)tabbed.ToOouiElement(context);
            Assert.AreEqual(1, GetTabs(root).Length);

            tabbed.Children.Add(CreatePage("Settings content", "Settings"));

            WaitUntil(() => GetTabs(root).Length == 2, "The tab bar was not rebuilt.");
        }
    }

    [TestMethod]
    public void ChangingTheCurrentPageSwitchesTheContent()
    {
        var (context, scope) = CreateContext();
        using (scope)
        {
            var tabbed = new TabbedPage();
            var home = CreatePage("Home content", "Home");
            var settings = CreatePage("Settings content", "Settings");
            tabbed.Children.Add(home);
            tabbed.Children.Add(settings);

            var root = (Div)tabbed.ToOouiElement(context);
            tabbed.CurrentPage = settings;

            WaitUntil(() => GetContent(root).Text == "Settings content", "The current page was not rendered.");
        }
    }
}
