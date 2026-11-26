using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ooui.Maui.Handlers;

namespace Ooui.Maui.Tests;

using CheckBox = Microsoft.Maui.Controls.CheckBox;
using Switch = Microsoft.Maui.Controls.Switch;

[TestClass]
public sealed class FormControlTests
{
    static MauiApp app = null!;

    [ClassInitialize]
    public static void Initialize(TestContext context)
    {
        app = MauiApp.CreateBuilder().UseMauiApp<Application>().UseOoui().Build();
    }

    [TestMethod]
    public void EntryMapsPropertiesAndBrowserTextUpdatesBinding()
    {
        using var scope = app.Services.CreateScope();
        var context = new MauiContext(scope.ServiceProvider);
        var model = new TextModel();
        var entry = new Entry
        {
            Placeholder = "E-Mail",
            Keyboard = Keyboard.Email,
            MaxLength = 80,
            IsReadOnly = true,
            BindingContext = model,
        };
        entry.SetBinding(Entry.TextProperty, nameof(TextModel.Text), BindingMode.OneWayToSource);
        var element = (TextInput)entry.ToOouiElement(context);

        Assert.AreEqual(InputType.Email, element.Type);
        Assert.AreEqual("E-Mail", element.Placeholder);
        Assert.AreEqual("80", element.GetStringAttribute("maxlength", ""));
        Assert.IsNotNull(element.GetAttribute("readonly"));

        element.Receive(Message.Event(element.Id, "input", "new@example.com"));
        Assert.AreEqual("new@example.com", model.Text);
        Assert.AreEqual("new@example.com", entry.Text);

        model.Text = "changed@example.com";
        Assert.AreEqual("changed@example.com", model.Text);
    }

    [TestMethod]
    public void EntryFocusInvokesBrowserFocus()
    {
        using var scope = app.Services.CreateScope();
        var context = new MauiContext(scope.ServiceProvider);
        var entry = new Entry();
        var input = (TextInput)entry.ToOouiElement(context);
        Message? focus = null;
        input.MessageSent += message =>
        {
            if (message.MessageType == MessageType.Call && message.Key == "focus") focus = message;
        };

        var result = entry.Focus();

        Assert.IsTrue(result);
        Assert.IsNotNull(focus);
        Assert.AreEqual(input.Id, focus.TargetId);
    }

    [TestMethod]
    public void PasswordEntryAndEditorUseCorrectBrowserControls()
    {
        using var scope = app.Services.CreateScope();
        var context = new MauiContext(scope.ServiceProvider);
        var entry = new Entry { IsPassword = true, Keyboard = Keyboard.Numeric };
        var password = (TextInput)entry.ToOouiElement(context);
        Assert.AreEqual(InputType.Password, password.Type);

        var editor = new Editor { Text = "Line 1\nLine 2", Placeholder = "Beschreibung" };
        var area = (TextArea)editor.ToOouiElement(context);
        Assert.AreEqual("Line 1\nLine 2", area.Value);
        var completed = 0;
        editor.Completed += (_, _) => completed++;
        area.Receive(Message.Event(area.Id, "input", "Aktualisiert"));
        area.Receive(Message.Event(area.Id, "change", "Aktualisiert"));
        Assert.AreEqual("Aktualisiert", editor.Text);
        Assert.AreEqual(1, completed);
    }

    [TestMethod]
    public void CheckBoxAndSwitchRaisePropertyAndControlEvents()
    {
        using var scope = app.Services.CreateScope();
        var context = new MauiContext(scope.ServiceProvider);
        var checkBox = new CheckBox { IsChecked = false };
        var checkedChanges = 0;
        checkBox.CheckedChanged += (_, args) =>
        {
            checkedChanges++;
            Assert.IsTrue(args.Value);
        };
        var check = (Input)checkBox.ToOouiElement(context);
        check.Receive(Message.Event(check.Id, "change", true));
        Assert.IsTrue(checkBox.IsChecked);
        Assert.AreEqual(1, checkedChanges);

        var switchView = new Switch { IsToggled = true };
        var toggles = 0;
        switchView.Toggled += (_, _) => toggles++;
        var toggle = (Input)switchView.ToOouiElement(context);
        Assert.IsTrue(toggle.IsChecked);
        toggle.Receive(Message.Event(toggle.Id, "change", false));
        Assert.IsFalse(switchView.IsToggled);
        Assert.AreEqual(1, toggles);

        switchView.IsToggled = false;
        Assert.IsFalse(toggle.IsChecked);

        switchView.IsToggled = true;
        toggle.Receive(Message.Event(toggle.Id, "change", false));
        switchView.IsToggled = true;
        Assert.IsTrue(toggle.IsChecked);
    }

    [TestMethod]
    public void SliderAndStepperAcceptRangeValuesAndClampThroughMaui()
    {
        using var scope = app.Services.CreateScope();
        var context = new MauiContext(scope.ServiceProvider);
        var slider = new Slider { Minimum = 0, Maximum = 10, Value = 4 };
        var range = (Input)slider.ToOouiElement(context);
        range.Receive(Message.Event(range.Id, "input", "6.5"));
        Assert.AreEqual(6.5, slider.Value);
        Assert.AreEqual("10", range.GetStringAttribute("max", ""));

        var stepper = new Stepper { Minimum = 0, Maximum = 5, Increment = 0.5, Value = 1 };
        var number = (Input)stepper.ToOouiElement(context);
        Assert.AreEqual("0.5", number.GetStringAttribute("step", ""));
        number.Receive(Message.Event(number.Id, "change", "8"));
        Assert.AreEqual(5, stepper.Value);
        Assert.AreEqual("5", number.Value);
    }

    [TestMethod]
    public void PickerSynchronizesItemsAndSelection()
    {
        using var scope = app.Services.CreateScope();
        var context = new MauiContext(scope.ServiceProvider);
        var model = new TextModel();
        var picker = new Picker { Title = "Projekt wählen", ItemsSource = new[] { "Ooui", "MAUI" }, BindingContext = model };
        picker.SetBinding(Picker.SelectedItemProperty, nameof(TextModel.Text), BindingMode.OneWayToSource);
        var select = (Select)picker.ToOouiElement(context);
        Assert.AreEqual(3, select.Children.Count);

        var json = Newtonsoft.Json.JsonConvert.SerializeObject(Message.Event(select.Id, "change", "1"));
        select.Receive(Message.FromJson(json));
        Assert.AreEqual(1, picker.SelectedIndex);
        Assert.AreEqual("MAUI", picker.SelectedItem);
        Assert.AreEqual("MAUI", model.Text);

        picker.SelectedIndex = 0;
        Assert.AreEqual("0", select.Value);

        select.Receive(Message.Event(select.Id, "change", "1"));
        Assert.AreEqual(1, picker.SelectedIndex);
        picker.SelectedIndex = 0;
        Assert.AreEqual("0", select.Value);

        picker.ItemsSource = new[] { "Ooui", "MAUI", "Web" };
        Assert.AreEqual(4, select.Children.Count);
    }

    [TestMethod]
    public void DateTimeAndSearchUseInvariantBrowserValues()
    {
        using var scope = app.Services.CreateScope();
        var context = new MauiContext(scope.ServiceProvider);
        var datePicker = new DatePicker { Date = new DateTime(2026, 7, 14) };
        var date = (Input)datePicker.ToOouiElement(context);
        Assert.AreEqual("2026-07-14", date.Value);
        date.Receive(Message.Event(date.Id, "change", "2027-02-03"));
        Assert.AreEqual(new DateTime(2027, 2, 3), datePicker.Date);

        var timePicker = new TimePicker { Time = new TimeSpan(14, 5, 0) };
        var time = (Input)timePicker.ToOouiElement(context);
        Assert.AreEqual("14:05", time.Value);
        time.Receive(Message.Event(time.Id, "change", "09:30"));
        Assert.AreEqual(new TimeSpan(9, 30, 0), timePicker.Time);

        var searches = 0;
        var searchBar = new SearchBar { Placeholder = "Suchen" };
        searchBar.SearchButtonPressed += (_, _) => searches++;
        var search = (TextInput)searchBar.ToOouiElement(context);
        search.Receive(Message.Event(search.Id, "keydown", "Enter"));
        Assert.AreEqual(1, searches);
    }

    sealed class TextModel : INotifyPropertyChanged
    {
        string? text;
        public event PropertyChangedEventHandler? PropertyChanged;
        public string? Text
        {
            get => text;
            set { text = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text))); }
        }
    }
}
