using System.Text.Json;

namespace ExampleAudit;

public static class ExampleAuditAllowlistStore
{
    public static ExampleAuditAllowlist Load(string repoRoot)
    {
        var allowlistPath = Path.Combine(repoRoot, "eng", "example-audit", "allowlist.json");
        if (!File.Exists(allowlistPath))
        {
            return new ExampleAuditAllowlist();
        }

        var json = File.ReadAllText(allowlistPath);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        return JsonSerializer.Deserialize<ExampleAuditAllowlist>(json, options) ?? new ExampleAuditAllowlist();
    }
}
