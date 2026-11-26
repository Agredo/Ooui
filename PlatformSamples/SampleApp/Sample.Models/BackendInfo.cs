namespace Sample.Models;

/// <summary>
/// One rendering backend of the sample app. The view models display these cards, so the app can
/// describe its own hosting without knowing anything about Ooui.
/// </summary>
/// <param name="Kind">Which backend this is.</param>
/// <param name="Name">Short name shown in the page header.</param>
/// <param name="Summary">One sentence about how this backend renders the app.</param>
/// <param name="Project">The host project that provides this backend.</param>
public sealed record BackendInfo (BackendKind Kind, string Name, string Summary, string Project);
