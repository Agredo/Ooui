using Microsoft.Extensions.Configuration;

namespace Sample.Ooui.AspNetCoreWasm;

/// <summary>
/// Finds the published <c>Sample.Ooui.Wasm</c> bundle that this host serves under <c>/wasm</c>.
/// The bundle is a directory of static files, so no build of this project can produce it: it needs a
/// second publish with a runtime identifier.
/// </summary>
static class WasmBundle
{
    public static string? Find (string contentRoot, IConfiguration configuration) =>
        Candidates (contentRoot, configuration).FirstOrDefault (IsBundle);

    static IEnumerable<string> Candidates (string contentRoot, IConfiguration configuration)
    {
        var configured = configuration["Ooui:WasmBundle"] ?? Environment.GetEnvironmentVariable ("OOUI_WASM_BUNDLE");
        if (!string.IsNullOrWhiteSpace (configured))
            yield return Path.GetFullPath (configured);

        // `dotnet run --project` uses the caller's directory as content root while a published host
        // runs from its own folder, so both the content root and the assembly folder are searched,
        // together with their parents.
        foreach (var start in new[] { contentRoot, AppContext.BaseDirectory })
        {
            if (string.IsNullOrWhiteSpace (start))
                continue;

            var directory = new DirectoryInfo (Path.GetFullPath (start));
            for (var depth = 0; directory is not null && depth < 6; depth++, directory = directory.Parent)
            {
                // A copy of the bundle inside the host, and the same folder next to a published host.
                yield return Path.Combine (directory.FullName, "wwwroot", "wasm");
                yield return Path.Combine (directory.FullName, "wasm");

                // The publish output of the WebAssembly host next door, newest configuration first.
                foreach (var configurationName in new[] { "Release", "Debug" })
                    yield return Path.Combine (directory.FullName, "PlatformSamples", "SampleApp", "Sample.Ooui.Wasm", "bin", configurationName, "net11.0", "browser-wasm", "publish");
            }
        }
    }

    static bool IsBundle (string directory) =>
        File.Exists (Path.Combine (directory, "index.html")) && File.Exists (Path.Combine (directory, "wasm-boot.json"));
}
