using System.Text.Json.Nodes;
using TokenExport;

namespace Agterhuis.Ui.Tests;

public sealed class TokenExportTests
{
    [Fact]
    public void TokenExport_WritesFamilyArtifactsForAllThemeFamilies()
    {
        var repoRoot = FindRepositoryRoot(AppContext.BaseDirectory);
        var outputDir = Path.Combine(Path.GetTempPath(), "agt-token-export", Guid.NewGuid().ToString("N"));

        try
        {
            var report = TokenExportEngine.Export(repoRoot, outputDir);

            Assert.Equal(14, report.FamiliesExported);
            Assert.Equal(28, report.ArtifactsWritten);

            var plumDesign = JsonNode.Parse(File.ReadAllText(Path.Combine(outputDir, "design-tokens.plum.json")))!.AsObject();
            var plumStyles = JsonNode.Parse(File.ReadAllText(Path.Combine(outputDir, "style-dictionary.plum.json")))!.AsObject();

            Assert.Equal("plum", plumDesign["family"]!.GetValue<string>());
            Assert.True(plumDesign["modes"]!.AsArray().Count >= 2);
            Assert.NotNull(plumDesign["tokens"]);
            Assert.NotNull(plumStyles["color/primary/500"]);
        }
        finally
        {
            if (Directory.Exists(outputDir))
            {
                Directory.Delete(outputDir, recursive: true);
            }
        }
    }

    private static string FindRepositoryRoot(string startPath)
    {
        var current = new DirectoryInfo(startPath);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Agterhuis.Ui.sln")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException($"Could not find repository root from '{startPath}'.");
    }
}