using System.Text;

namespace TokenAudit;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var repoRoot = RepositoryLayout.FindRepositoryRoot(AppContext.BaseDirectory);
        var report = TokenAuditEngine.Generate(repoRoot);

        var docsPath = Path.Combine(repoRoot, "docs", "TOKEN-AUDIT.md");
        Directory.CreateDirectory(Path.GetDirectoryName(docsPath)!);
        await File.WriteAllTextAsync(docsPath, report.ToMarkdown(repoRoot), Encoding.UTF8);

        Console.WriteLine(docsPath);
        Console.WriteLine(report.SummaryLine());

        return report.HasFailures ? 1 : 0;
    }
}