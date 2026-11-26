using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace AspNetCoreMaui;

/// <summary>
/// The pages rendered by <see cref="Controllers.DemoController"/>. They are ordinary MAUI pages:
/// no HTML, no JavaScript, no view models required.
/// </summary>
static class Demos
{
    static readonly Color Ink = Color.FromArgb ("#172238");
    static readonly Color Muted = Color.FromArgb ("#6f7990");
    static readonly Color Accent = Color.FromArgb ("#6854d9");
    static readonly Color Line = Color.FromArgb ("#e0e5f0");
    static readonly Color Success = Color.FromArgb ("#31846a");
    static readonly Color Danger = Color.FromArgb ("#bb3d57");

    public const string LogoSvg = """
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 96 96">
          <rect x="4" y="4" width="88" height="88" rx="22" fill="#17213f"/>
          <circle cx="48" cy="54" r="19" fill="none" stroke="#8f7dff" stroke-width="9"/>
          <path d="M34 28h28" stroke="#8f7dff" stroke-width="9" stroke-linecap="round"/>
        </svg>
        """;

    public static ContentPage Overview (SiteState state, Ooui.AspNetCore.OouiUserSession session) => Page (
        new Border {
            AutomationId = "hero",
            BackgroundColor = Color.FromArgb ("#17213f"),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 24 },
            Padding = new Thickness (34),
            Content = new VerticalStackLayout {
                Spacing = 14,
                Children = {
                    new Label { Text = "OOUI  /  MAUI 11  /  ASP.NET CORE", TextColor = Color.FromArgb ("#bdb5ff"), FontSize = 13, CharacterSpacing = 2 },
                    new Label { Text = "Your next website, written in C#.", TextColor = Colors.White, FontSize = 42, FontAttributes = FontAttributes.Bold, AutomationId = "hero-title" },
                    new Label { Text = "Familiar .NET controls become native browser elements. State stays on the server, each browser gets its own session.", TextColor = Color.FromArgb ("#d3d0ef"), FontSize = 18, LineHeight = 1.35 },
                },
            },
        },
        Card ("Server-rendered MAUI",
            "Ooui.Maui registers a MAUI handler per control and translates the visual tree into HTML elements.",
            Body ("A click is sent over the WebSocket, handled in C#, and only the changed attributes are sent back.")),
        Card ("State that outlives the page",
            "Pages are rebuilt on every request. Everything that has to survive lives in the user session of this browser.",
            Body ($"Counter: {state.Count} · pages seen: {state.Pages} · name: {(string.IsNullOrWhiteSpace (state.Name) ? "not entered yet" : state.Name)}"),
            Small ($"Session {session.Id[..8]} · stored in memory while you keep browsing. Navigation, reload and the back button all keep it.")),
        Card ("A small, honest foundation",
            "Labels, buttons, inputs, pickers, sliders, grids, borders, a CollectionView and a few more controls are supported.",
            Small ("Shell, ListView, WebView and animations are not implemented yet. This sample is a demonstration, not a production platform.")));

    public static ContentPage Counter (SiteState state)
    {
        var value = new Label { Text = state.Count.ToString (), FontSize = 64, FontAttributes = FontAttributes.Bold, TextColor = Accent, AutomationId = "count" };
        var status = Small (state.CounterNote);

        void Update (string message)
        {
            state.CounterNote = message;
            value.Text = state.Count.ToString ();
            status.Text = message;
            status.TextColor = Muted;
        }

        var decrement = Secondary ("−1", "decrement");
        decrement.Clicked += (_, _) => { state.Count--; Update ($"Decremented to {state.Count}."); };

        var increment = Primary ("+1", "increment");
        increment.Clicked += (_, _) => { state.Count++; Update ($"Incremented to {state.Count}."); };

        var reset = Secondary ("Reset", "reset");
        reset.Clicked += (_, _) => { state.Count = 0; Update ("Counter reset."); };

        return Page (
            Heading ("Counter"),
            Lead ("Each click travels to the server over this page's WebSocket and back."),
            new Border {
                BackgroundColor = Colors.White,
                Stroke = new SolidColorBrush (Line),
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = 18 },
                Padding = new Thickness (28),
                Content = new VerticalStackLayout {
                    Spacing = 16,
                    Children = {
                        value,
                        new HorizontalStackLayout { Spacing = 12, Children = { decrement, increment, reset } },
                        status,
                    },
                },
            },
            Card ("Why does the number stay?",
                "The counter lives in the user session of this browser, not in the page and not in the tab.",
                Small ("Visit another page or reload: the number is still there. Open the site in a second browser and you get a second session, starting at 0.")));
    }

    public static ContentPage Form (SiteState state)
    {
        var name = new Entry { Placeholder = "e.g. Northstar", Text = state.Name, AutomationId = "name" };
        name.TextChanged += (_, e) => state.Name = e.NewTextValue ?? "";
        var email = new Entry { Placeholder = "you@example.com", Text = state.Email, AutomationId = "email" };
        email.TextChanged += (_, e) => state.Email = e.NewTextValue ?? "";
        var role = new Picker { Title = "Choose a project type", AutomationId = "role" };
        role.ItemsSource = new[] { "Marketing site", "Customer portal", "Internal dashboard" };
        role.SelectedIndex = 0;
        var date = new DatePicker { Date = DateTime.Today.AddDays (14), AutomationId = "date" };
        var time = new TimePicker { Time = new TimeSpan (10, 0, 0), AutomationId = "time" };

        var budget = new Slider { Minimum = 0, Maximum = 100, Value = 68, ThumbColor = Accent, MinimumTrackColor = Accent, AutomationId = "budget" };
        var budgetValue = new Label { Text = "68 %", FontSize = 18, FontAttributes = FontAttributes.Bold, TextColor = Ink, AutomationId = "budget-value" };
        var progress = new ProgressBar { Progress = .68, ProgressColor = Accent, HeightRequest = 10 };
        budget.ValueChanged += (_, e) => {
            budgetValue.Text = $"{e.NewValue:0} %";
            progress.Progress = e.NewValue / 100;
        };

        var notifications = new Switch { IsToggled = true, OnColor = Accent, AutomationId = "notifications" };
        var terms = new CheckBox { IsChecked = false, Color = Accent, AutomationId = "terms" };
        var notes = new Editor { Placeholder = "What makes this project special?", Text = state.Notes, HeightRequest = 110, MaxLength = 400, AutomationId = "notes" };
        notes.TextChanged += (_, e) => state.Notes = e.NewTextValue ?? "";

        var status = Small (state.FormNote);
        status.AutomationId = "form-status";

        var submit = Primary ("Submit", "submit");
        submit.Clicked += (_, _) => {
            if (string.IsNullOrWhiteSpace (name.Text)) {
                status.TextColor = Danger;
                status.Text = "Please enter a project name.";
                name.Focus ();
                return;
            }
            if (!terms.IsChecked) {
                status.TextColor = Danger;
                status.Text = "Please accept the terms first.";
                return;
            }
            status.TextColor = Success;
            var at = time.Time ?? new TimeSpan (10, 0, 0);
            var when = $"{date.Date:yyyy-MM-dd} {at.Hours:00}:{at.Minutes:00}";
            state.FormNote = $"Saved {name.Text.Trim ()} · {role.SelectedItem} · {when} · budget {budget.Value:0} %"
                + (notifications.IsToggled ? " · team notified" : "");
            status.Text = state.FormNote;
        };

        var clear = Secondary ("Reset", "reset-form");
        clear.Clicked += (_, _) => {
            name.Text = string.Empty;
            email.Text = string.Empty;
            notes.Text = string.Empty;
            state.Name = "";
            state.Email = "";
            state.Notes = "";
            role.SelectedIndex = 0;
            date.Date = DateTime.Today.AddDays (14);
            time.Time = new TimeSpan (10, 0, 0);
            budget.Value = 68;
            notifications.IsToggled = true;
            terms.IsChecked = false;
            state.FormNote = "Sample values restored. Only this session changed.";
            status.TextColor = Muted;
            status.Text = state.FormNote;
        };

        return Page (
            Heading ("Form"),
            Lead ("Entries, pickers, sliders and switches — all bound to plain C# state."),
            Card ("Launch a new idea",
                "Your draft belongs to this browser: type a name, visit another page, come back — it is still there.",
                Caption ("PROJECT NAME"), name,
                Caption ("CONTACT"), email,
                Caption ("PROJECT TYPE"), role,
                new Grid {
                    ColumnSpacing = 16,
                    ColumnDefinitions = new ColumnDefinitionCollection { new (GridLength.Star), new (GridLength.Star) },
                    RowDefinitions = new RowDefinitionCollection { new (GridLength.Auto) },
                    Children = {
                        Cell (Caption ("LAUNCH DATE"), date, 0),
                        Cell (Caption ("LAUNCH TIME"), time, 1),
                    },
                },
                Caption ("BUDGET"), budgetValue, progress, budget,
                new HorizontalStackLayout { Spacing = 12, Children = { notifications, Body ("Notify the launch team") } },
                new HorizontalStackLayout { Spacing = 12, Children = { terms, Body ("I accept the terms") } },
                Caption ("NOTES"), notes,
                new HorizontalStackLayout { Spacing = 12, Children = { submit, clear } },
                status));
    }

    public static ContentPage People (SiteState state)
    {
        var status = Small (state.ListNote);
        status.AutomationId = "list-status";

        var remove = Secondary ("Remove selected", "remove-person");
        remove.IsEnabled = state.SelectedPerson is not null;

        var list = new CollectionView {
            AutomationId = "people",
            ItemsSource = state.People,
            ItemTemplate = new DataTemplate (PersonTemplate),
            EmptyView = "Nobody here yet. Use “Add person” to start the list.",
            SelectionMode = SelectionMode.Single,
            SelectedItem = state.SelectedPerson,
            ItemsLayout = new LinearItemsLayout (ItemsLayoutOrientation.Vertical) { ItemSpacing = 12 },
        };

        void Show (string? message = null)
        {
            var person = state.SelectedPerson;
            state.ListNote = message ?? (person is null ? "Nobody selected yet." : $"{person.Name} · {person.Role}");
            status.TextColor = person is null ? Muted : Success;
            status.Text = state.ListNote;
            remove.IsEnabled = person is not null;
        }

        list.SelectionChanged += (_, e) => {
            state.SelectedPerson = e.CurrentSelection.FirstOrDefault () as Person;
            Show ();
        };

        var add = Primary ("Add person", "add-person");
        add.Clicked += (_, _) => {
            var person = NextPerson (state);
            state.People.Add (person);
            state.SelectedPerson = person;
            list.SelectedItem = person;
            Show ($"Added {person.Name} · {state.People.Count} people in the list.");
        };

        remove.Clicked += (_, _) => {
            if (state.SelectedPerson is not { } person)
                return;
            list.SelectedItem = null;
            state.SelectedPerson = null;
            state.People.Remove (person);
            Show ($"Removed {person.Name} · {state.People.Count} people left.");
        };

        return Page (
            Heading ("CollectionView"),
            Lead ("An observable ItemsSource renders one element per item. Clicking a row selects it and reports back to C#."),
            Card ("Team",
                "The list is a MAUI CollectionView. Its items, the template and the selection all live in the session of this browser.",
                list,
                new HorizontalStackLayout { Spacing = 12, Children = { add, remove } },
                status),
            Card ("What happens under the hood",
                "Selecting a row sends one message to the server, the handler flips aria-selected and calls SelectionChanged in C#.",
                Small ("Adding or removing an item raises CollectionChanged and only the containers are re-rendered — no page reload, no virtualisation. Grouping, headers and footers are still missing.")));
    }

    public static ContentPage Controls ()
    {
        var search = new SearchBar { Placeholder = "Type to see the server react…", AutomationId = "search" };
        var echo = Small ("The search text is echoed from C#.");
        echo.AutomationId = "search-echo";
        search.TextChanged += (_, e) => echo.Text = string.IsNullOrEmpty (e.NewTextValue)
            ? "The search text is echoed from C#."
            : $"Server received: “{e.NewTextValue}”";

        var stepper = new Stepper { Minimum = 0, Maximum = 10, Value = 3, Increment = 1, AutomationId = "stepper" };
        var stepValue = new Label { Text = "3 seats", FontSize = 18, FontAttributes = FontAttributes.Bold, TextColor = Ink, AutomationId = "stepper-value" };
        stepper.ValueChanged += (_, e) => stepValue.Text = e.NewValue == 1 ? "1 seat" : $"{e.NewValue:0} seats";

        return Page (
            Heading ("Controls"),
            Lead ("A quick tour of what this backend renders today."),
            Card ("Input and feedback",
                "SearchBar, Stepper and ActivityIndicator.",
                search, echo, stepValue, stepper,
                new HorizontalStackLayout {
                    Spacing = 14,
                    Children = {
                        new ActivityIndicator { IsRunning = true, Color = Accent, HeightRequest = 28, AutomationId = "busy" },
                        Body ("An ActivityIndicator, animating in the browser"),
                    },
                }),
            Card ("Layout and decoration",
                "Border, BoxView, Image and ProgressBar.",
                new BoxView { Color = Accent, HeightRequest = 4, CornerRadius = 2 },
                new ProgressBar { Progress = .45, ProgressColor = Accent, HeightRequest = 10 },
                new HorizontalStackLayout {
                    Spacing = 16,
                    Children = {
                        new Image { Source = "/logo.svg", Aspect = Aspect.AspectFit, HeightRequest = 56, AutomationId = "logo" },
                        new VerticalStackLayout {
                            Spacing = 4,
                            Children = {
                                new Label { Text = "This logo is an SVG served by the same ASP.NET app.", FontSize = 16, TextColor = Ink },
                                Small ("Fill, spacing, corner radius and object-fit all come from MAUI properties."),
                            },
                        },
                    },
                }),
            Card ("Not supported yet",
                "Keep these out of a production plan for now.",
                Small ("NavigationPage, Shell and TabbedPage, ListView, WebView, graphics, gestures and animations.")));
    }

    static View PersonTemplate ()
    {
        var initials = new Label { FontSize = 16, FontAttributes = FontAttributes.Bold, TextColor = Colors.White, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
        initials.SetBinding (Label.TextProperty, nameof (Person.Initials));

        var avatar = new Border {
            WidthRequest = 44,
            HeightRequest = 44,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 22 },
            VerticalOptions = LayoutOptions.Center,
            Content = initials,
        };
        avatar.SetBinding (VisualElement.BackgroundColorProperty, nameof (Person.Accent));

        var name = new Label { FontSize = 17, FontAttributes = FontAttributes.Bold, TextColor = Ink };
        name.SetBinding (Label.TextProperty, nameof (Person.Name));

        var role = new Label { FontSize = 14, TextColor = Muted };
        role.SetBinding (Label.TextProperty, nameof (Person.Role));

        return new Border {
            BackgroundColor = Colors.White,
            Stroke = new SolidColorBrush (Line),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            Padding = new Thickness (14, 12),
            Content = new HorizontalStackLayout {
                Spacing = 14,
                Children = {
                    avatar,
                    new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center, Children = { name, role } },
                },
            },
        };
    }

    static readonly (string Name, string Role)[] Candidates = {
        ("Katherine Johnson", "Research"),
        ("Margaret Hamilton", "Flight software"),
        ("Hedy Lamarr", "Signals"),
        ("Barbara Liskov", "Architecture"),
        ("Radia Perlman", "Networking"),
    };

    static readonly Color[] Palette = {
        Color.FromArgb ("#3f7fd6"),
        Color.FromArgb ("#8a4fb8"),
        Color.FromArgb ("#2f8f7a"),
        Color.FromArgb ("#c06a2a"),
    };

    static Person NextPerson (SiteState state)
    {
        var (name, role) = Candidates[state.People.Count % Candidates.Length];
        var initials = string.Concat (name.Split (' ').Select (part => char.ToUpperInvariant (part[0])));
        return new Person (name, role, initials, Palette[state.People.Count % Palette.Length]);
    }

    static ContentPage Page (params View[] content)
    {
        var stack = new VerticalStackLayout { Spacing = 20 };
        foreach (var view in content)
            stack.Children.Add (view);
        return new ContentPage { Content = stack };
    }

    static View Cell (View caption, View editor, int column)
    {
        var stack = new VerticalStackLayout {
            Spacing = 6,
            Children = { caption, editor },
        };
        Grid.SetColumn (stack, column);
        return stack;
    }

    static Border Card (string heading, string description, params View[] content)
    {
        var stack = new VerticalStackLayout {
            Spacing = 12,
            Children = {
                new Label { Text = heading, FontSize = 23, FontAttributes = FontAttributes.Bold, TextColor = Ink },
                Small (description),
            },
        };
        foreach (var view in content)
            stack.Children.Add (view);
        return new Border {
            BackgroundColor = Colors.White,
            Stroke = new SolidColorBrush (Line),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 18 },
            Padding = new Thickness (24),
            Content = stack,
        };
    }

    static Label Heading (string text) => new () { Text = text, FontSize = 34, FontAttributes = FontAttributes.Bold, TextColor = Ink };
    static Label Lead (string text) => new () { Text = text, FontSize = 18, TextColor = Muted, LineHeight = 1.4 };
    static Label Body (string text) => new () { Text = text, FontSize = 16, TextColor = Ink };
    static Label Small (string text) => new () { Text = text, FontSize = 14, TextColor = Muted, LineHeight = 1.35 };
    static Label Caption (string text) => new () { Text = text, FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = Muted, CharacterSpacing = 1 };
    static Button Primary (string text, string automationId) => new () { Text = text, TextColor = Colors.White, BackgroundColor = Accent, CornerRadius = 10, FontAttributes = FontAttributes.Bold, AutomationId = automationId };
    static Button Secondary (string text, string automationId) => new () { Text = text, TextColor = Ink, BackgroundColor = Color.FromArgb ("#eef0f7"), CornerRadius = 10, AutomationId = automationId };
}
