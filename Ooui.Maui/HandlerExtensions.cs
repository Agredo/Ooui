using Microsoft.Maui.Hosting;

namespace Ooui.Maui;

public static class HandlerExtensions
{
    /// <summary>Base URI used to resolve relative image sources in a plain WebAssembly host.</summary>
    public static string? DocumentBaseUri { get; set; }

    public static Element ToOouiElement(this IView view, IMauiContext context)
    {
        ArgumentNullException.ThrowIfNull(view);
        ArgumentNullException.ThrowIfNull(context);
        if (view is IReplaceableView replaceable)
            view = replaceable.ReplacedView;
        var handler = view.Handler ?? context.Handlers.GetHandler(view.GetType())
            ?? throw new InvalidOperationException($"No Ooui handler is registered for {view.GetType()}.");
        handler.SetMauiContext(context);
        handler.SetVirtualView(view);
        return handler.PlatformView as Element
            ?? throw new InvalidOperationException($"The handler for {view.GetType()} did not create an Ooui element.");
    }

    public static Element ToOouiElement(this IApplication application, IMauiContext context) =>
        new OouiMauiSession(application, context);

    public static void Publish(this MauiApp app, string path = "/")
    {
        ArgumentNullException.ThrowIfNull(app);
        var application = app.Services.GetService(typeof(IApplication)) as IApplication
            ?? throw new InvalidOperationException("Configure the application with UseMauiApp<TApp>() before publishing.");
        UI.Publish(path, _ =>
        {
            var context = new MauiContext(app.Services);
            return application.ToOouiElement(context);
        });
    }
}
