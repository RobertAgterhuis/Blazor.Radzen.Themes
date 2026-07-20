using System.IO.Compression;
using System.Text;
using System.Text.Json;
using Agterhuis.Ui.Designer.CodeGen;
using Agterhuis.Ui.Designer.Model;
using Agterhuis.Ui.Designer.Serialization;

namespace Agterhuis.Ui.Designer.Export;

/// <summary>
/// Represents the result of a project export operation.
/// </summary>
public sealed record ExportResult(
    string ProjectName,
    string ThemeFamily,
    byte[] ZipData,
    DateTime ExportedAt);

/// <summary>
/// Generates a complete .NET project structure from a DesignDocument.
/// This implementation is designed to be fully client-side (works in Blazor WASM).
/// 
/// The export process:
/// 1. Takes a DesignDocument with pages, validation, and theme settings
/// 2. Substitutes project name and theme throughout the template files
/// 3. Generates .razor pages from the design model
/// 4. Includes the design document as design/document.json (for re-import)
/// 5. Zips everything and returns as byte array (for download)
/// 
/// Note: This is client-side only; it does NOT execute `dotnet new` or any build process.
/// Template files must be embedded as resources during the build.
/// </summary>
public sealed class ProjectExporter
{
    private static readonly string[] SupportedThemeFamilies = ["plum", "ocean", "dagobah", "dathomir", "hoth", "tatooine"];

    private readonly RazorCodeGenerator _codeGenerator;

    public ProjectExporter(RazorCodeGenerator? codeGenerator = null)
    {
        _codeGenerator = codeGenerator ?? new();
    }

    /// <summary>
    /// Exports a DesignDocument as a complete .NET project.
    /// </summary>
    /// <param name="document">The design document to export</param>
    /// <param name="projectName">Name for the exported project</param>
    /// <param name="themeFamily">Theme family (plum, ocean, dagobah, dathomir, hoth, tatooine)</param>
    /// <returns>Export result with zipped project data</returns>
    public ExportResult ExportProject(
        DesignDocument document,
        string projectName,
        string themeFamily = "plum")
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(projectName);

        if (string.IsNullOrWhiteSpace(projectName))
        {
            throw new ArgumentException("Project name cannot be empty.", nameof(projectName));
        }

        if (!SupportedThemeFamilies.Contains(themeFamily, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"Invalid theme family. Must be one of: {string.Join(", ", SupportedThemeFamilies)}", nameof(themeFamily));
        }

        var projectFiles = new Dictionary<string, string>(StringComparer.Ordinal);
        var normalizedThemeFamily = SupportedThemeFamilies.First(family => string.Equals(family, themeFamily, StringComparison.OrdinalIgnoreCase));

        var pagesPath = $"{projectName}/Components/Pages";
        foreach (var page in document.Pages)
        {
            var razorCode = _codeGenerator.GeneratePageCode(page, document);
            var fileName = ToPascalCase(page.Route.TrimStart('/').Replace('/', '-')) + ".razor";
            projectFiles[$"{pagesPath}/{fileName}"] = razorCode;
        }

        projectFiles[$"{projectName}/design/document.json"] = JsonSerializer.Serialize(document, DesignJsonOptions.Default);
        AddDataServiceFiles(projectFiles, projectName, document);

        foreach (var (path, content) in GetStarterTemplateFiles(projectName, normalizedThemeFamily))
        {
            projectFiles[path] = content;
        }

        var zipData = CreateProjectZip(projectFiles, projectName);

        return new ExportResult(
            ProjectName: projectName,
            ThemeFamily: themeFamily,
            ZipData: zipData,
            ExportedAt: DateTime.UtcNow);
    }

    private static byte[] CreateProjectZip(
        Dictionary<string, string> projectFiles,
        string projectName)
    {
        using var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var (path, content) in projectFiles.OrderBy(static item => item.Key, StringComparer.Ordinal))
            {
                var entry = archive.CreateEntry(path, CompressionLevel.Optimal);
                using var entryStream = entry.Open();
                using var writer = new StreamWriter(entryStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
                writer.Write(content);
            }

            archive.CreateEntry($"{projectName}/.template-checked", CompressionLevel.NoCompression);
        }

        return stream.ToArray();
    }

    private static IReadOnlyDictionary<string, string> GetStarterTemplateFiles(string projectName, string themeFamily)
    {
        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [$"{projectName}/README.md"] = $$"""
# {{projectName}}

Generated from Agterhuis.Ui.Designer.

Theme family: {{themeFamily}}

The design model is stored at `design/document.json`.
""".Replace("{{projectName}}", projectName, StringComparison.Ordinal).Replace("{{themeFamily}}", themeFamily, StringComparison.Ordinal),
            [$"{projectName}/Program.cs"] = $$"""
using Agterhuis.Ui.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseStaticWebAssets();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

var app = builder.Build();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
""",
            [$"{projectName}/Components/_Imports.razor"] = "@using Microsoft.AspNetCore.Components\n@using Microsoft.AspNetCore.Components.Web\n",
            [$"{projectName}/Components/Routes.razor"] = "<Router AppAssembly=\"typeof(Program).Assembly\">\n    <Found Context=\"routeData\">\n        <RouteView RouteData=\"routeData\" DefaultLayout=\"typeof(MainLayout)\" />\n    </Found>\n</Router>\n",
            [$"{projectName}/Components/App.razor"] = "<Routes />\n",
            [$"{projectName}/Components/Layout/MainLayout.razor"] = "@inherits LayoutComponentBase\n\n<div class=\"page\">\n    @Body\n</div>\n",
            [$"{projectName}/wwwroot/app.css"] = $"body {{ font-family: system-ui, sans-serif; }}\n:root {{ --theme-family: '{themeFamily}'; }}\n"
        };
    }

    private static void AddDataServiceFiles(Dictionary<string, string> projectFiles, string projectName, DesignDocument document)
    {
        var servicesPath = $"{projectName}/Services";
        var dataModel = document.DataModel;

        projectFiles[$"{servicesPath}/DesignDataContracts.cs"] = GenerateDataContracts(document.DataModel);
        projectFiles[$"{servicesPath}/DesignDataService.cs"] = GenerateDataService(document.DataModel);
    }

    private static string GenerateDataContracts(DesignDataModel dataModel)
    {
        var lines = new List<string>
        {
            "using System.Collections.Generic;",
            "",
            "namespace ExportedApp.Services;",
            ""
        };

        foreach (var entity in dataModel.Entities)
        {
            lines.Add($"public sealed record {entity.Name}Record");
            lines.Add("{");
            foreach (var field in entity.Fields)
            {
                lines.Add($"    public {GetClrType(field)} {field.Name} {{ get; init; }} = {GetDefaultLiteral(field)};");
            }
            lines.Add("}");
            lines.Add("");
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static string GenerateDataService(DesignDataModel dataModel)
    {
        var lines = new List<string>
        {
            "using System.Collections.Generic;",
            "using System.Linq;",
            "",
            "namespace ExportedApp.Services;",
            "",
            "public sealed class DesignDataService",
            "{"
        };

        foreach (var entity in dataModel.Entities)
        {
            lines.Add($"    private readonly List<{entity.Name}Record> _{ToCamelCase(entity.PluralName)};");
        }

        lines.Add("");
        lines.Add("    public DesignDataService()");
        lines.Add("    {");
        foreach (var entity in dataModel.Entities)
        {
            lines.Add($"        _{ToCamelCase(entity.PluralName)} = Generate{entity.PluralName}(new Random({entity.Seed.Seed})).ToList();");
        }
        lines.Add("    }");
        lines.Add("");

        foreach (var entity in dataModel.Entities)
        {
            lines.Add($"    public IReadOnlyList<{entity.Name}Record> Get{entity.PluralName}() => _{ToCamelCase(entity.PluralName)};");
            lines.Add("");
        }

        foreach (var entity in dataModel.Entities)
        {
            lines.Add($"    private static IEnumerable<{entity.Name}Record> Generate{entity.PluralName}(Random random)");
            lines.Add("    {");
            lines.Add($"        for (var index = 0; index < {entity.Seed.RowCount}; index++)");
            lines.Add("        {");
            lines.Add($"            yield return new {entity.Name}Record");
            lines.Add("            {");
            foreach (var field in entity.Fields)
            {
                lines.Add($"                {field.Name} = {GenerateSeedLiteral(field)},");
            }
            lines.Add("            };");
            lines.Add("        }");
            lines.Add("    }");
            lines.Add("");
        }

        lines.Add("}");
        return string.Join(Environment.NewLine, lines);
    }

    private static string GetClrType(DesignField field)
        => field.Type switch
        {
            DesignFieldType.Int => "int",
            DesignFieldType.Decimal => "decimal",
            DesignFieldType.Bool => "bool",
            DesignFieldType.DateTime => "DateTime",
            DesignFieldType.Enum => "string",
            _ => "string"
        };

    private static string GetDefaultLiteral(DesignField field)
        => field.Type switch
        {
            DesignFieldType.Int => "0",
            DesignFieldType.Decimal => "0m",
            DesignFieldType.Bool => "false",
            DesignFieldType.DateTime => "DateTime.MinValue",
            _ => "string.Empty"
        };

    private static string GenerateSeedLiteral(DesignField field)
        => field.Type switch
        {
            DesignFieldType.Int => "random.Next(1, 1000)",
            DesignFieldType.Decimal => "decimal.Round((decimal)(random.NextDouble() * 1000), 2)",
            DesignFieldType.Bool => "random.NextDouble() > 0.5",
            DesignFieldType.DateTime => "DateTime.Today.AddDays(-random.Next(0, 60))",
            DesignFieldType.Enum => field.EnumValues.Count > 0 ? $"\"{field.EnumValues[0]}\"" : "string.Empty",
            _ => "string.Empty"
        };

    private static string ToCamelCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return char.ToLowerInvariant(value[0]) + value[1..];
    }

    /// <summary>
    /// Converts a kebab-case or space-separated string to PascalCase.
    /// </summary>
    private static string ToPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var parts = input.Split(new[] { '-', ' ', '_' }, StringSplitOptions.RemoveEmptyEntries);
        return string.Concat(parts.Select(part =>
            char.ToUpperInvariant(part[0]) + (part.Length > 1 ? part[1..].ToLowerInvariant() : string.Empty)));
    }
}
