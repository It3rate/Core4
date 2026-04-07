# Core4

Core4 is an experimental numeric engine for representing structure, relation, and tension directly in number form.

The project is built around the idea that a system should not rely on symbolic case lists, enums, or hand-authored tags when a concept can instead be carried by a physically interpretable numeric structure. Core4 numbers are intended to stay inspectable while they are incomplete, under tension, branching, or partially frozen. The goal is not to "solve an equation" once and stop, but to let structure form, harden, relax, and lower tension dynamically.

## Core Philosophy

- A single isolated measure is not yet a number line. It is only a reading.
- A reusable number system appears only after a relation is pinned and frozen into a basis.
- Opposed or orthogonal bases should encode polarity, inversion, bounded structure, and unresolved tension numerically.
- When a value does not fit the current basis, it should degrade into tension rather than become an opaque error state.
- Structure should be traversable numerically and remain meaningful at intermediate stages.
- The same mechanism should ideally explain attachment, relation, hardening, relaxation, branching, and composition.

In practice this means:

- `Core4/Core4.csproj` contains the numeric and structural engine.
- `Core4.AvaloniaProbe/` contains the current interactive demo application.
- The interactive demo is now the main way to explore the engine visually.

## Quick Start

### Prerequisites

- .NET 10 SDK
- Python 3.x for serving the static browser build locally
- WebAssembly tools for browser publishing:

```bash
dotnet workload install wasm-tools
```

## Run The Windows Desktop Demo

From the repository root:

```bash
dotnet run --project Core4.AvaloniaProbe/Core4.AvaloniaProbe.Desktop/Core4.AvaloniaProbe.Desktop.csproj
```

This launches **Core4 Interactive**, the Avalonia desktop demo.

## Run The Web Demo

### 1. Publish the browser build

From the repository root:

```bash
dotnet publish Core4.AvaloniaProbe/Core4.AvaloniaProbe.Browser/Core4.AvaloniaProbe.Browser.csproj -c Release -o Core4.AvaloniaProbe/publish-browser
```

### 2. Serve the published static files

Serve the `wwwroot` folder inside the publish output:

```bash
cd Core4.AvaloniaProbe/publish-browser/wwwroot
python -m http.server 8080
```

If your system uses `python3` instead of `python`, use:

```bash
python3 -m http.server 8080
```

### 3. Open the demo

Open this in a browser:

- `http://localhost:8080`

Important:

- Serve the site over HTTP. Do not open `index.html` directly from disk.
- The web root is `Core4.AvaloniaProbe/publish-browser/wwwroot`, not just `Core4.AvaloniaProbe/publish-browser`.

## Linux And iOS Viewing

The current recommended path for Linux and iOS review is the browser build.

### Linux

Use the same publish + Python steps above, then open:

- `http://localhost:8080`

### iOS

Run the Python server on another machine on the same network and bind it to all interfaces:

```bash
cd Core4.AvaloniaProbe/publish-browser/wwwroot
python -m http.server 8080 --bind 0.0.0.0
```

Then open Safari on the iPhone or iPad and browse to:

- `http://<your-computer-ip>:8080`

Example:

- `http://192.168.1.25:8080`

## Notes

- The Windows desktop demo includes a small **Web** button that publishes the browser target, starts a local preview server, and opens the local browser automatically.
- If the browser only shows the Avalonia splash screen, make sure `wasm-tools` is installed, rebuild the browser target, and serve the published `wwwroot` folder over HTTP.
- Legacy display code is being retired; the Avalonia demo is the active visual path for the project.
