# Core4 Avalonia Probe

Throwaway experiment for testing a Core4-driven render surface in Avalonia.

What it includes:

- a shared Core4-backed directed segment state
- a code-driven render control with draggable endpoints
- a small Avalonia widget shell with a textbox and button
- a desktop host
- a browser host that publishes to static files

Projects:

- `Core4.AvaloniaProbe/Core4.AvaloniaProbe.Desktop/Core4.AvaloniaProbe.Desktop.csproj`
- `Core4.AvaloniaProbe/Core4.AvaloniaProbe.Browser/Core4.AvaloniaProbe.Browser.csproj`

Run desktop:

```powershell
dotnet run --project C:\Users\Robin\source\repos\Core4\Core4.AvaloniaProbe\Core4.AvaloniaProbe.Desktop\Core4.AvaloniaProbe.Desktop.csproj
```

Build browser:

```powershell
dotnet build C:\Users\Robin\source\repos\Core4\Core4.AvaloniaProbe\Core4.AvaloniaProbe.Browser\Core4.AvaloniaProbe.Browser.csproj
```

Publish browser static files:

```powershell
dotnet publish C:\Users\Robin\source\repos\Core4\Core4.AvaloniaProbe\Core4.AvaloniaProbe.Browser\Core4.AvaloniaProbe.Browser.csproj -c Release -o C:\Users\Robin\source\repos\Core4\Core4.AvaloniaProbe\publish-browser
```

Static output folder:

- `C:\Users\Robin\source\repos\Core4\Core4.AvaloniaProbe\publish-browser`

Notes:

- this probe keeps the scene code-driven inside `DirectedSegmentSceneControl`
- the outer shell still uses Avalonia controls
- the rendering path here is Avalonia custom drawing, which is enough to test the architecture before deciding whether deeper Skia interop is worth it
