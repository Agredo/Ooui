# Plan 6: Reconnect and session robustness

Read `docs\navigation-plan.md` section 0 (ground rules) and section 1 first. They apply here too.

Goal: a lost WebSocket (Wi-Fi blip, server restart, laptop sleep) does not leave a dead page, abandoned sessions free
their memory, and one misbehaving handler cannot take down the host.

Background (verified in code):
- `Ooui\Client.js` `connectWebSocket()` opens `ws://host/ooui.ws?id=<sessionId>&w=..&h=..`; on `close` after a successful
  open it calls `reloadSocket()`, which clears `<body>` and connects again with the same `rootElementPath`.
- `Ooui.AspNetCore\WebSocketHandler.cs` keeps `pendingSessions` keyed by the 32-char id from `BeginSession`, with
  `SessionTimeout` (1 minute). A pending session is created when the HTML page is rendered, and consumed by the WebSocket.
- `Ooui.Maui\OouiMauiSession.cs` has `Dispose()` (deactivates the MAUI window and disconnects handlers).
- Per-browser user state lives in `Ooui.AspNetCore\OouiUserSession*.cs` (cookie based, in memory) and survives
  a page reload. The AspNetCoreMaui sample is designed so that a reload loses nothing important.

Work in this order. Steps 1 and 2 are the main value; Step 5 is optional and needs a go from the user.

## Step 1 – Characterize the current behavior (no production code)

Write a Playwright script `reconnect_probe.py` (in the session `files` folder, not in the repo) against AspNetCoreMaui (port 8090):

1. Open `/counter`, click the counter button twice (note the value).
2. Kill the socket: `page.evaluate("socket.close()")` (the variable `socket` is global in `Client.js`), wait 3 s.
3. Observe and record: console messages, network requests (`page.on("request")`), whether the page is blank, whether clicking
   still works, whether the counter value survived.
4. Repeat with the host stopped and restarted (stop by PID, start again, wait 10 s).
5. Check whether `OouiMauiSession.Dispose` runs on disconnect: add a temporary `Console.WriteLine` or use a debugger-free
   check by looking at host log output. Remove temporary code afterwards.
6. Write the observations into "Findings" at the end of this file (what happens, what is broken). Do not fix yet.

## Step 2 – Client side: visible status and a controlled retry

Change `Ooui\Client.js` (embedded resource, rebuild needed; every sample host serves it from the `Ooui` assembly, so
rebuild the host before testing).

1. Add a small overlay element (`#ooui-status`, fixed, top center, small pill, high z-index, hidden by default) created lazily.
   Text: "Reconnecting…" while retrying, "Connection lost. Reload the page." after giving up. Style it inline in JS.
   It must not appear for normal operation and must not break pages that use `#ooui-body`.
2. In the `socket` `close` handler (only when the socket had opened):
   - show the overlay, retry with backoff 0.5 s, 1 s, 2 s, 4 s, 8 s (max 5 tries);
   - **retry strategy**: the session id in the URL is single-use on the server. A plain re-connect with the same id fails,
     so after the first failed retry do `location.reload()` ONLY if the first re-connect failed. If Step 1 shows that the
     server ignores the reused id and starts a new session, keep the existing behavior and only add the overlay.
     Decide from the Step 1 findings and write the decision in "Findings".
   - after the last failed try keep the final message and stop retrying.
3. Hide the overlay on a successful `open`.
4. Add a `ping` every 20 s from the client only if Step 1 shows proxies drop idle connections (skip otherwise).
5. Verify with Playwright (extend `reconnect_probe.py` into assertions): after `socket.close()` the page recovers
   (overlay disappears, button clicks work again). After a host restart the page recovers by reload within 15 s.

Because Client.js is also used by the wasm hosts (`wasmSend`), guard everything with `socket != null` and make sure
`oouiWasm` mode never shows the overlay. Re-run `verify_asp_maui_wasm.py`.

## Step 3 – Server side cleanup of abandoned sessions

1. Confirm in Step 1 that `Dispose` runs when the browser tab closes. If `OouiMauiSession.Dispose` is not called by
   `WebSocketSession`/`WebSocketHandler` on close (check `Ooui\WebSocketSession.cs` for a `disposeElementWhenDone` /
   `Dispose` call and `Ooui.AspNetCore\WebSocketHandler.cs` `DisposeElementAfterSession`), fix it so that
   an element implementing `IDisposable` is disposed after the socket closes.
2. Pending sessions that never connect (bots fetching the HTML only) must be dropped after `SessionTimeout`.
   Check that a cleanup actually runs (timer or on next `BeginSession`). If not, add a cleanup pass inside `BeginSession`
   that removes entries older than `SessionTimeout` and disposes their elements.
3. Add a limit: `WebSocketHandler.MaxPendingSessions` (default 1000). If exceeded, `BeginSession` removes the oldest.
4. Tests (in `Ooui.Maui.Tests`, copy the style of `WebSocketSessionTests.cs`):
   - `PendingSessionsExpire`: create sessions with a very small `SessionTimeout`, wait, assert they were disposed.
   - `ElementIsDisposedWhenSocketCloses` (use the existing fake `WebSocket` in that test file).

## Step 4 – Fault isolation

An exception inside a handler event (for example a user's `Clicked` handler) must not close the socket.

1. Find where incoming events are dispatched (`Ooui\Session.cs`, `Ooui\WebSocketSession.cs`,
   `Element.TriggerEventFromMessage`). Check what happens on an exception (try it with a button whose handler throws).
2. Wrap the dispatch of ONE message in try/catch: log through the existing logger (`Logger` field in `PendingSession`) or
   `Console.Error` if none, continue with the next message. Do not swallow `OperationCanceledException`.
3. Test: a handler that throws on the first click; second click still works. Keep it to one test.

## Step 5 – OPTIONAL: real session resume (ask the user first)

Resume means the page reconnects to the SAME server-side element tree instead of getting a new one. This needs the server
to replay the whole DOM to a new socket. Investigate only:
1. Does `Ooui\Session.cs` keep a message history or can an element serialize its complete current state
   (`Element.SaveState`/`StateMessages`)? Search for `StateMessages`, `Session`, `history`.
2. Write a short assessment in "Findings": feasible yes/no, estimated change size, risks.
3. Use the `ask_user` tool to ask whether to implement it. Do not start without a yes.

## Step 6 – Docs and verification

- README: replace the "reconnect behavior ... require additional application design" wording with what now exists
  (visible status, automatic recovery by reload, cleanup of abandoned sessions) and what is still open (no session resume).
- Full build (0 warnings), `dotnet test Ooui.sln` (port 8080 free), all `verify_*.py` scripts, plus the new reconnect assertions.

## Out of scope

Scale-out / sticky sessions, authentication, rate limiting beyond the pending-session cap, TLS configuration.

## Definition of done

Reconnect assertions pass in Playwright, new tests green (about 3), existing tests green, 0 warnings, README updated,
nothing committed without user approval. Final report to the user in German, short.

## Findings (fill in during Steps 1 and 2)

_(empty)_
