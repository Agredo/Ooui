using System.Collections.ObjectModel;
using Microsoft.Maui.Graphics;

namespace AspNetCoreMaui;

/// <summary>
/// Everything the demo keeps for one browser. It lives in the Ooui user session, so it survives
/// navigation, reloads and new page instances.
/// </summary>
public sealed class SiteState
{
    public const string Key = "site";

    public int Count { get; set; }
    public string CounterNote { get; set; } = "No clicks yet.";

    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Notes { get; set; } = "";
    public string FormNote { get; set; } = "Nothing saved yet.";

    // The people page renders this collection in a CollectionView. It is observable, so adding or
    // removing a person updates the rendered list over the open WebSocket instead of a reload.
    public ObservableCollection<Person> People { get; } = new (InitialPeople ());
    public Person? SelectedPerson { get; set; }
    public string ListNote { get; set; } = "Nobody selected yet.";

    // The grouped CollectionView on the same page renders one block per team. The teams are observable
    // too, so adding a member reaches the browser over the open WebSocket.
    public ObservableCollection<PersonTeam> Teams { get; } = new (InitialTeams ());
    public int MemberAdds { get; set; }

    public int Pages { get; set; }

    static IEnumerable<Person> InitialPeople () => new[] {
        new Person ("Ada Lovelace", "Engineering", "AL", Color.FromArgb ("#6854d9")),
        new Person ("Grace Hopper", "Platform", "GH", Color.FromArgb ("#31846a")),
        new Person ("Alan Turing", "Research", "AT", Color.FromArgb ("#b4661f")),
    };

    static IEnumerable<PersonTeam> InitialTeams () => new[] {
        new PersonTeam ("Engineering",
            new Person ("Ada Lovelace", "Engineering", "AL", Color.FromArgb ("#6854d9")),
            new Person ("Grace Hopper", "Platform", "GH", Color.FromArgb ("#31846a"))),
        new PersonTeam ("Research",
            new Person ("Alan Turing", "Research", "AT", Color.FromArgb ("#b4661f")),
            new Person ("Katherine Johnson", "Research", "KJ", Color.FromArgb ("#8a4fb8"))),
    };
}

/// <summary>One group of the grouped demo list: the team name is the group header, the members are the items.</summary>
public sealed class PersonTeam (string title, params Person[] members) : ObservableCollection<Person> (members)
{
    public string Title { get; } = title;
}

/// <summary>One row of the demo list: plain data, no view state.</summary>
public sealed class Person (string name, string role, string initials, Color accent)
{
    public string Name { get; } = name;
    public string Role { get; } = role;
    public string Initials { get; } = initials;
    public Color Accent { get; } = accent;
}
