namespace TokenExport;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var repoRoot = RepositoryLayout.FindRepositoryRoot(AppContext.BaseDirectory);
        var outputDir = Path.Combine(repoRoot, "eng", "token-export", "output");

        var report = TokenExportEngine.Export(repoRoot, outputDir);

        Console.WriteLine(outputDir);
        Console.WriteLine($"Families exported: {report.FamiliesExported}");
        Console.WriteLine($"Artifacts written: {report.ArtifactsWritten}");

        await Task.CompletedTask;
        return 0;
    }
}