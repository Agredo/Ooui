using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ooui.Maui.Tests;

using MauiLabel = Microsoft.Maui.Controls.Label;

[TestClass]
public sealed class NavigationUrlTests
{
    sealed class SettingsPage : ContentPage { }

    sealed class MyAccountPage : ContentPage { }

    static ContentPage CreatePage(string text) => new() { Content = new MauiLabel { Text = text } };

    [TestMethod]
    public void SegmentUsesStyleIdWhenSet()
    {
        var page = CreatePage("Content");
        Assert.AreEqual("content", NavigationUrl.GetSegment(page));

        page.StyleId = "custom-page";
        Assert.AreEqual("custom-page", NavigationUrl.GetSegment(page));
    }

    [TestMethod]
    public void SegmentDropsThePageSuffixAndUsesKebabCase()
    {
        Assert.AreEqual("settings", NavigationUrl.GetSegment(new SettingsPage()));
        Assert.AreEqual("my-account", NavigationUrl.GetSegment(new MyAccountPage()));
    }

    [TestMethod]
    public void HashIsBuiltFromTheStack()
    {
        var root = CreatePage("Root");
        root.StyleId = "main";
        var details = CreatePage("Details");
        details.StyleId = "details";
        IView[] stack = [root, details];

        Assert.AreEqual("#/main/details", NavigationUrl.GetHash(stack));
        Assert.AreEqual("#/main", NavigationUrl.GetHash(stack, 1));
        Assert.AreEqual("", NavigationUrl.GetHash(stack, 0));
    }

    [TestMethod]
    public void NormalizeMatchesTheGeneratedShape()
    {
        Assert.AreEqual("#/main/details", NavigationUrl.Normalize("#/main/details"));
        Assert.AreEqual("#/main/details", NavigationUrl.Normalize("/main/details"));
        Assert.AreEqual("#/main", NavigationUrl.Normalize("#/main/"));
        Assert.AreEqual("", NavigationUrl.Normalize(""));
        Assert.AreEqual("", NavigationUrl.Normalize("#"));
    }
}
