using Microsoft.Maui.Controls;

namespace AspNetCoreMaui.Pages;

public partial class PeoplePage : ContentPage
{
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

    readonly SiteState state;

    public PeoplePage (SiteState state)
    {
        this.state = state;
        InitializeComponent ();

        PeopleList.ItemsSource = state.People;
        PeopleList.SelectedItem = state.SelectedPerson;
        GroupedList.ItemsSource = state.Teams;
        Show ();
    }

    Color ThemeColor (string key) => Resources.TryGetValue (key, out var value) && value is Color color
        ? color
        : Colors.Gray;

    void OnSelectionChanged (object? sender, SelectionChangedEventArgs e)
    {
        state.SelectedPerson = e.CurrentSelection.FirstOrDefault () as Person;
        Show ();
    }

    void OnAddClicked (object? sender, EventArgs e)
    {
        var person = NextPerson (state);
        state.People.Add (person);
        state.SelectedPerson = person;
        PeopleList.SelectedItem = person;
        Show ($"Added {person.Name} · {state.People.Count} people in the list.");
    }

    void OnRemoveClicked (object? sender, EventArgs e)
    {
        if (state.SelectedPerson is not { } person)
            return;
        PeopleList.SelectedItem = null;
        state.SelectedPerson = null;
        state.People.Remove (person);
        Show ($"Removed {person.Name} · {state.People.Count} people left.");
    }

    void OnAddMemberClicked (object? sender, EventArgs e)
    {
        var team = state.Teams[state.MemberAdds % state.Teams.Count];
        var person = CreatePerson (state.People.Count + state.MemberAdds);
        state.MemberAdds++;
        team.Add (person);
        Show ($"Added {person.Name} to {team.Title} · {team.Count} members.");
    }

    void Show (string? message = null)
    {
        var person = state.SelectedPerson;
        state.ListNote = message ?? (person is null ? "Nobody selected yet." : $"{person.Name} · {person.Role}");
        StatusLabel.TextColor = person is null ? ThemeColor ("Muted") : ThemeColor ("Success");
        StatusLabel.Text = state.ListNote;
        RemoveButton.IsEnabled = person is not null;
    }

    static Person NextPerson (SiteState state) => CreatePerson (state.People.Count);

    static Person CreatePerson (int index)
    {
        var (name, role) = Candidates[index % Candidates.Length];
        var initials = string.Concat (name.Split (' ').Select (part => char.ToUpperInvariant (part[0])));
        return new Person (name, role, initials, Palette[index % Palette.Length]);
    }
}
