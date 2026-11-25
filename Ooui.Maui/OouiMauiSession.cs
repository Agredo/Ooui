using Microsoft.Maui.Hosting;

namespace Ooui.Maui;

/// <summary>Owns the MAUI view hierarchy associated with one Ooui connection.</summary>
public sealed class OouiMauiSession : Div, IDisposable
{
    readonly IWindow window;
    bool disposed;

    public OouiMauiSession(IApplication application, IMauiContext context)
    {
        ArgumentNullException.ThrowIfNull(application);
        ArgumentNullException.ThrowIfNull(context);
        window = application.CreateWindow(new ActivationState(context));
        var content = window.Content?.ToOouiElement(context)
            ?? throw new InvalidOperationException("The MAUI window has no content.");
        AppendChild(content);
        window.Created();
        window.Activated();
    }

    public Element Content => this;

    public void Dispose()
    {
        if (disposed)
            return;
        disposed = true;
        try
        {
            window.Deactivated();
            window.Destroying();
        }
        finally
        {
            window.Content?.DisconnectHandlers();
            RemoveChild(Children[0]);
        }
    }
}
