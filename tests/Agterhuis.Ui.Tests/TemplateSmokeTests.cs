using System.Diagnostics;
using Xunit;

namespace Agterhuis.Ui.Tests;

public sealed class TemplateSmokeTests
{
    [Fact]
    public async Task TemplatePacksInstantiatesAndBuilds()
    {
        var repoRoot = FindRepositoryRoot();
        var templateProject = Path.Combine(repoRoot, "templates", "Agterhuis.Ui.Templates", "Agterhuis.Ui.Templates.csproj");
        var libraryProject = Path.Combine(repoRoot, "src", "Agterhuis.Ui", "Agterhuis.Ui.csproj");
        var scratchRoot = Path.Combine(Path.GetTempPath(), "agt-template-smoke", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(scratchRoot);
        var cliHomeDirectory = Path.Combine(scratchRoot, "cli-home");
        Directory.CreateDirectory(cliHomeDirectory);

        var packageOutput = Path.Combine(scratchRoot, "nupkg");
        Directory.CreateDirectory(packageOutput);
        var localFeed = Path.Combine(scratchRoot, "feed");
        Directory.CreateDirectory(localFeed);

        await RunDotNetAsync(Path.GetDirectoryName(templateProject)!, cliHomeDirectory, ["pack", templateProject, "-c", "Release", "-o", packageOutput]);
        await RunDotNetAsync(Path.GetDirectoryName(libraryProject)!, cliHomeDirectory, ["pack", libraryProject, "-c", "Release", "-o", localFeed]);

        var templatePackage = Directory.GetFiles(packageOutput, "Agterhuis.Ui.Templates.*.nupkg", SearchOption.TopDirectoryOnly)
            .Single();

        await RunDotNetAsync(scratchRoot, cliHomeDirectory, ["new", "install", templatePackage, "--force"]);

        var appDir = Path.Combine(scratchRoot, "SampleApp");
        await RunDotNetAsync(scratchRoot, cliHomeDirectory, ["new", "agterhuis-app", "-n", "SampleApp", "--output", appDir, "--theme", "volt", "--variant", "light"]);

                var projectFile = Directory.GetFiles(appDir, "*.csproj", SearchOption.AllDirectories).Single();
                var projectDirectory = Path.GetDirectoryName(projectFile) ?? appDir;

                var nugetConfig = Path.Combine(projectDirectory, "NuGet.config");
        await File.WriteAllTextAsync(nugetConfig, $"""
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
        <add key="local" value="{localFeed.Replace("&", "&amp;")}" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
""");

    await RunDotNetAsync(projectDirectory, cliHomeDirectory, ["build", projectFile, "-c", "Release"]);
    }

    private static string FindRepositoryRoot()
    {
        var currentDirectory = AppContext.BaseDirectory;

        while (!string.IsNullOrWhiteSpace(currentDirectory))
        {
            if (File.Exists(Path.Combine(currentDirectory, "Agterhuis.Ui.sln")))
            {
                return currentDirectory;
            }

            var parent = Directory.GetParent(currentDirectory);
            if (parent is null)
            {
                break;
            }

            currentDirectory = parent.FullName;
        }

        throw new InvalidOperationException("Unable to locate repository root.");
    }

    private static async Task RunDotNetAsync(string workingDirectory, string cliHomeDirectory, IReadOnlyList<string> arguments)
    {
        var executable = OperatingSystem.IsWindows() ? "dotnet.exe" : "dotnet";
        var commandLine = string.Join(" ", arguments);
        var startInfo = new ProcessStartInfo(executable)
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        startInfo.Environment["DOTNET_CLI_HOME"] = cliHomeDirectory;

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException($"Failed to start dotnet {commandLine}.");
        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new Xunit.Sdk.XunitException($"dotnet {commandLine} failed in {workingDirectory}.{Environment.NewLine}{output}{Environment.NewLine}{error}");
        }
    }
}