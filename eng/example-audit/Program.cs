using ExampleAudit;

var repoRoot = RepositoryLayout.FindRepositoryRoot(Directory.GetCurrentDirectory());
var result = ExampleAuditEngine.Run(repoRoot);
var markdown = ExampleAuditEngine.BuildMarkdownReport(result);

var outputPath = Path.Combine(repoRoot, "docs", "EXAMPLE-AUDIT.md");
File.WriteAllText(outputPath, markdown);

Console.WriteLine($"Example audit report written: {outputPath}");
Console.WriteLine($"Unallowlisted similarity violations: {result.UnallowlistedSimilarityViolations}");
Console.WriteLine($"Unallowlisted title/code mismatches: {result.UnallowlistedTitleMismatches}");

return result.IsSuccess ? 0 : 1;
