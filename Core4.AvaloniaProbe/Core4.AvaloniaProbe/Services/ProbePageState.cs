using System;
using System.IO;
using System.Runtime.Versioning;
using System.Text.Json;
using System.Runtime.InteropServices.JavaScript;

namespace Core4.AvaloniaProbe.Services;

/// <summary>
/// Small shared page-route persistence for the probe shell. Desktop remembers the last route locally; browser reads
/// and writes the route through the location hash so links can open directly to a page.
/// </summary>
public static partial class ProbePageState
{
    private const string HashPrefix = "#page=";

    public static string? TryRestoreRoute()
    {
        return OperatingSystem.IsBrowser()
            ? TryReadBrowserRoute()
            : TryReadDesktopRoute();
    }

    public static void RememberRoute(string route)
    {
        if (string.IsNullOrWhiteSpace(route))
        {
            return;
        }

        if (OperatingSystem.IsBrowser())
        {
            TryWriteBrowserRoute(route);
            return;
        }

        TryWriteDesktopRoute(route);
    }

    private static string? TryReadDesktopRoute()
    {
        try
        {
            var path = DesktopStatePath();

            if (!File.Exists(path))
            {
                return null;
            }

            var state = JsonSerializer.Deserialize<PageState>(File.ReadAllText(path));
            return string.IsNullOrWhiteSpace(state?.Route) ? null : state.Route;
        }
        catch
        {
            return null;
        }
    }

    private static void TryWriteDesktopRoute(string route)
    {
        try
        {
            var path = DesktopStatePath();
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(new PageState(route)));
        }
        catch
        {
            // Best-effort only.
        }
    }

    private static string DesktopStatePath() => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Core4",
        "probe-page.json");

    [SupportedOSPlatform("browser")]
    private static string? TryReadBrowserRoute()
    {
        try
        {
            var global = JSHost.GlobalThis;
            var location = global.GetPropertyAsJSObject("location");
            var hash = location?.GetPropertyAsString("hash");

            if (string.IsNullOrWhiteSpace(hash))
            {
                return null;
            }

            return hash.StartsWith(HashPrefix, StringComparison.OrdinalIgnoreCase)
                ? Uri.UnescapeDataString(hash[HashPrefix.Length..])
                : null;
        }
        catch
        {
            return null;
        }
    }

    [SupportedOSPlatform("browser")]
    private static void TryWriteBrowserRoute(string route)
    {
        try
        {
            var global = JSHost.GlobalThis;
            var location = global.GetPropertyAsJSObject("location");
            var hash = $"{HashPrefix}{Uri.EscapeDataString(route)}";

            if (location?.GetPropertyAsString("hash") != hash)
            {
                location?.SetProperty("hash", hash);
            }
        }
        catch
        {
            // Best-effort only.
        }
    }

    private sealed record PageState(string Route);
}
