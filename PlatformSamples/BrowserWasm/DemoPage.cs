using Ooui;

namespace BrowserWasm;

/// <summary>
/// The page of the sample. It is written in plain Ooui elements because MAUI controls need a
/// handler that produces real visuals, and in the browser the DOM is the visual layer.
/// </summary>
static class DemoPage
{
    public static Element Create () => new Div (
        Header (),
        Counter (),
        Greeting (),
        Features (),
        Footer ()) {
        ClassName = "page",
    };

    static Element Header () => new Div (
        new Paragraph ("OOUI  /  WEBASSEMBLY  /  .NET 11") { ClassName = "eyebrow" },
        new Heading (1, "Your UI runs in the tab, not on a server.") { ClassName = "title" },
        new Paragraph ("No WebSocket and no HTTP listener. The controls are C# objects, the DOM is updated by messages, and the runtime is dotnet.native.wasm.") { ClassName = "lead" }) {
        ClassName = "hero",
    };

    static Element Counter ()
    {
        var count = 0;
        var value = new Span ("0") { ClassName = "count" };
        value.SetAttribute ("data-automation-id", "count");

        void Set (int next)
        {
            count = next;
            value.Text = count.ToString ();
        }

        var down = new Button ("-") { ClassName = "button ghost", Type = ButtonType.Button };
        var up = new Button ("+") { ClassName = "button primary", Type = ButtonType.Button };
        down.SetAttribute ("data-automation-id", "decrement");
        up.SetAttribute ("data-automation-id", "increment");
        down.Click += (s, e) => Set (count - 1);
        up.Click += (s, e) => Set (count + 1);

        return Card (
            "Events round trip through JavaScript",
            "Every click becomes a message that .NET receives. Only the changed text travels back to the page.",
            new Div (down, value, up) { ClassName = "row" });
    }

    static Element Greeting ()
    {
        var name = new TextInput { ClassName = "input", Placeholder = "Type your name" };
        var greeting = new Paragraph ("Hello, stranger.") { ClassName = "greeting" };
        name.SetAttribute ("data-automation-id", "name");
        greeting.SetAttribute ("data-automation-id", "greeting");

        name.Input += (s, e) => {
            var typed = name.Value.Trim ();
            greeting.Text = typed.Length == 0 ? "Hello, stranger." : $"Hello, {typed}.";
        };

        return Card (
            "State lives in your C# objects",
            "What you type is pushed into the session, the label is updated in place, and the state survives because the object graph belongs to this page.",
            name,
            greeting);
    }

    static Element Features ()
    {
        var list = new List { ClassName = "list" };
        list.AppendChild (new ListItem ("Messages are serialized by the existing Session code, unchanged."));
        list.AppendChild (new ListItem ("The page only has to forward events and apply DOM patches."));
        list.AppendChild (new ListItem ("Same elements, same handlers, same CSS - different transport."));

        return Card (
            "What this sample proves",
            "The WebAssembly path is a thin bridge, so the rest of the library keeps working.",
            list);
    }

    static Element Footer ()
    {
        var footer = new Paragraph { ClassName = "footer" };
        footer.AppendChild (new Span ("Built with Ooui on .NET 11. Reload the page to reset the state."));
        return footer;
    }

    static Div Card (string title, string text, params Node[] body)
    {
        var card = new Div (
            new Heading (2, title) { ClassName = "card-title" },
            new Paragraph (text) { ClassName = "card-text" }) {
            ClassName = "card",
        };
        foreach (var node in body) {
            card.AppendChild (node);
        }
        return card;
    }
}
