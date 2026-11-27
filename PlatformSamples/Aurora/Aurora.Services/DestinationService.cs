using Aurora.Models;

namespace Aurora.Services;

public sealed class DestinationService : IDestinationService
{
    readonly IReadOnlyList<Destination> destinations;

    public DestinationService()
    {
        destinations =
        [
            new("iceland", "Iceland", "Reykjavík & South Coast", "Europe", "A little closer to another planet",
                "Black sand, glacier lagoons and roads that disappear into the horizon.",
                "There is a moment on the south coast when the noise of everyday life simply stops. Waterfalls become your landmarks, the weather becomes your itinerary, and every turn feels like a small discovery. This slow road trip pairs the iconic places with room to get wonderfully lost.",
                "images/iceland.webp", "Jun – Sep", "8 days", "Slow adventure", 2480, 4.9,
                ["Nature", "Road trips", "Wild"],
                [new("Glacier lagoon", "A quiet morning among drifting blue ice", "◈"),
                 new("Geothermal bathing", "Warm water, cold air, nowhere to rush", "≈"),
                 new("South coast drive", "Waterfalls and black-sand horizons", "↗")],
                [new("Arrive into the elements", "Reykjavík, a local table and your first glimpse of the North Atlantic.", "images/iceland.webp"),
                 new("Follow the water", "Walk behind Seljalandsfoss before the coast opens up towards Vík.", "images/iceland.webp"),
                 new("A day of ice", "An unhurried morning at Jökulsárlón and the diamond-strewn shore.", "images/iceland.webp")]),
            new("kyoto", "Kyoto", "Japan", "Asia", "A thousand quiet moments",
                "Lantern-lit lanes, hidden gardens and the gentle rhythm of old Japan.",
                "Kyoto rewards a different pace. Rise before the temple gates open, follow the smell of freshly roasted tea and leave the map behind in the backstreets of Gion. Between refined ryokan hospitality and tiny neighbourhood counters, this is a journey designed around the details.",
                "images/kyoto.webp", "Mar – May", "6 days", "Culture & food", 1890, 4.8,
                ["Culture", "Food", "City"],
                [new("Private tea ceremony", "Learn the beauty of a deliberate ritual", "❋"),
                 new("Dawn at Fushimi Inari", "The vermilion gates before the city wakes", "↗"),
                 new("A taste of Nishiki", "Follow a local guide through Kyoto's pantry", "◌")],
                [new("The old capital", "Settle into a machiya and explore lantern-lit Gion at dusk.", "images/kyoto.webp"),
                 new("Temples before breakfast", "A peaceful walk through Fushimi Inari, followed by a tea ritual.", "images/kyoto.webp"),
                 new("Arashiyama slowly", "Bamboo, riverside gardens and a final seasonal tasting menu.", "images/kyoto.webp")]),
            new("madeira", "Madeira", "Portugal", "Europe", "The island above the clouds",
                "Clifftop paths, forest trails and Atlantic views that go on forever.",
                "Madeira is an island made for looking up – and looking down. Walk a levada through laurisilva forest, swim in volcanic pools and take the long way along the coast. This week balances iconic trails with the kind of unplanned lunches that become your favourite memories.",
                "images/madeira.webp", "All year", "7 days", "Walking & nature", 1640, 4.9,
                ["Nature", "Hiking", "Ocean"],
                [new("Sunrise at Pico do Arieiro", "Watch the island emerge above a sea of cloud", "△"),
                 new("Levada wandering", "A privately guided walk into ancient forest", "↗"),
                 new("Volcanic pools", "A slow Atlantic afternoon in Porto Moniz", "≈")],
                [new("Funchal at your feet", "A hilltop stay, botanical gardens and dinner by the water.", "images/madeira.webp"),
                 new("Above the clouds", "An early start for a ridge walk, followed by a slow island lunch.", "images/madeira.webp"),
                 new("The wild north", "Natural pools and a coastal road made for stopping often.", "images/madeira.webp")]),
            new("lofoten", "Lofoten", "Norway", "Europe", "Where mountains meet the sea",
                "Red cabins, Arctic light and the wild beauty of Norway's northern islands.",
                "At the edge of the Arctic Circle, fishing villages gather beneath impossible mountains. Your cabin is a base for sea safaris, empty beaches and evenings watching the light change. Come for the photographs; stay for the profound sense of space.",
                "images/lofoten.webp", "May – Sep", "5 days", "Remote escape", 2190, 4.8,
                ["Nature", "Wild", "Ocean"],
                [new("Midnight sea safari", "Quiet fjords under a sun that barely sets", "☼"),
                 new("A fisherman’s cabin", "Your own warm corner by the Arctic water", "⌂"),
                 new("Empty Arctic beaches", "Turquoise water without the crowds", "≈")],
                [new("Life by the fjord", "Settle into a restored rorbu and share a northern seafood supper.", "images/lofoten.webp"),
                 new("Out on the water", "A small-boat safari and a picnic on an island beach.", "images/lofoten.webp"),
                 new("Follow the Arctic light", "A flexible day of fishing villages, peaks and photography.", "images/lofoten.webp")]),
            new("alps", "The Dolomites", "Italy", "Europe", "Big peaks, little pleasures",
                "Sunrise ridgelines, alpine meadows and long lunches in the mountains.",
                "The best days here begin above the valley and end around a table. Walk between welcoming rifugi, pause in a wildflower meadow and let a local guide choose the trail that fits the morning. Alpine air, Italian warmth, and not a single hurried breakfast.",
                "images/alps.webp", "Jun – Oct", "6 days", "Active escape", 1760, 4.7,
                ["Hiking", "Food", "Nature"],
                [new("Rifugio mornings", "Fresh coffee where the peaks feel close enough to touch", "△"),
                 new("Alpe di Siusi", "Gentle trails through Europe's largest alpine meadow", "↗"),
                 new("South Tyrolean table", "Mountain cooking with an Italian soul", "◌")],
                [new("Into the mountains", "A panoramic transfer and an easy sunset walk from your rifugio.", "images/alps.webp"),
                 new("The high meadows", "A day of flexible trails, local cheese and wide-open views.", "images/alps.webp"),
                 new("A little higher", "A guided ridge walk with a leisurely mountain-lunch finale.", "images/alps.webp")]),
            new("forest", "The Black Forest", "Germany", "Europe", "A softer kind of wild",
                "Deep green valleys, forest spas and quiet trails made for reconnecting.",
                "Sometimes the greatest luxury is fewer decisions. Wake among the pines, take a thermal soak before lunch and follow a trail without checking the time. A restorative short escape rooted in thoughtful small hotels and the healing quiet of the forest.",
                "images/forest.webp", "All year", "4 days", "Rest & reset", 980, 4.7,
                ["Wellness", "Nature", "Slow"],
                [new("Forest bathing", "A guided practice in paying attention again", "♧"),
                 new("Thermal stillness", "An afternoon of warm pools and quiet rooms", "≈"),
                 new("The local table", "Seasonal cooking from just beyond the forest", "◌")],
                [new("Switch off", "Check into a forest hideaway and leave the afternoon gloriously empty.", "images/forest.webp"),
                 new("Breathe a little deeper", "A slow guided forest walk followed by a thermal spa.", "images/forest.webp"),
                 new("A taste of the region", "Village bakeries, a local winery and one last woodland trail.", "images/forest.webp")]),
        ];
    }

    public IReadOnlyList<Destination> GetAll() => destinations;

    public Destination? Find(string? id) => destinations.FirstOrDefault(destination => destination.Id == id);
}
