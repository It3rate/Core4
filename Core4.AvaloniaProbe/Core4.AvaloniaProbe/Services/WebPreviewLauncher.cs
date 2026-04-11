using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Core4.AvaloniaProbe.Services;

/// <summary>
/// Best-effort local web preview launcher for the desktop app. It publishes the browser target once, starts a
/// simple local server, and opens the preview URL.
/// </summary>
public static class WebPreviewLauncher
{
    private static Process? _serverProcess;
    private static int _serverPort = 8080;

    public static async Task LaunchAsync(string? route = null)
    {
        StopServer();

        var root = FindRepoRoot();
        var browserProject = Path.Combine(root, "Core4.AvaloniaProbe", "Core4.AvaloniaProbe.Browser", "Core4.AvaloniaProbe.Browser.csproj");
        var publishDirectory = Path.Combine(root, "Core4.AvaloniaProbe", "publish-browser");
        var webRoot = Path.Combine(publishDirectory, "wwwroot");

        await RunAsync(
            "dotnet",
            $"publish \"{browserProject}\" -c Release -o \"{publishDirectory}\" -p:UseSharedCompilation=false",
            root);

        _serverPort = FindAvailablePort();
        _serverProcess = TryStartServer(webRoot, _serverPort);
        await WaitForServerReadyAsync().ConfigureAwait(false);
        OpenBrowser(route);
    }

    private static async Task RunAsync(string fileName, string arguments, string workingDirectory)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            }
        };

        process.Start();
        await process.WaitForExitAsync().ConfigureAwait(false);

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync().ConfigureAwait(false);
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(error)
                ? $"{fileName} exited with code {process.ExitCode}."
                : error.Trim());
        }
    }

    private static Process? TryStartServer(string workingDirectory, int port)
    {
        foreach (var candidate in new[] { "python", "py", "python3" })
        {
            try
            {
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = candidate,
                    Arguments = $"-m http.server {port}",
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });

                if (process is not null)
                {
                    return process;
                }
            }
            catch
            {
                // Try the next candidate.
            }
        }

        return null;
    }

    private static void OpenBrowser(string? route)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = BuildUrl(route),
            UseShellExecute = true
        });
    }

    private static async Task WaitForServerReadyAsync()
    {
        if (_serverProcess is null)
        {
            return;
        }

        using var client = new HttpClient
        {
            Timeout = TimeSpan.FromMilliseconds(500)
        };

        var url = BuildUrl(null);
        var attempts = 30;
        for (var attempt = 0; attempt < attempts; attempt++)
        {
            if (_serverProcess.HasExited)
            {
                throw new InvalidOperationException("The local web preview server exited before it became ready.");
            }

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                using var response = await client.SendAsync(request).ConfigureAwait(false);
                if ((int)response.StatusCode is >= 200 and < 500)
                {
                    return;
                }
            }
            catch
            {
                // Server not ready yet; wait briefly and try again.
            }

            await Task.Delay(200).ConfigureAwait(false);
        }

        throw new InvalidOperationException("The local web preview server did not become ready in time.");
    }

    private static string BuildUrl(string? route)
    {
        var baseUrl = $"http://localhost:{_serverPort}";

        if (string.IsNullOrWhiteSpace(route))
        {
            return baseUrl;
        }

        return $"{baseUrl}#page={Uri.EscapeDataString(route)}";
    }

    private static void StopServer()
    {
        if (_serverProcess is null)
        {
            return;
        }

        try
        {
            if (!_serverProcess.HasExited)
            {
                _serverProcess.Kill(entireProcessTree: true);
                _serverProcess.WaitForExit(2000);
            }
        }
        catch
        {
            // Best-effort only.
        }
        finally
        {
            _serverProcess.Dispose();
            _serverProcess = null;
        }
    }

    private static int FindAvailablePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Core4.slnx")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate Core4.slnx from the desktop app.");
    }
}
