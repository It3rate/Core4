using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Core4.AvaloniaProbe.Services;

/// <summary>
/// Best-effort local web preview launcher for the desktop app. It publishes the browser target once, starts a
/// simple local server, and opens the preview URL.
/// </summary>
public static class WebPreviewLauncher
{
    private static Process? _serverProcess;
    private const string Url = "http://localhost:8080";

    public static async Task LaunchAsync()
    {
        if (_serverProcess is not null && !_serverProcess.HasExited)
        {
            OpenBrowser();
            return;
        }

        var root = FindRepoRoot();
        var browserProject = Path.Combine(root, "Core4.AvaloniaProbe", "Core4.AvaloniaProbe.Browser", "Core4.AvaloniaProbe.Browser.csproj");
        var publishDirectory = Path.Combine(root, "Core4.AvaloniaProbe", "publish-browser");
        var webRoot = Path.Combine(publishDirectory, "wwwroot");

        await RunAsync(
            "dotnet",
            $"publish \"{browserProject}\" -c Release -o \"{publishDirectory}\" -p:UseSharedCompilation=false",
            root);

        _serverProcess = TryStartServer(webRoot);
        OpenBrowser();
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

    private static Process? TryStartServer(string workingDirectory)
    {
        foreach (var candidate in new[] { "python", "py", "python3" })
        {
            try
            {
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = candidate,
                    Arguments = "-m http.server 8080",
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

    private static void OpenBrowser()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = Url,
            UseShellExecute = true
        });
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
