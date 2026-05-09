using System.Diagnostics;

namespace Gojo.GearDemo.Desktop.Services;

internal static class GearDemoWebPublisher
{
    private const string DeployTokenEnvName = "SWA_CLI_DEPLOYMENT_TOKEN";
    private const string LiveUrl = "https://core4.inklingai.ca/gears/";
    private const string NodeWingetId = "OpenJS.NodeJS.20";
    private const string StaticWebAppName = "core4-static";
    private const string StaticWebAppResourceGroup = "inkling";

    public static async Task PublishDeployAndOpenAsync()
    {
        var root = FindRepoRoot();
        var browserProject = Path.Combine(root, "Gojo.GearDemo.Browser", "Gojo.GearDemo.Browser.csproj");
        var publishDirectory = Path.Combine(root, "Gojo.GearDemo", "publish-browser");
        var gearWebRoot = Path.Combine(publishDirectory, "wwwroot");
        var sharedWebRoot = Path.Combine(root, "Core4.AvaloniaProbe", "publish-browser", "wwwroot");
        var sharedGearRoute = Path.Combine(sharedWebRoot, "gears");
        var deployWebRoot = Path.Combine(root, "Gojo.GearDemo", "publish-deploy-root");

        await RunAsync(
            "dotnet",
            $"publish \"{browserProject}\" -c Release -o \"{publishDirectory}\" -p:UseSharedCompilation=false",
            root).ConfigureAwait(false);

        CopyDirectory(gearWebRoot, sharedGearRoute, sharedWebRoot);
        CopyDirectory(sharedWebRoot, deployWebRoot, root, ShouldDeployFile);

        var deploymentToken = RequireDeploymentToken();
        var npxPath = await EnsureNpxAsync(root).ConfigureAwait(false);
        await DeployAsync(npxPath, deployWebRoot, deployWebRoot, deploymentToken).ConfigureAwait(false);
        OpenBrowser();
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

    private static void CopyDirectory(
        string source,
        string target,
        string allowedRoot,
        Func<FileInfo, bool>? includeFile = null)
    {
        if (!Directory.Exists(source))
        {
            throw new DirectoryNotFoundException($"The gear browser publish output was not found at '{source}'.");
        }

        var sourceFull = Path.GetFullPath(source);
        var targetFull = Path.GetFullPath(target);
        var allowedFull = Path.GetFullPath(allowedRoot);
        var allowedPrefix = allowedFull.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!targetFull.StartsWith(allowedPrefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Refusing to publish outside the shared web root: '{targetFull}'.");
        }

        Directory.CreateDirectory(allowedFull);
        if (Directory.Exists(targetFull))
        {
            Directory.Delete(targetFull, recursive: true);
        }

        Directory.CreateDirectory(targetFull);
        foreach (var directory in Directory.EnumerateDirectories(sourceFull, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(Path.Combine(targetFull, Path.GetRelativePath(sourceFull, directory)));
        }

        foreach (var file in Directory.EnumerateFiles(sourceFull, "*", SearchOption.AllDirectories))
        {
            var fileInfo = new FileInfo(file);
            if (includeFile is not null && !includeFile(fileInfo))
            {
                continue;
            }

            var relative = Path.GetRelativePath(sourceFull, file);
            File.Copy(file, Path.Combine(targetFull, relative), overwrite: true);
        }
    }

    private static bool ShouldDeployFile(FileInfo file) =>
        !string.Equals(file.Extension, ".br", StringComparison.OrdinalIgnoreCase) &&
        !string.Equals(file.Extension, ".gz", StringComparison.OrdinalIgnoreCase);

    private static async Task DeployAsync(string npxPath, string webRoot, string workingDirectory, string deploymentToken)
    {
        if (!Directory.Exists(webRoot))
        {
            throw new DirectoryNotFoundException($"The shared web root was not found at '{webRoot}'.");
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
            if (File.Exists(Path.Combine(current.FullName, "Gojo.slnx")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate Gojo.slnx from the gear demo app.");
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
            $"Local /gears build is ready. Set '{DeployTokenEnvName}' to deploy it live.");
    }

    private static void OpenBrowser()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = LiveUrl,
            UseShellExecute = true
        });
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
