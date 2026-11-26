using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ooui.Maui.Handlers;

namespace Ooui.Maui.Tests;

using ListView = Microsoft.Maui.Controls.ListView;
using MauiLabel = Microsoft.Maui.Controls.Label;

// ListView and the cell types are deprecated in MAUI, but the handler supports existing apps.
#pragma warning disable CS0618

[TestClass]
public sealed class ListViewTests
{
    static MauiApp app = null!;

    [ClassInitialize]
    public static void Initialize(TestContext context)
    {
        app = MauiApp.CreateBuilder().UseMauiApp<Application>().UseOoui().Build();
    }

    static (Div PlatformView, List<Div> Rows) Render(View view)
    {
        var context = new MauiContext(app.Services);
        var platformView = (Div)view.ToOouiElement(context);
        var items = platformView.Children.OfType<Div>().Single(c => c.ClassName == ListViewHandler.ItemsHostClassName);
        return (platformView, items.Children.OfType<Div>().ToList());
    }

    static DataTemplate TextTemplate() =>
        new(() =>
        {
            var label = new MauiLabel();
            label.SetBinding(MauiLabel.TextProperty, ".");
            return new ViewCell { View = label };
        });

    [TestMethod]
    public void ViewCellTemplateRendersOneRowPerItem()
    {
        var list = new ListView { ItemsSource = new[] { "Alpha", "Beta", "Gamma" }, ItemTemplate = TextTemplate() };

        var (_, rows) = Render(list);

        Assert.AreEqual(3, rows.Count);
        CollectionAssert.AreEqual(
            new[] { "Alpha", "Beta", "Gamma" },
            rows.Select(r => ((Span)r.Children[0]).Text).ToArray());
    }

    [TestMethod]
    public void ClickingARowRaisesItemTappedAndSelectsIt()
    {
        var list = new ListView { ItemsSource = new[] { "Alpha", "Beta" }, ItemTemplate = TextTemplate() };
        var tapped = new List<object?>();
        list.ItemTapped += (_, args) => tapped.Add(args.Item);

        var (_, rows) = Render(list);
        rows[1].Receive(Message.Event(rows[1].Id, "click"));

        CollectionAssert.AreEqual(new object?[] { "Beta" }, tapped);
        Assert.AreEqual("Beta", list.SelectedItem);
        Assert.AreEqual("true", rows[1].GetStringAttribute("aria-selected", ""));
    }

    [TestMethod]
    public void TextCellTemplateRendersTitleAndDetail()
    {
        var list = new ListView
        {
            ItemTemplate = new DataTemplate(() =>
            {
                var cell = new TextCell();
                cell.SetBinding(TextCell.TextProperty, "Name");
                cell.SetBinding(TextCell.DetailProperty, "City");
                return cell;
            }),
        };
        list.ItemsSource = new[] { new Person("Ada", "London") };

        var (_, rows) = Render(list);

        Assert.AreEqual(1, rows.Count);
        var texts = rows[0].Children[0].Children.OfType<Span>().ToList();
        Assert.AreEqual("Ada", texts[0].Text);
        Assert.AreEqual("London", texts[1].Text);
    }

    sealed record Person(string Name, string City);

    [TestMethod]
    public void ItemsWithoutTemplateShowTheirText()
    {
        var list = new ListView { ItemsSource = new[] { "Ohne Vorlage" } };

        var (_, rows) = Render(list);

        Assert.AreEqual(1, rows.Count);
        Assert.AreEqual("Ohne Vorlage", ((Span)rows[0].Children[0].Children[0]).Text);
    }

    [TestMethod]
    public void HeaderAndFooterSurroundTheRows()
    {
        var list = new ListView
        {
            ItemsSource = new[] { "Alpha" },
            ItemTemplate = TextTemplate(),
            Header = "Kopfzeile",
            Footer = "Fußzeile",
        };

        var (platformView, rows) = Render(list);

        Assert.AreEqual(1, rows.Count);
        var header = platformView.Children.OfType<Element>().Single(c => c.ClassName == ListViewHandler.HeaderClassName);
        var footer = platformView.Children.OfType<Element>().Single(c => c.ClassName == ListViewHandler.FooterClassName);
        Assert.AreEqual("Kopfzeile", ((Span)header).Text);
        Assert.AreEqual("Fußzeile", ((Span)footer).Text);
        var children = platformView.Children.ToList();
        Assert.IsTrue(children.IndexOf(header) < children.IndexOf(footer));
    }

    [TestMethod]
    public void AddingAnItemRebuildsTheRows()
    {
        var source = new ObservableCollection<string> { "Alpha" };
        var list = new ListView { ItemsSource = source, ItemTemplate = TextTemplate() };

        var (_, rows) = Render(list);
        Assert.AreEqual(1, rows.Count);

        source.Add("Beta");

        var itemsHost = list.Handler!.PlatformView as Div;
        var updated = itemsHost!.Children.OfType<Div>().Single(c => c.ClassName == ListViewHandler.ItemsHostClassName);
        Assert.AreEqual(2, updated.Children.Count);
    }

    [TestMethod]
    public void HidingTheSeparatorsRemovesTheBorder()
    {
        var list = new ListView
        {
            ItemsSource = new[] { "Alpha", "Beta" },
            ItemTemplate = TextTemplate(),
            SeparatorVisibility = SeparatorVisibility.None,
        };

        var (_, rows) = Render(list);

        // Ooui reports unset properties as "inherit".
        Assert.AreEqual("inherit", rows[0].Style["border-bottom"]);
    }

    [TestMethod]
    public void DefaultSeparatorsDrawALineBetweenRows()
    {
        var list = new ListView { ItemsSource = new[] { "Alpha", "Beta" }, ItemTemplate = TextTemplate() };

        var (_, rows) = Render(list);

        StringAssert.Contains((string)rows[0].Style["border-bottom"], "solid");
        Assert.AreEqual("inherit", rows[1].Style["border-bottom"]);
    }
}
