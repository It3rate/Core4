# Core4 Avalonia Probe

`Core4.AvaloniaProbe` is the interactive demo application for the Core4 engine.

It hosts a set of visual numeric explorations built directly on top of Core4 structures and renderers, including:

- dominant-only half-lines
- inverted pairs
- frozen directed segments
- orthogonal output and input
- primitive bases
- multiple orthogonal inputs

The shared project contains the scene logic and renderers. The desktop and browser hosts wrap that same content for local use and web sharing.

## Projects

- `Core4.AvaloniaProbe/Core4.AvaloniaProbe.csproj`
  Shared Avalonia UI, scene controls, renderers, and demo state.
- `Core4.AvaloniaProbe/Core4.AvaloniaProbe.Desktop/Core4.AvaloniaProbe.Desktop.csproj`
  Windows desktop host.
- `Core4.AvaloniaProbe/Core4.AvaloniaProbe.Browser/Core4.AvaloniaProbe.Browser.csproj`
  Static browser host.

## Requirements

- .NET 10 SDK
- Python 3.x if you want to serve the web build locally
- WebAssembly tools for browser publishing:

```bash
dotnet workload install wasm-tools
```

## Windows

### Run the desktop app

From the repository root:

```bash
dotnet run --project Core4.AvaloniaProbe/Core4.AvaloniaProbe.Desktop/Core4.AvaloniaProbe.Desktop.csproj
```

This opens **Core4 Interactive**.

### Open the web preview from the desktop app

The desktop app includes a **Web** button in the preset row.

That button will:

1. publish the browser host
2. start a local Python preview server
3. open the local browser preview

This is the easiest way to compare desktop and browser output on Windows.

### Publish the browser build manually

```bash
dotnet publish Core4.AvaloniaProbe/Core4.AvaloniaProbe.Browser/Core4.AvaloniaProbe.Browser.csproj -c Release -o Core4.AvaloniaProbe/publish-browser
```

### Serve the published browser build

```bash
cd Core4.AvaloniaProbe/publish-browser/wwwroot
python -m http.server 8080
```

Then open:

- `http://localhost:8080`

If needed, use `python3` instead of `python`.

## Linux

The current recommended Linux path is the browser build.

### Publish

From the repository root:

```bash
dotnet publish Core4.AvaloniaProbe/Core4.AvaloniaProbe.Browser/Core4.AvaloniaProbe.Browser.csproj -c Release -o Core4.AvaloniaProbe/publish-browser
```

### Serve

```bash
cd Core4.AvaloniaProbe/publish-browser/wwwroot
python3 -m http.server 8080
```

### Open

- `http://localhost:8080`

## iOS

The current recommended iOS path is also the browser build.

### On the machine that contains this repo

Publish the browser build:

```bash
dotnet publish Core4.AvaloniaProbe/Core4.AvaloniaProbe.Browser/Core4.AvaloniaProbe.Browser.csproj -c Release -o Core4.AvaloniaProbe/publish-browser
```

Serve it on your local network:

```bash
cd Core4.AvaloniaProbe/publish-browser/wwwroot
python -m http.server 8080 --bind 0.0.0.0
```

### On the iPhone or iPad

Open Safari and browse to:

- `http://<your-computer-ip>:8080`

Example:

- `http://192.168.1.25:8080`

## Browser Notes

- Serve `Core4.AvaloniaProbe/publish-browser/wwwroot`, not just `Core4.AvaloniaProbe/publish-browser`.
- Do not open `index.html` directly from disk.
- If the browser only shows the Avalonia splash screen, make sure `wasm-tools` is installed and republish the browser host.

## Development Notes

- The scene renderers live in `Core4.AvaloniaProbe/Rendering/`.
- Interactive handles live in `Core4.AvaloniaProbe/Controls/`.
- Demo state lives in `Core4.AvaloniaProbe/Models/`.
- The demo shell and pages live in `Core4.AvaloniaProbe/Views/`.

This README intentionally focuses on the active Avalonia path and does not document legacy display code.
