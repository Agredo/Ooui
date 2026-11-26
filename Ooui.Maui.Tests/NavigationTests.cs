using System.Diagnostics;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ooui.Maui.Tests;

using MauiLabel = Microsoft.Maui.Controls.Label;

[TestClass]
public sealed class NavigationTests
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

    static ContentPage CreatePage(string text, string? title = null) =>
        new() { Title = title, Content = new MauiLabel { Text = text } };

    static Div GetBar(Div root) => (Div)root.Children[0];

    static Div GetHost(Div root) => (Div)root.Children[1];

    static Ooui.Button GetBackButton(Div root) =>
        GetBar(root).Children.OfType<Ooui.Button>().Single();

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
    public void RootPageIsRenderedWithoutChrome()
    {
        var (context, scope) = CreateContext();
        using (scope)
        {
            var nav = new NavigationPage(CreatePage("Root page"));

            var root = (Div)nav.ToOouiElement(context);

            Assert.AreEqual("Root page", GetHost(root).Text);
            Assert.AreEqual("none", GetBar(root).Style.Display);
        }
    }

    [TestMethod]
    public void BarShowsTitleAndBackButtonAfterPush()
    {
        var (context, scope) = CreateContext();
        using (scope)
        {
            var rootPage = CreatePage("Root page", "Home");
            var nav = new NavigationPage(rootPage);
            var root = (Div)nav.ToOouiElement(context);
            var details = CreatePage("Details page", "Details");

            nav.PushAsync(details).GetAwaiter().GetResult();
            WaitUntil(() => GetHost(root).Text == "Details page", "The pushed page was not rendered.");

            Assert.AreEqual("flex", GetBar(root).Style.Display);
            Assert.AreEqual("Details", GetBar(root).Children.OfType<Ooui.Span>().Single().Text);
            Assert.AreNotEqual("none", GetBackButton(root).Style["display"]);
        }
    }

    [TestMethod]
    public void BackButtonPopsToThePreviousPage()
    {
        var (context, scope) = CreateContext();
        using (scope)
        {
            var nav = new NavigationPage(CreatePage("Root page"));
            var root = (Div)nav.ToOouiElement(context);
            nav.PushAsync(CreatePage("Details page", "Details")).GetAwaiter().GetResult();
            WaitUntil(() => GetHost(root).Text == "Details page", "The pushed page was not rendered.");

            var backButton = GetBackButton(root);
            backButton.Receive(Message.Event(backButton.Id, "click", ""));
            WaitUntil(() => GetHost(root).Text == "Root page", "The back button did not pop the page.");

            Assert.AreEqual("none", GetBar(root).Style.Display);
        }
    }

    [TestMethod]
    public void OnlyTheTopPageOfADeepStackIsRendered()
    {
        var (context, scope) = CreateContext();
        using (scope)
        {
            var nav = new NavigationPage(CreatePage("Root page"));
            var root = (Div)nav.ToOouiElement(context);

            nav.PushAsync(CreatePage("Second page", "Second")).GetAwaiter().GetResult();
            nav.PushAsync(CreatePage("Third page", "Third")).GetAwaiter().GetResult();
            WaitUntil(() => GetHost(root).Text == "Third page", "The third page was not rendered.");

            Assert.AreEqual(1, GetHost(root).Children.Count);
            Assert.AreEqual("flex", GetBar(root).Style.Display);
            Assert.AreEqual("Third", GetBar(root).Children.OfType<Ooui.Span>().Single().Text);

            var backButton = GetBackButton(root);
            backButton.Receive(Message.Event(backButton.Id, "click", ""));
            WaitUntil(() => GetHost(root).Text == "Second page", "Popping did not reveal the second page.");
            Assert.AreEqual("Second", GetBar(root).Children.OfType<Ooui.Span>().Single().Text);
        }
    }

    [TestMethod]
    public void HasNavigationBarFalseHidesTheBar()
    {
        var (context, scope) = CreateContext();
        using (scope)
        {
            var nav = new NavigationPage(CreatePage("Root page"));
            var root = (Div)nav.ToOouiElement(context);
            var details = CreatePage("Details page", "Details");
            NavigationPage.SetHasNavigationBar(details, false);

            nav.PushAsync(details).GetAwaiter().GetResult();
            WaitUntil(() => GetHost(root).Text == "Details page", "The pushed page was not rendered.");

            Assert.AreEqual("none", GetBar(root).Style.Display);
        }
    }

    static void ReportInitialHash(Div root, string hash = "") =>
        root.Receive(Message.Event("window", "hashchange", HashPayload(hash)));

    static Newtonsoft.Json.Linq.JObject HashPayload(string hash) =>
        new(new Newtonsoft.Json.Linq.JProperty("hash", hash));

    [TestMethod]
    public void PushSetsTheBrowserHash()
    {
        var (context, scope) = CreateContext();
        using (scope)
        {
            var rootPage = CreatePage("Root page");
            rootPage.StyleId = "root";
            var nav = new NavigationPage(rootPage);
            var root = (Div)nav.ToOouiElement(context);
            ReportInitialHash(root);

            var messages = new List<Message>();
            root.MessageSent += message =>
            {
                lock (messages)
                    messages.Add(message);
            };

            var details = CreatePage("Details page");
            details.StyleId = "details";
            nav.PushAsync(details).GetAwaiter().GetResult();
            WaitUntil(() => GetHost(root).Text == "Details page", "The pushed page was not rendered.");

            Message[] sent;
            lock (messages)
                sent = messages.ToArray();
            var hash = sent.Where(m => m.TargetId == "window" && m.Key == "location.hash").Select(m => m.Value).ToArray();
            Assert.AreEqual("#/root/details", hash.LastOrDefault());
        }
    }

    [TestMethod]
    public void HashChangePopsToThePreviousPage()
    {
        var (context, scope) = CreateContext();
        using (scope)
        {
            var rootPage = CreatePage("Root page");
            rootPage.StyleId = "root";
            var nav = new NavigationPage(rootPage);
            var root = (Div)nav.ToOouiElement(context);
            ReportInitialHash(root);
            var details = CreatePage("Details page");
            details.StyleId = "details";
            nav.PushAsync(details).GetAwaiter().GetResult();
            WaitUntil(() => GetHost(root).Text == "Details page", "The pushed page was not rendered.");

            root.Receive(Message.Event("window", "hashchange", HashPayload("#/root")));
            WaitUntil(() => GetHost(root).Text == "Root page", "The browser hash did not pop the page.");
        }
    }

    [TestMethod]
    public void UnknownHashKeepsTheCurrentPage()
    {
        var (context, scope) = CreateContext();
        using (scope)
        {
            var rootPage = CreatePage("Root page");
            rootPage.StyleId = "root";
            var nav = new NavigationPage(rootPage);
            var root = (Div)nav.ToOouiElement(context);
            ReportInitialHash(root);
            var details = CreatePage("Details page");
            details.StyleId = "details";
            nav.PushAsync(details).GetAwaiter().GetResult();
            WaitUntil(() => GetHost(root).Text == "Details page", "The pushed page was not rendered.");

            root.Receive(Message.Event("window", "hashchange", HashPayload("#/somewhere")));
            Thread.Sleep(50);
            root.Receive(Message.Event("window", "hashchange", HashPayload("")));
            Thread.Sleep(50);

            Assert.AreEqual("Details page", GetHost(root).Text);
        }
    }
}
