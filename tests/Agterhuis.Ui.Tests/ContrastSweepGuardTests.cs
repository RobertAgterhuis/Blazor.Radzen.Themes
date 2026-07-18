using System.Text.Json;

namespace Agterhuis.Ui.Tests;

public sealed class ContrastSweepGuardTests
{
    [Fact]
    public void ContrastSweepScriptExists()
    {
        var scriptPath = GetWorkspaceFilePath(Path.Combine("eng", "contrast-sweep", "contrast-sweep.mjs"));
        Assert.True(File.Exists(scriptPath));
    }

    [Fact]
    public void ContrastAllowlistParsesAsJsonArray()
    {
        var allowlistPath = GetWorkspaceFilePath(Path.Combine("eng", "contrast-sweep", "allowlist.json"));
        var text = File.ReadAllText(allowlistPath);
        var document = JsonDocument.Parse(text);

        Assert.Equal(JsonValueKind.Array, document.RootElement.ValueKind);
    }

    private static string GetWorkspaceFilePath(string relativePath)
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            current = current.Parent;
        }

        throw new FileNotFoundException($"Could not find '{relativePath}' from '{AppContext.BaseDirectory}'.");
    }
}