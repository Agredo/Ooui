using Ooui.AspNetCore;
using Sample.Services;

namespace Sample.Ooui.AspNetCoreWasm;

/// <summary>
/// Keeps the state of the app in the Ooui session of one browser, so the counter of a page reaches
/// the next request instead of the process-wide store the app uses by default.
/// </summary>
public sealed class UserSessionStateStore (OouiUserSession session) : IStateStore
{
    public T GetOrAdd<T> (string key, Func<T> factory) where T : class =>
        session.GetOrAdd (key, factory);

    public void Remove (string key) => session.Remove (key);
}
