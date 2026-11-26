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
    public void EntryMapsKeyboardMaxLengthReturnTypeAndSpellCheck()
    {
        using var scope = app.Services.CreateScope();
        var context = new MauiContext(scope.ServiceProvider);
        var entry = new Entry
        {
            Keyboard = Keyboard.Numeric,
            ReturnType = ReturnType.Go,
            IsSpellCheckEnabled = false,
        };
        var input = (TextInput)entry.ToOouiElement(context);

        Assert.AreEqual(InputType.Text, input.Type);
        Assert.AreEqual("decimal", input.GetStringAttribute("inputmode", ""));
        Assert.AreEqual("go", input.GetStringAttribute("enterkeyhint", ""));
        Assert.AreEqual("false", input.GetStringAttribute("spellcheck", ""));

        entry.Keyboard = Keyboard.Url;
        Assert.AreEqual(InputType.Url, input.Type);
        Assert.AreEqual("", input.GetStringAttribute("inputmode", ""));

        entry.Keyboard = Keyboard.Telephone;
        Assert.AreEqual(InputType.Tel, input.Type);

        entry.ReturnType = ReturnType.Search;
        Assert.AreEqual("search", input.GetStringAttribute("enterkeyhint", ""));

        entry.IsSpellCheckEnabled = true;
        Assert.AreEqual("true", input.GetStringAttribute("spellcheck", ""));

        var limited = new Entry { MaxLength = 12 };
        var limitedInput = (TextInput)limited.ToOouiElement(context);
        Assert.AreEqual("12", limitedInput.GetStringAttribute("maxlength", ""));
        limited.MaxLength = int.MaxValue;
        Assert.IsNull(limitedInput.GetAttribute("maxlength"));

        var unlimitedEditor = new Editor { MaxLength = int.MaxValue };
        var area = (TextArea)unlimitedEditor.ToOouiElement(context);
        Assert.IsNull(area.GetAttribute("maxlength"));
        unlimitedEditor.MaxLength = 40;
        Assert.AreEqual("40", area.GetStringAttribute("maxlength", ""));
    }

    [TestMethod]
    public void EntryRaisesCompletedOnTheEnterKey()
    {
        using var scope = app.Services.CreateScope();
        var context = new MauiContext(scope.ServiceProvider);
        var entry = new Entry { Text = "Projekt" };
        var completed = 0;
        entry.Completed += (_, _) => completed++;
        var input = (TextInput)entry.ToOouiElement(context);

        input.Receive(Message.Event(input.Id, "keydown", "Enter"));
        Assert.AreEqual(1, completed);

        input.Receive(Message.Event(input.Id, "keydown", "Tab"));
        Assert.AreEqual(1, completed);
    }

    [TestMethod]
    public void EntryCompletesOnceForEnterAndAgainAfterTyping()
    {
        using var scope = app.Services.CreateScope();
        var context = new MauiContext(scope.ServiceProvider);
        var entry = new Entry { Text = "Projekt" };
        var completed = 0;
        entry.Completed += (_, _) => completed++;
        var input = (TextInput)entry.ToOouiElement(context);

        input.Receive(Message.Event(input.Id, "keydown", "Enter"));
        input.Receive(Message.Event(input.Id, "change", "Projekt"));
        Assert.AreEqual(1, completed);

        input.Receive(Message.Event(input.Id, "input", "Projekt2"));
        input.Receive(Message.Event(input.Id, "change", "Projekt2"));
        Assert.AreEqual(2, completed);

        input.Receive(Message.Event(input.Id, "keydown", "Enter"));
        Assert.AreEqual(3, completed);
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

        var searches = 0;
        var searchBar = new SearchBar { Placeholder = "Suchen" };
        searchBar.SearchButtonPressed += (_, _) => searches++;
        var search = (TextInput)searchBar.ToOouiElement(context);
        search.Receive(Message.Event(search.Id, "keydown", "Enter"));
        Assert.AreEqual(1, searches);
    }

    [TestMethod]
    public void DatePickerKeepsTheNativeValueAndShowsCustomFormatsInASpan()
    {
        using var scope = app.Services.CreateScope();
        var context = new MauiContext(scope.ServiceProvider);
        var datePicker = new DatePicker { Date = new DateTime(2027, 3, 5) };
        var wrapper = (Div)datePicker.ToOouiElement(context);
        var display = (Span)wrapper.Children[0];
        var date = (Input)wrapper.Children[1];

        Assert.AreEqual("relative", wrapper.Style["position"]);
        Assert.AreEqual("none", display.Style["display"]);
        Assert.AreEqual("2027-03-05", date.Value);
        Assert.AreEqual("inherit", date.Style["position"]);

        datePicker.Format = "dd.MM.yyyy";
        Assert.AreEqual("inline-block", display.Style["display"]);
        Assert.AreEqual("05.03.2027", display.Text);
        Assert.AreEqual("2027-03-05", date.Value);
        Assert.AreEqual("absolute", date.Style["position"]);

        date.Receive(Message.Event(date.Id, "change", "2028-01-02"));
        Assert.AreEqual(new DateTime(2028, 1, 2), datePicker.Date);
        Assert.AreEqual("02.01.2028", display.Text);

        datePicker.Format = "d";
        Assert.AreEqual("none", display.Style["display"]);
        Assert.AreEqual("inherit", date.Style["position"]);
    }

    [TestMethod]
    public void TimePickerKeepsTheNativeValueAndShowsCustomFormatsInASpan()
    {
        using var scope = app.Services.CreateScope();
        var context = new MauiContext(scope.ServiceProvider);
        var timePicker = new TimePicker { Time = new TimeSpan(14, 5, 0) };
        var wrapper = (Div)timePicker.ToOouiElement(context);
        var display = (Span)wrapper.Children[0];
        var time = (Input)wrapper.Children[1];

        Assert.AreEqual("14:05", time.Value);
        Assert.AreEqual("none", display.Style["display"]);

        timePicker.Format = "HH.mm";
        Assert.AreEqual("14.05", display.Text);
        Assert.AreEqual("14:05", time.Value);

        time.Receive(Message.Event(time.Id, "change", "09:30"));
        Assert.AreEqual(new TimeSpan(9, 30, 0), timePicker.Time);
        Assert.AreEqual("09.30", display.Text);

        timePicker.Format = "t";
        Assert.AreEqual("none", display.Style["display"]);
    }

    [TestMethod]
    public void SliderAndSwitchMergeTheirColorsIntoAccentColor()
    {
        using var scope = app.Services.CreateScope();
        var context = new MauiContext(scope.ServiceProvider);
        var red = Microsoft.Maui.Graphics.Color.FromArgb("#FF0000");
        var blue = Microsoft.Maui.Graphics.Color.FromArgb("#0000FF");

        var slider = new Slider { MinimumTrackColor = red, ThumbColor = blue };
        var range = (Input)slider.ToOouiElement(context);
        Assert.AreEqual("#ff0000ff", range.Style["accent-color"].ToString()!.ToLowerInvariant());

        var switchView = new Switch { OnColor = red, ThumbColor = blue };
        var toggle = (Input)switchView.ToOouiElement(context);
        Assert.AreEqual("#ff0000ff", toggle.Style["accent-color"].ToString()!.ToLowerInvariant());

        switchView.OnColor = null!;
        Assert.AreEqual("#0000ffff", toggle.Style["accent-color"].ToString()!.ToLowerInvariant());
    }

    [TestMethod]
    public void RadioButtonsShareAGroupAndFollowBrowserChanges()
    {
        using var scope = app.Services.CreateScope();
        var context = new MauiContext(scope.ServiceProvider);
        var small = new RadioButton { Content = "Small", GroupName = "size" };
        var large = new RadioButton { Content = "Large", GroupName = "size" };
        var layout = new VerticalStackLayout { small, large };
        var page = new ContentPage { Content = layout };
        var root = layout.ToOouiElement(context);
        var smallElement = (Label)root.Children[0];
        var largeElement = (Label)root.Children[1];
        var smallInput = (Input)smallElement.Children[0];
        var largeInput = (Input)largeElement.Children[0];

        Assert.AreEqual(InputType.Radio, largeInput.Type);
        Assert.AreEqual("size", largeInput.Name);
        Assert.AreEqual("Large", largeElement.Text);
        Assert.IsFalse(smallInput.IsChecked);

        smallInput.Receive(Message.Event(smallInput.Id, "change", true));
        Assert.IsTrue(small.IsChecked);
        Assert.IsTrue(smallInput.IsChecked);

        largeInput.Receive(Message.Event(largeInput.Id, "change", true));
        Assert.IsTrue(large.IsChecked);
        Assert.IsFalse(small.IsChecked);
        Assert.IsFalse(smallInput.IsChecked);
    }

    [TestMethod]
    public void ImageButtonShowsItsImageAndRaisesClicked()
    {
        using var scope = app.Services.CreateScope();
        var context = new MauiContext(scope.ServiceProvider);
        var clicks = 0;
        var button = new ImageButton { Source = "logo.svg", Aspect = Aspect.AspectFill };
        button.Clicked += (_, _) => clicks++;
        var element = (Button)button.ToOouiElement(context);
        var image = (Image)element.Children[0];

        Assert.AreEqual("/logo.svg", image.Source);
        Assert.AreEqual("cover", image.Style["object-fit"]);
        element.Receive(Message.Event(element.Id, "click"));
        Assert.AreEqual(1, clicks);
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
