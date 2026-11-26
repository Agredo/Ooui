using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
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

    sealed class Group : List<string>
    {
        public Group(string name, params string[] items) : base(items) => Name = name;

        public string Name { get; }

        public override string ToString() => Name;
    }

    static (Div PlatformView, Div Items) Render(View view)
    {
        var context = new MauiContext(app.Services);
        var platformView = (Div)view.ToOouiElement(context);
        var items = platformView.Children.OfType<Div>().Single(c => c.ClassName == CollectionViewHandler.ItemsHostClassName);
        return (platformView, items);
    }

    static void Scroll(Div items, double scrollTop, double scrollHeight, double clientHeight) =>
        items.Receive(Message.Event(items.Id, "scroll", JObject.Parse(
            $"{{\"scrollLeft\":0,\"scrollTop\":{scrollTop},\"scrollWidth\":100,\"scrollHeight\":{scrollHeight},\"clientWidth\":100,\"clientHeight\":{clientHeight}}}")));

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
        var items = list.Children.OfType<Div>().Single();

        Assert.AreEqual("listbox", items.GetStringAttribute("role", ""));
        Assert.AreEqual(2, items.Children.Count);
        foreach (var container in items.Children.Cast<Div>())
        {
            Assert.AreEqual("option", container.GetStringAttribute("role", ""));
            Assert.AreEqual("false", container.GetStringAttribute("aria-selected", ""));
            Assert.AreEqual("0", container.GetStringAttribute("tabindex", ""));
        }
        Assert.AreEqual("Ada", ((Ooui.Span)items.Children[0].Children[0]).Text);
        Assert.AreEqual("Grace", ((Ooui.Span)items.Children[1].Children[0]).Text);
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
        var items = list.Children.OfType<Div>().Single();

        Assert.AreEqual("Ada", ((Ooui.Span)items.Children[0].Children[0]).Text);
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
        var items = list.Children.OfType<Div>().Single();
        var first = (Div)items.Children[0];
        var second = (Div)items.Children[1];

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
        var items = list.Children.OfType<Div>().Single();
        var first = (Div)items.Children[0];
        var second = (Div)items.Children[1];

        Assert.AreEqual("true", items.GetStringAttribute("aria-multiselectable", ""));

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
        var items = list.Children.OfType<Div>().Single();
        var container = (Div)items.Children[0];
        container.Receive(Message.Event(container.Id, "click"));

        Assert.AreEqual("list", items.GetStringAttribute("role", ""));
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
        var people = new ObservableCollection<string> { "Ada" };
        var view = new CollectionView { ItemsSource = people, EmptyView = "Nobody here yet." };

        var list = (Div)view.ToOouiElement(context);
        var items = list.Children.OfType<Div>().Single();
        Assert.AreEqual(1, items.Children.Count);

        people.Add("Grace");
        Assert.AreEqual(2, items.Children.Count);

        people.RemoveAt(0);
        Assert.AreEqual(1, items.Children.Count);

        people.Clear();
        Assert.AreEqual(1, items.Children.Count);
        Assert.IsInstanceOfType(items.Children[0], typeof(Ooui.Span));
        Assert.AreEqual("Nobody here yet.", ((Ooui.Span)items.Children[0]).Text);
    }

    [TestMethod]
    public void AddingAnItemKeepsTheExistingContainers()
    {
        var people = new ObservableCollection<string> { "Ada", "Grace" };
        var view = new CollectionView { ItemsSource = people };
        var (_, items) = Render(view);

        var ada = items.Children[0];
        var grace = items.Children[1];

        people.Insert(1, "Alan");

        Assert.AreEqual(3, items.Children.Count);
        Assert.AreSame(ada, items.Children[0]);
        Assert.AreSame(grace, items.Children[2]);
        Assert.AreEqual("Alan", ((Ooui.Span)items.Children[1].Children[0]).Text);

        people.Add("Edsger");

        Assert.AreSame(ada, items.Children[0]);
        Assert.AreSame(grace, items.Children[2]);
        Assert.AreEqual("Edsger", ((Ooui.Span)items.Children[3].Children[0]).Text);
    }

    [TestMethod]
    public void RemovingAnItemKeepsTheOtherContainers()
    {
        var people = new ObservableCollection<string> { "Ada", "Grace", "Alan" };
        var view = new CollectionView { ItemsSource = people };
        var (_, items) = Render(view);

        var ada = items.Children[0];
        var alan = items.Children[2];

        people.RemoveAt(1);

        Assert.AreEqual(2, items.Children.Count);
        Assert.AreSame(ada, items.Children[0]);
        Assert.AreSame(alan, items.Children[1]);
    }

    [TestMethod]
    public void ReplacingAnItemOnlyRenewsThatContainer()
    {
        var people = new ObservableCollection<string> { "Ada", "Grace" };
        var view = new CollectionView { ItemsSource = people };
        var (_, items) = Render(view);

        var grace = items.Children[1];

        people[0] = "Alan";

        Assert.AreEqual(2, items.Children.Count);
        Assert.AreEqual("Alan", ((Ooui.Span)items.Children[0].Children[0]).Text);
        Assert.AreSame(grace, items.Children[1]);
    }

    [TestMethod]
    public void HeaderAndFooterSurroundTheItems()
    {
        var view = new CollectionView { ItemsSource = new[] { "Ada" }, Header = "People", Footer = "End of list" };
        var (list, items) = Render(view);

        Assert.AreEqual(3, list.Children.Count);
        Assert.AreEqual("People", ((Ooui.Span)list.Children[0]).Text);
        Assert.AreSame(items, list.Children[1]);
        Assert.AreEqual("End of list", ((Ooui.Span)list.Children[2]).Text);
    }

    [TestMethod]
    public void HeaderTemplateBindsTheHeaderData()
    {
        var view = new CollectionView
        {
            ItemsSource = new[] { "Ada" },
            Header = new Person("Ada Lovelace"),
            HeaderTemplate = new DataTemplate(() =>
            {
                var label = new MauiLabel();
                label.SetBinding(MauiLabel.TextProperty, nameof(Person.Name));
                return label;
            }),
        };
        var (list, _) = Render(view);

        Assert.AreEqual("Ada Lovelace", ((Ooui.Span)list.Children[0]).Text);
    }

    [TestMethod]
    public void GroupedSourceRendersOneBlockPerGroup()
    {
        var groups = new ObservableCollection<Group>
        {
            new("Team A", "Ada", "Alan"),
            new("Team B", "Grace"),
        };
        var view = new CollectionView { ItemsSource = groups, IsGrouped = true, SelectionMode = SelectionMode.Single };
        var (_, items) = Render(view);

        Assert.AreEqual(2, items.Children.Count);
        var first = (Div)items.Children[0];
        Assert.AreEqual("group", first.GetStringAttribute("role", ""));
        // group header + two items
        Assert.AreEqual(3, first.Children.Count);
        Assert.AreEqual("Team A", ((Ooui.Span)first.Children[0]).Text);
        Assert.AreEqual(2, ((Div)items.Children[1]).Children.Count);

        var row = (Div)first.Children[1];
        row.Receive(Message.Event(row.Id, "click"));

        Assert.AreEqual("Ada", view.SelectedItem);
    }

    [TestMethod]
    public void EmptyViewTemplateBindsTheEmptyViewData()
    {
        var view = new CollectionView
        {
            ItemsSource = Array.Empty<string>(),
            EmptyView = new Person("Nobody"),
            EmptyViewTemplate = new DataTemplate(() =>
            {
                var label = new MauiLabel();
                label.SetBinding(MauiLabel.TextProperty, nameof(Person.Name));
                return label;
            }),
        };
        var (_, items) = Render(view);

        Assert.AreEqual(1, items.Children.Count);
        Assert.AreEqual("Nobody", ((Ooui.Span)items.Children[0]).Text);
    }

    [TestMethod]
    public void ScrollingNearTheEndRaisesTheThresholdEvent()
    {
        var view = new CollectionView
        {
            ItemsSource = new[] { "Ada", "Grace", "Alan", "Edsger" },
            RemainingItemsThreshold = 1,
        };
        var raised = 0;
        view.RemainingItemsThresholdReached += (_, _) => raised++;
        var (_, items) = Render(view);

        Scroll(items, scrollTop: 0, scrollHeight: 400, clientHeight: 100);
        Assert.AreEqual(0, raised);

        Scroll(items, scrollTop: 350, scrollHeight: 400, clientHeight: 100);
        Assert.AreEqual(1, raised);

        // The event stays quiet until the items changed and the browser scrolls again.
        Scroll(items, scrollTop: 360, scrollHeight: 400, clientHeight: 100);
        Assert.AreEqual(1, raised);
    }

    [TestMethod]
    public void ScrollingReportsTheVisibleRows()
    {
        var view = new CollectionView { ItemsSource = new[] { "Ada", "Grace", "Alan", "Edsger" } };
        ItemsViewScrolledEventArgs? last = null;
        view.Scrolled += (_, e) => last = e;
        var (_, items) = Render(view);

        Scroll(items, scrollTop: 150, scrollHeight: 400, clientHeight: 100);

        Assert.AreEqual(150, last!.VerticalOffset);
        Assert.AreEqual(1, last.FirstVisibleItemIndex);
        Assert.AreEqual(2, last.CenterItemIndex);
        Assert.AreEqual(2, last.LastVisibleItemIndex);
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
        var linearItems = linearList.Children.OfType<Div>().Single();

        Assert.AreEqual("flex", linearItems.Style.Display);
        Assert.AreEqual("row", linearItems.Style["flex-direction"]);
        Assert.AreEqual("8px", linearItems.Style["row-gap"]);
        Assert.AreEqual("auto", linearItems.Style.Overflow);

        var grid = new CollectionView
        {
            ItemsSource = new[] { new Person("Ada") },
            ItemsLayout = new GridItemsLayout(2, ItemsLayoutOrientation.Vertical) { HorizontalItemSpacing = 12 },
        };

        var gridList = (Div)grid.ToOouiElement(context);
        var gridItems = gridList.Children.OfType<Div>().Single();

        Assert.AreEqual("grid", gridItems.Style.Display);
        Assert.AreEqual("repeat(2, minmax(0, 1fr))", gridItems.Style["grid-template-columns"]);
        Assert.AreEqual("12px", gridItems.Style["column-gap"]);
    }
}
