using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Reflection;
using Agterhuis.Ui.Designer.CodeGen;
using Agterhuis.Ui.Designer.Model;
using Agterhuis.Ui.Designer.Serialization;
using Agterhuis.Ui.Designer.Validation;

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
    private static readonly string TemplateNamespace = "Agterhuis.Ui.Designer.Export.Templates";

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
        string themeFamily = "plum",
        bool includeSeedData = true)
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

        var validationErrors = DesignDocumentValidator.Validate(document);
        var blockingErrors = validationErrors
            .Where(static error => error.Severity == DesignValidationSeverity.Error)
            .ToArray();

        if (blockingErrors.Length > 0)
        {
            throw new InvalidOperationException(string.Join("; ", blockingErrors.Select(error => $"{error.Path}: {error.Message}")));
        }

        var projectFiles = new Dictionary<string, string>(StringComparer.Ordinal);
        var normalizedThemeFamily = SupportedThemeFamilies.First(family => string.Equals(family, themeFamily, StringComparison.OrdinalIgnoreCase));

        var pagesPath = $"{projectName}/Components/Pages";
        foreach (var page in document.Pages)
        {
            var razorCode = _codeGenerator.GeneratePageCode(page, document);
            var normalizedRoute = page.Route.TrimStart('/').Replace('/', '-');
            var fileName = string.IsNullOrWhiteSpace(normalizedRoute)
                ? "Home.razor"
                : ToPascalCase(normalizedRoute) + ".razor";
            projectFiles[$"{pagesPath}/{fileName}"] = razorCode;
        }

        projectFiles[$"{projectName}/design/document.json"] = JsonSerializer.Serialize(document, DesignJsonOptions.Default);
        AddDataServiceFiles(projectFiles, projectName, document, includeSeedData);

        foreach (var (path, content) in GetStarterTemplateFiles(projectName, normalizedThemeFamily, includeSeedData))
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

    private static IReadOnlyDictionary<string, string> GetStarterTemplateFiles(string projectName, string themeFamily, bool includeSeedData)
    {
        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [$"{projectName}/{projectName}.csproj"] = LoadTemplateText("Agterhuis.Ui.Demo.export.csproj.template", projectName, themeFamily),
            [$"{projectName}/README.md"] = LoadTemplateText("README.template", projectName, themeFamily),
            [$"{projectName}/Program.cs"] = LoadTemplateText("Program.template", projectName, themeFamily, includeSeedData),
            [$"{projectName}/appsettings.json"] = LoadTemplateText("appsettings.json.template", projectName, themeFamily, includeSeedData),
            [$"{projectName}/Components/_Imports.razor"] = LoadTemplateText("_Imports.razor.template", projectName, themeFamily),
            [$"{projectName}/Components/Routes.razor"] = LoadTemplateText("Routes.razor.template", projectName, themeFamily),
            [$"{projectName}/Components/App.razor"] = LoadTemplateText("App.razor.template", projectName, themeFamily),
            [$"{projectName}/Components/Layout/MainLayout.razor"] = LoadTemplateText("Components.Layout.MainLayout.template", projectName, themeFamily),
            [$"{projectName}/wwwroot/index.html"] = LoadTemplateText("wwwroot.index.html.template", projectName, themeFamily),
            [$"{projectName}/wwwroot/app.css"] = LoadTemplateText("wwwroot.app.css.template", projectName, themeFamily)
        };
    }

    private static string LoadTemplateText(string resourceSuffix, string projectName, string themeFamily, bool includeSeedData = true)
    {
        var resourceName = TemplateNamespace + "." + resourceSuffix.Replace('/', '.');
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Missing export template resource '{resourceName}'. Available resources: {string.Join(", ", Assembly.GetExecutingAssembly().GetManifestResourceNames().Where(name => name.StartsWith(TemplateNamespace, StringComparison.Ordinal)).OrderBy(name => name, StringComparer.Ordinal))}");
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false);
        var servicesUsing = "using ExportedApp.Services;";
        var registration = "builder.Services.AddSingleton<DesignDataService>();\nif (useSeedData)\n{\n    builder.Services.AddScoped<IDataProvider, SeedDataProvider>();\n}";
        return reader.ReadToEnd()
            .Replace("__PROJECT_NAME__", projectName, StringComparison.Ordinal)
            .Replace("__THEME_FAMILY__", themeFamily, StringComparison.Ordinal)
            .Replace("__SERVICES_USING__", servicesUsing, StringComparison.Ordinal)
            .Replace("__USE_SEED_DATA__", includeSeedData ? "true" : "false", StringComparison.Ordinal)
            .Replace("__SEED_DATA_REGISTRATION__", registration, StringComparison.Ordinal);
    }

    private static void AddDataServiceFiles(Dictionary<string, string> projectFiles, string projectName, DesignDocument document, bool includeSeedData)
    {
        var servicesPath = $"{projectName}/Services";
        var dataModel = document.DataModel;

        projectFiles[$"{servicesPath}/DesignDataContracts.cs"] = GenerateDataContracts(document.DataModel);
        projectFiles[$"{servicesPath}/DesignDataService.cs"] = GenerateDataService(document.DataModel, includeSeedData);
        projectFiles[$"{servicesPath}/IDataProvider.cs"] = GenerateDataProviderInterface(document.DataModel);
        projectFiles[$"{servicesPath}/SeedDataProvider.cs"] = GenerateSeedDataProvider(document.DataModel);
    }

    private static string GenerateDataProviderInterface(DesignDataModel dataModel)
    {
        var lines = new List<string>
        {
            "using System.Collections.Generic;",
            string.Empty,
            "namespace ExportedApp.Services;",
            string.Empty,
            "public interface IDataProvider",
            "{"
        };

        foreach (var entity in dataModel.Entities)
        {
            lines.Add($"    IReadOnlyList<{entity.Name}Record> Get{entity.PluralName}();");
        }

        lines.Add("}");
        return string.Join(Environment.NewLine, lines);
    }

    private static string GenerateSeedDataProvider(DesignDataModel dataModel)
    {
        var lines = new List<string>
        {
            "using System.Collections.Generic;",
            string.Empty,
            "namespace ExportedApp.Services;",
            string.Empty,
            "public sealed class SeedDataProvider : IDataProvider",
            "{",
            "    private readonly DesignDataService _dataService;",
            string.Empty,
            "    public SeedDataProvider(DesignDataService dataService)",
            "    {",
            "        _dataService = dataService;",
            "    }",
            string.Empty
        };

        foreach (var entity in dataModel.Entities)
        {
            lines.Add($"    public IReadOnlyList<{entity.Name}Record> Get{entity.PluralName}() => _dataService.Get{entity.PluralName}();");
        }

        lines.Add("}");
        return string.Join(Environment.NewLine, lines);
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

    private static string GenerateDataService(DesignDataModel dataModel, bool includeSeedData)
    {
        var lines = new List<string>
        {
            "using System.Collections.Generic;",
            "",
            "namespace ExportedApp.Services;",
            "",
            "public sealed class DesignDataService",
            "{"
        };

        if (includeSeedData)
        {
            lines.Insert(1, "using System.Linq;");
        }

        foreach (var entity in dataModel.Entities)
        {
            lines.Add($"    private readonly List<{entity.Name}Record> _{ToCamelCase(entity.PluralName)};");
        }

        lines.Add("");
        lines.Add("    public DesignDataService()");
        lines.Add("    {");
        foreach (var entity in dataModel.Entities)
        {
            lines.Add(includeSeedData
                ? $"        _{ToCamelCase(entity.PluralName)} = Generate{entity.PluralName}(new Random({entity.Seed.Seed})).ToList();"
                : $"        _{ToCamelCase(entity.PluralName)} = [];");
        }
        lines.Add("    }");
        lines.Add("");

        foreach (var entity in dataModel.Entities)
        {
            lines.Add($"    public IReadOnlyList<{entity.Name}Record> Get{entity.PluralName}() => _{ToCamelCase(entity.PluralName)};");
            lines.Add("");
        }

        if (includeSeedData)
        {
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
                    lines.Add($"                {field.Name} = {GenerateSeedLiteral(field, entity.Name)},");
                }
                lines.Add("            };");
                lines.Add("        }");
                lines.Add("    }");
                lines.Add("");
            }

            lines.Add("    private static T Pick<T>(Random random, IReadOnlyList<T> items)");
            lines.Add("        => items[random.Next(items.Count)];");
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

    private static string GenerateSeedLiteral(DesignField field, string entityName)
        => (entityName, field.Name, field.Type) switch
        {
            ("Schadedossier", "Dossiernummer", _) => "$\"ATG-2024-{index + 1:00000}\"",
            ("Schadedossier", "Status", _) => "Pick(random, new[] { \"Nieuw\", \"Ingepland\", \"InBehandeling\", \"Gereed\", \"Gefactureerd\", \"Gesloten\" })",
            ("Schadedossier", "Schadesoort", _) => "Pick(random, new[] { \"Sterretje\", \"Ster\", \"Barst\", \"TotaalBreuk\" })",
            ("Schadedossier", "GlasType", _) => "Pick(random, new[] { \"Voorruit\", \"Achterruit\", \"Zijruit\", \"Dakraam\" })",
            ("Schadedossier", "Actie", _) => "Pick(random, new[] { \"Reparatie\", \"Vervanging\" })",
            ("Schadedossier", "Opmerkingen", _) => "$\"Dossier {index + 1} opmerking.\"",

            ("Klant", "Klantnaam", _) => "$\"Klant {index + 1}\"",
            ("Klant", "KlantType", _) => "Pick(random, new[] { \"Particulier\", \"Lease\", \"Wagenparkbeheerder\", \"Dealer\" })",
            ("Klant", "Telefoonnummer", _) => "$\"06 {random.Next(10,99)} {random.Next(10,99)} {random.Next(10,99)}\"",
            ("Klant", "Email", _) => "$\"klant{index + 1}@voorbeeld.nl\"",
            ("Klant", "Verzekeraar", _) => "Pick(random, new[] { \"Centraal Beheer\", \"Interpolis\", \"FBTO\", \"Univé\", \"OHRA\" })",
            ("Klant", "Polisnummer", _) => "$\"POL-{random.Next(100000, 999999)}\"",
            ("Klant", "Woonplaats", _) => "Pick(random, new[] { \"Amsterdam\", \"Rotterdam\", \"Utrecht\", \"Den Haag\", \"Eindhoven\" })",

            ("Voertuig", "Kenteken", _) => "$\"{Pick(random, new[] { \"AB\", \"CD\", \"EF\", \"GH\" })}-{random.Next(100,999)}-{Pick(random, new[] { \"A\", \"B\", \"C\", \"D\" })}\"",
            ("Voertuig", "Merk", _) => "Pick(random, new[] { \"Volkswagen\", \"Toyota\", \"BMW\", \"Peugeot\", \"Opel\", \"Škoda\" })",
            ("Voertuig", "Model", _) => "Pick(random, new[] { \"Golf\", \"Corolla\", \"3 Serie\", \"208\", \"Astra\", \"Octavia\" })",
            ("Voertuig", "Kleur", _) => "Pick(random, new[] { \"Grijs\", \"Wit\", \"Zwart\", \"Blauw\", \"Rood\", \"Zilver\" })",

            ("Werkorder", "DossierNummer", _) => "$\"ATG-2024-{index + 1:00000}\"",
            ("Werkorder", "WerkorderType", _) => "Pick(random, new[] { \"RuitReparatie\", \"RuitVervanging\", \"AdasKalibratie\" })",
            ("Werkorder", "Monteur", _) => "Pick(random, new[] { \"J. de Vries\", \"M. Jansen\", \"S. Bakker\", \"T. Visser\" })",
            ("Werkorder", "Status", _) => "Pick(random, new[] { \"Gepland\", \"Onderweg\", \"Bezig\", \"Afgerond\" })",

            ("Factuur", "FactuurNummer", _) => "$\"F-2024-{index + 1:00000}\"",
            ("Factuur", "BetaalStatus", _) => "Pick(random, new[] { \"Open\", \"Verzonden\", \"BetaaldKlant\", \"Volledig\" })",

            _ => field.Type switch
            {
                DesignFieldType.Int => "random.Next(1, 1000)",
                DesignFieldType.Decimal => "decimal.Round((decimal)(random.NextDouble() * 1000), 2)",
                DesignFieldType.Bool => "random.NextDouble() > 0.5",
                DesignFieldType.DateTime => "DateTime.Today.AddDays(-random.Next(0, 60))",
                DesignFieldType.Enum => field.EnumValues.Count > 0
                    ? $"Pick(random, new[] {{ {string.Join(", ", field.EnumValues.Select(static value => $"\"{value}\""))} }})"
                    : "string.Empty",
                _ => $"$\"{field.Name} {{index + 1}}\""
            }
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
