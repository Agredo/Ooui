using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ooui.Maui.Handlers;

namespace Ooui.Maui.Tests;

using CollectionView = Microsoft.Maui.Controls.CollectionView;
using MauiLabel = Microsoft.Maui.Controls.Label;

[TestClass]
public sealed class CollectionViewTests
{
    static MauiApp app = null!;

    [ClassInitialize]
    public static void Initialize(TestContext context)
    {
        app = MauiApp.CreateBuilder().UseMauiApp<Application>().UseOoui().Build();
    }

    sealed record Person(string Name);

    [TestMethod]
    public void ItemsRenderOneSelectableContainerPerItem()
    {
        using var scope = app.Services.CreateScope();
        var context = new MauiContext(scope.ServiceProvider);
        var view = new CollectionView
        {
            ItemsSource = new View[] { new MauiLabel { Text = "Ada" }, new MauiLabel { Text = "Grace" } },
            SelectionMode = SelectionMode.Single,
        };

        var list = (Div)view.ToOouiElement(context);

        Assert.AreEqual("listbox", list.GetStringAttribute("role", ""));
        Assert.AreEqual(2, list.Children.Count);
        foreach (var container in list.Children.Cast<Div>())
        {
            Assert.AreEqual("option", container.GetStringAttribute("role", ""));
            Assert.AreEqual("false", container.GetStringAttribute("aria-selected", ""));
            Assert.AreEqual("0", container.GetStringAttribute("tabindex", ""));
        }
        Assert.AreEqual("Ada", ((Ooui.Span)list.Children[0].Children[0]).Text);
        Assert.AreEqual("Grace", ((Ooui.Span)list.Children[1].Children[0]).Text);
    }

    [TestMethod]
    public void ItemTemplateBindsEachItem()
    {
        using var scope = app.Services.CreateScope();
        var context = new MauiContext(scope.ServiceProvider);
        var view = new CollectionView
        {
            ItemsSource = new[] { new Person("Ada") },
            ItemTemplate = new DataTemplate(() =>
            {
                var label = new MauiLabel();
                label.SetBinding(MauiLabel.TextProperty, nameof(Person.Name));
                return label;
            }),
        };

        var list = (Div)view.ToOouiElement(context);

        Assert.AreEqual("Ada", ((Ooui.Span)list.Children[0].Children[0]).Text);
    }

    [TestMethod]
    public void ClickingARowSelectsItAndRaisesSelectionChanged()
    {
        using var scope = app.Services.CreateScope();
        var context = new MauiContext(scope.ServiceProvider);
        var people = new[] { new Person("Ada"), new Person("Grace") };
        var view = new CollectionView { ItemsSource = people, SelectionMode = SelectionMode.Single };
        SelectionChangedEventArgs? last = null;
        view.SelectionChanged += (_, e) => last = e;

        var list = (Div)view.ToOouiElement(context);
        var first = (Div)list.Children[0];
        var second = (Div)list.Children[1];

        first.Receive(Message.Event(first.Id, "click"));

        Assert.AreSame(people[0], view.SelectedItem);
        Assert.AreSame(people[0], last!.CurrentSelection.Single());
        Assert.AreEqual("true", first.GetStringAttribute("aria-selected", ""));
        Assert.AreEqual("false", second.GetStringAttribute("aria-selected", ""));

        // Clicking the selected row again clears the selection.
        first.Receive(Message.Event(first.Id, "click"));

        Assert.IsNull(view.SelectedItem);
        Assert.AreEqual("false", first.GetStringAttribute("aria-selected", ""));
    }

    [TestMethod]
    public void MultipleSelectionTogglesRows()
    {
        using var scope = app.Services.CreateScope();
        var context = new MauiContext(scope.ServiceProvider);
        var people = new[] { new Person("Ada"), new Person("Grace") };
        var view = new CollectionView { ItemsSource = people, SelectionMode = SelectionMode.Multiple };

        var list = (Div)view.ToOouiElement(context);
        var first = (Div)list.Children[0];
        var second = (Div)list.Children[1];

        Assert.AreEqual("true", list.GetStringAttribute("aria-multiselectable", ""));

        first.Receive(Message.Event(first.Id, "click"));
        second.Receive(Message.Event(second.Id, "click"));

        CollectionAssert.AreEqual(new[] { people[0], people[1] }, view.SelectedItems.ToArray());
        Assert.AreEqual("true", first.GetStringAttribute("aria-selected", ""));
        Assert.AreEqual("true", second.GetStringAttribute("aria-selected", ""));

        first.Receive(Message.Event(first.Id, "click"));

        Assert.AreSame(people[1], view.SelectedItems.Single());
        Assert.AreEqual("false", first.GetStringAttribute("aria-selected", ""));
    }

    [TestMethod]
    public void SelectionModeNoneRendersAPlainList()
    {
        using var scope = app.Services.CreateScope();
        var context = new MauiContext(scope.ServiceProvider);
        var view = new CollectionView
        {
            ItemsSource = new[] { new Person("Ada") },
            SelectionMode = SelectionMode.None,
        };

        var list = (Div)view.ToOouiElement(context);
        var container = (Div)list.Children[0];
        container.Receive(Message.Event(container.Id, "click"));

        Assert.AreEqual("list", list.GetStringAttribute("role", ""));
        Assert.AreEqual("listitem", container.GetStringAttribute("role", ""));
        Assert.IsNull(container.GetAttribute("aria-selected"));
        Assert.IsNull(container.GetAttribute("tabindex"));
        Assert.IsNull(view.SelectedItem);
    }

    [TestMethod]
    public void ObservableSourceUpdatesTheRenderedRows()
    {
        using var scope = app.Services.CreateScope();
        var context = new MauiContext(scope.ServiceProvider);
        var people = new ObservableCollection<Person> { new("Ada") };
        var view = new CollectionView { ItemsSource = people, EmptyView = "Nobody here yet." };

        var list = (Div)view.ToOouiElement(context);
        Assert.AreEqual(1, list.Children.Count);

        people.Add(new Person("Grace"));
        Assert.AreEqual(2, list.Children.Count);

        people.RemoveAt(0);
        Assert.AreEqual(1, list.Children.Count);

        people.Clear();
        Assert.AreEqual(1, list.Children.Count);
        Assert.IsInstanceOfType(list.Children[0], typeof(Ooui.Span));
        Assert.AreEqual("Nobody here yet.", ((Ooui.Span)list.Children[0]).Text);
    }

    [TestMethod]
    public void ItemsLayoutBecomesCss()
    {
        using var scope = app.Services.CreateScope();
        var context = new MauiContext(scope.ServiceProvider);
        var linear = new CollectionView
        {
            ItemsSource = new[] { new Person("Ada") },
            ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal) { ItemSpacing = 8 },
        };

        var linearList = (Div)linear.ToOouiElement(context);

        Assert.AreEqual("flex", linearList.Style.Display);
        Assert.AreEqual("row", linearList.Style["flex-direction"]);
        Assert.AreEqual("8px", linearList.Style["row-gap"]);

        var grid = new CollectionView
        {
            ItemsSource = new[] { new Person("Ada") },
            ItemsLayout = new GridItemsLayout(2, ItemsLayoutOrientation.Vertical) { HorizontalItemSpacing = 12 },
        };

        var gridList = (Div)grid.ToOouiElement(context);

        Assert.AreEqual("grid", gridList.Style.Display);
        Assert.AreEqual("repeat(2, minmax(0, 1fr))", gridList.Style["grid-template-columns"]);
        Assert.AreEqual("12px", gridList.Style["column-gap"]);
    }
}
