namespace Sample.Models;

/// <summary>The pages of the app, independent of how a host turns them into URLs.</summary>
public enum AppRoute
{
    Home,
    Details,
}

/// <summary>Maps routes to the paths the hosts serve.</summary>
public static class AppRoutes
{
    public static string Path (AppRoute route) => route switch {
        AppRoute.Details => "/details",
        _ => "/",
    };
}
