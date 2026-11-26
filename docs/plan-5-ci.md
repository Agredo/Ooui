# Plan 5: CI workflow (build, test, browser checks)

Read `docs\navigation-plan.md` section 0 (ground rules) first. Differences for this plan: the work is mostly YAML and
scripts, and you cannot run GitHub Actions locally. Validate scripts locally, validate YAML by reading it carefully,
and tell the user that the first real run happens on GitHub after the push.

Goal: every push and pull request on `master` and `maui-11-port` builds the solution warning-free, runs all unit tests and
runs the Playwright browser checks. Today `.github\workflows\build.yml` is the old upstream workflow: .NET 5,
`ubuntu-latest`, only `master`, submodules, NuGet packaging. It cannot build this fork.

## Step 1 – Find out what the build needs

1. `dotnet --list-sdks` and read `global.json` (none exists today) and the `TargetFramework` of each project:
   `Get-ChildItem -Recurse -Filter *.csproj | Select-String TargetFramework`.
2. Which projects need workloads? The wasm projects (`BrowserWasm`, `BrowserWasmMaui`, `AspNetCoreMauiWasm`) probably need
   `wasm-tools`. Check by reading their csproj (`WasmBuildNative`, `RuntimeIdentifier` `browser-wasm`) and by running a
   clean build locally: `dotnet build Ooui.sln -c Release --nologo` and note any workload hint in the output.
3. Does `Ooui.sln` contain old projects that fail on Linux (Forms, Xamarin)? Run `dotnet sln Ooui.sln list`.
4. Is the .NET 11 SDK a preview? `dotnet --version`. In `actions/setup-dotnet@v4` use
   `dotnet-version: 11.0.x` and, if it is still a preview, `dotnet-quality: preview`.
5. Write all findings at the end of this file.

## Step 2 – Make the browser checks part of the repo

The Playwright scripts currently live outside the repo in
`C:\Users\chris\.copilot\session-state\13218a70-2e74-4e2f-937a-91a06ac7cb74\files\`
(`verify_pages.py`, `verify_mvvm.py`, `verify_console.py`, `verify_asp_maui_wasm.py`).

1. Create `e2e\` in the repo root and COPY those four scripts into it (do not move the originals).
2. Replace hard-coded `http://localhost:809x` with `os.environ.get("BASE_URL", "http://localhost:809x")`.
3. Remove references to `C:\Users\...` paths (screenshots etc.): write screenshots to `e2e\out\` (add `e2e/out/` to
   `.gitignore`). Remove Windows-only things (`PYTHONIOENCODING` handling is fine to keep).
4. Add `e2e\requirements.txt` with `playwright`.
5. Add `e2e\run.py`: starts a sample host with `subprocess.Popen`, waits until the URL answers (poll up to 60 s), runs the
   matching `verify_*.py` with `subprocess.run`, always kills the host in `finally`, returns the exit code. Usage:
   `python e2e\run.py pages|mvvm|console|wasm`. Host commands (after a Release build):
   - pages/mvvm: `dotnet PlatformSamples/AspNetCoreMaui/bin/Release/net11.0/AspNetCoreMaui.dll` with env
     `ASPNETCORE_URLS=http://localhost:8090`
   - console: `dotnet PlatformSamples/ConsoleNet11/bin/Release/net11.0/ConsoleNet11.dll` (port 8080)
   - wasm: only if the wasm publish output exists (it needs `dotnet publish`, see Step 3). Check
     `PlatformSamples\AspNetCoreMauiWasm` and the log line "Serving the MAUI WebAssembly bundle from ..." for the path.
   Use `pathlib` and forward slashes so it runs on Windows and Linux.
6. Run `python e2e\run.py pages` and `console` locally. Both must pass before continuing.

## Step 3 – Write the workflow

Create `.github\workflows\ci.yml` (do NOT delete or edit `build.yml`; the user decides about it, see Step 5).

```yaml
name: CI
on:
  push:
    branches: [ master, maui-11-port ]
  pull_request:
    branches: [ master, maui-11-port ]
concurrency:
  group: ci-${{ github.ref }}
  cancel-in-progress: true
jobs:
  build-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 11.0.x
          dotnet-quality: preview   # remove once 11 is GA
      # - run: dotnet workload install wasm-tools   # only if Step 1 says it is needed
      - run: dotnet build Ooui.sln -c Release --nologo -warnaserror
      - run: dotnet test Ooui.sln -c Release --no-build --nologo
  e2e:
    needs: build-test
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with: { dotnet-version: 11.0.x, dotnet-quality: preview }
      - uses: actions/setup-python@v5
        with: { python-version: '3.12' }
      - run: dotnet build Ooui.sln -c Release --nologo
      - run: pip install -r e2e/requirements.txt && python -m playwright install --with-deps chromium
      - run: python e2e/run.py pages
      - run: python e2e/run.py mvvm
      - run: python e2e/run.py console
      - uses: actions/upload-artifact@v4
        if: always()
        with: { name: e2e-output, path: e2e/out }
```

Adjust to the Step 1 findings (workloads, projects that must be excluded, SDK version). Rules:
- Pin nothing exotic; use only the official `actions/*` actions.
- `-warnaserror`: if it fails on third-party analyzers, remove it from the command and instead add
  `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` ONLY to `Ooui.Maui\Ooui.Maui.csproj` and the Maui test project,
  after checking they build with 0 warnings locally.
- The `Tests` project binds port 8080. It runs in `build-test` before any e2e host starts, so there is no clash.
- Wasm e2e (`run.py wasm`) is optional: add it as a separate job `e2e-wasm` with `continue-on-error: true` only if
  the publish works on Linux in Step 1; otherwise leave it out and note it as open.

## Step 4 – Validate locally

1. YAML syntax: `python -c "import yaml,sys; yaml.safe_load(open('.github/workflows/ci.yml')); print('ok')"`
   (`pip install pyyaml` if needed).
2. Simulate the commands in a clean state: `git status --short` must be clean of build output (check `.gitignore` covers `bin/`, `obj/`, `e2e/out/`),
   then `dotnet build Ooui.sln -c Release --nologo -warnaserror` and `dotnet test Ooui.sln -c Release --no-build --nologo`
   (port 8080 free), then `python e2e\run.py pages|mvvm|console`.
3. Optional if `act` is installed: skip, do not install tools.

## Step 5 – Decisions to ask the user (use the ask_user tool, one question at a time)

1. `build.yml` is the old upstream packaging workflow with .NET 5. Keep, delete, or rename to `package.yml.disabled`?
   Default recommendation: keep it untouched and mention that it will fail on `maui-11-port` (it only triggers on `master`).
2. Should the e2e job be required for pull requests or informational only?

## Step 6 – Docs

README: add a short "Continuous integration" section: what runs, how to run the e2e checks locally
(`python e2e\run.py pages`), and the Playwright install command.

## Out of scope

Publishing NuGet packages, release workflows, code coverage, matrix builds on Windows/macOS.

## Definition of done

`ci.yml` exists and parses, e2e scripts run locally through `run.py`, README section added, nothing committed or pushed
without user approval. Tell the user that GitHub has to run it once to confirm. Final report in German, short.

## Findings (fill in during Step 1)

_(empty)_
