using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Core4.AvaloniaProbe.Services;

/// <summary>
/// Publishes the browser target, deploys it to Azure Static Web Apps, and opens the live route.
/// </summary>
public static class WebPreviewLauncher
{
    private const string DeployTokenEnvName = "SWA_CLI_DEPLOYMENT_TOKEN";
    private const string LiveBaseUrl = "https://core4.inklingai.ca";
    private const string NodeWingetId = "OpenJS.NodeJS.20";
    private const string StaticWebAppName = "core4-static";
    private const string StaticWebAppResourceGroup = "inkling";

    public static async Task LaunchAsync(string? route = null)
    {
        var root = FindRepoRoot();
        var browserProject = Path.Combine(root, "Core4.AvaloniaProbe", "Core4.AvaloniaProbe.Browser", "Core4.AvaloniaProbe.Browser.csproj");
        var publishDirectory = Path.Combine(root, "Core4.AvaloniaProbe", "publish-browser");
        var webRoot = Path.Combine(publishDirectory, "wwwroot");
        var deploymentToken = RequireDeploymentToken();
        var npxPath = await EnsureNpxAsync(root).ConfigureAwait(false);

        await RunAsync(
            "dotnet",
            $"publish \"{browserProject}\" -c Release -o \"{publishDirectory}\" -p:UseSharedCompilation=false",
            root);

        await DeployAsync(npxPath, webRoot, root, deploymentToken).ConfigureAwait(false);
        OpenBrowser(route);
    }

    private static async Task RunAsync(
        string fileName,
        string arguments,
        string workingDirectory,
        Action<ProcessStartInfo>? configure = null)
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
        configure?.Invoke(process.StartInfo);

        process.Start();
        var standardError = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync().ConfigureAwait(false);
        var error = await standardError.ConfigureAwait(false);

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(error)
                ? $"{fileName} exited with code {process.ExitCode}."
                : error.Trim());
        }

    }

    private static async Task DeployAsync(string npxPath, string webRoot, string workingDirectory, string deploymentToken)
    {
        if (!Directory.Exists(webRoot))
        {
            throw new DirectoryNotFoundException($"The browser publish output was not found at '{webRoot}'.");
        }

        await RunAsync(
            npxPath,
            $"--yes @azure/static-web-apps-cli deploy \"{webRoot}\" --env production --app-name {StaticWebAppName} --resource-group {StaticWebAppResourceGroup} --no-use-keychain",
            workingDirectory,
            startInfo =>
            {
                var nodeDirectory = Path.GetDirectoryName(npxPath);
                if (!string.IsNullOrWhiteSpace(nodeDirectory))
                {
                    PrependPath(startInfo, nodeDirectory);
                }

                startInfo.Environment[DeployTokenEnvName] = deploymentToken;
                startInfo.Environment["CI"] = "1";
            }).ConfigureAwait(false);
    }

    private static async Task<string> EnsureNpxAsync(string workingDirectory)
    {
        var npxPath = FindNpxPath();
        if (!string.IsNullOrWhiteSpace(npxPath))
        {
            return npxPath;
        }

        await RunAsync(
            "winget",
            $"install --id {NodeWingetId} --exact --source winget --scope user --accept-package-agreements --accept-source-agreements --silent",
            workingDirectory).ConfigureAwait(false);

        npxPath = FindNpxPath();
        if (!string.IsNullOrWhiteSpace(npxPath))
        {
            return npxPath;
        }

        throw new InvalidOperationException(
            "Node.js 20 was installed, but 'npx' could not be located. " +
            "Please restart the app once so the new Node tools become visible.");
    }

    private static string? FindNpxPath()
    {
        foreach (var candidate in EnumeratePathCandidates("npx.cmd"))
        {
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        foreach (var candidate in new[]
                 {
                     Path.Combine(localAppData, "Programs", "nodejs", "npx.cmd"),
                     Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "nodejs", "npx.cmd")
                 })
        {
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        var wingetPackages = Path.Combine(localAppData, "Microsoft", "WinGet", "Packages");
        if (!Directory.Exists(wingetPackages))
        {
            return null;
        }

        foreach (var packageDirectory in Directory.EnumerateDirectories(wingetPackages, "OpenJS.NodeJS.*"))
        {
            var packageNpx = Directory
                .EnumerateFiles(packageDirectory, "npx.cmd", SearchOption.AllDirectories)
                .FirstOrDefault(path =>
                    path.Contains("\\node-v", StringComparison.OrdinalIgnoreCase) &&
                    !path.Contains("\\node_modules\\", StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(packageNpx))
            {
                return packageNpx;
            }
        }

        return null;
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

    private static string RequireDeploymentToken()
    {
        foreach (var value in new[]
                 {
                     Environment.GetEnvironmentVariable(DeployTokenEnvName),
                     Environment.GetEnvironmentVariable(DeployTokenEnvName, EnvironmentVariableTarget.User),
                     Environment.GetEnvironmentVariable(DeployTokenEnvName, EnvironmentVariableTarget.Machine)
                 })
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        throw new InvalidOperationException(
            $"Set the '{DeployTokenEnvName}' environment variable to the Azure Static Web Apps deployment token. " +
            "This launcher reads the user-level value directly, so you do not need to restart the app after setting it.");
    }

    private static void OpenBrowser(string? route)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = BuildUrl(route),
            UseShellExecute = true
        });
    }

    private static string BuildUrl(string? route)
    {
        if (string.IsNullOrWhiteSpace(route))
        {
            return LiveBaseUrl;
        }

        return $"{LiveBaseUrl}#page={Uri.EscapeDataString(route)}";
    }

    private static string[] EnumeratePathCandidates(string fileName)
    {
        var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        return path
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(entry => Path.Combine(entry, fileName))
            .ToArray();
    }

    private static void PrependPath(ProcessStartInfo startInfo, string directory)
    {
        var currentPath = startInfo.Environment.TryGetValue("PATH", out var existing)
            ? existing
            : Environment.GetEnvironmentVariable("PATH") ?? string.Empty;

        startInfo.Environment["PATH"] = string.IsNullOrWhiteSpace(currentPath)
            ? directory
            : $"{directory}{Path.PathSeparator}{currentPath}";
    }
}
