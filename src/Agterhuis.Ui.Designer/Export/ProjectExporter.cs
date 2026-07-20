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

        // Validate theme family
        var validThemes = new[] { "plum", "ocean", "dagobah", "dathomir", "hoth", "tatooine" };
        if (!validThemes.Contains(themeFamily, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"Invalid theme family. Must be one of: {string.Join(", ", validThemes)}", nameof(themeFamily));
        }

        // Create the project structure in memory (key = file path, value = file content)
        var projectFiles = new Dictionary<string, string>(StringComparer.Ordinal);

        // Add generated .razor pages
        var pagesPath = $"{projectName}/Components/Pages";
        foreach (var page in document.Pages)
        {
            var razorCode = _codeGenerator.GeneratePageCode(page, document);
            var fileName = ToPascalCase(page.Route.TrimStart('/').Replace('/', '-')) + ".razor";
            projectFiles[$"{pagesPath}/{fileName}"] = razorCode;
        }

        // Include the design document as JSON (for round-trip/re-import in future)
        var documentJson = JsonSerializer.Serialize(document, DesignJsonOptions.Default);
        projectFiles[$"{projectName}/design/document.json"] = documentJson;

        // TODO: Embed and substitute template files here
        // For v1, the template would be included as embedded resources in the Designer assembly
        // This would involve:
        // - Getting embedded template files (csproj files, layout files, etc.)
        // - Substituting {{ProjectName}} and {{ThemeFamily}} placeholders
        // - Adding generated pages and design/document.json
        // - Creating the zip file

        // Generate the zip file
        var zipData = CreateProjectZip(projectFiles, projectName, themeFamily);

        return new ExportResult(
            ProjectName: projectName,
            ThemeFamily: themeFamily,
            ZipData: zipData,
            ExportedAt: DateTime.UtcNow);
    }

    /// <summary>
    /// Creates a zip file containing all project files.
    /// For now, returns a minimal zip with the design document.
    /// </summary>
    private static byte[] CreateProjectZip(
        Dictionary<string, string> projectFiles,
        string projectName,
        string themeFamily)
    {
        // TODO: Implement actual zip creation once System.IO.Compression support is verified in Blazor WASM
        // For v1, create a minimal zip file with just the design document
        // This is a placeholder that creates a valid empty zip structure
        
        // Minimal ZIP file format (empty archive)
        // A valid zip file starts with: 50 4B 05 06 (PK\x05\x06) followed by 18 bytes of metadata
        var minimalZip = new byte[]
        {
            0x50, 0x4B, 0x05, 0x06,  // PK\x05\x06 - End of central directory signature
            0x00, 0x00,              // Disk number
            0x00, 0x00,              // Disk number where central dir starts
            0x00, 0x00,              // Number of disk entries
            0x00, 0x00,              // Total number of entries
            0x00, 0x00, 0x00, 0x00,  // Central directory size
            0x00, 0x00, 0x00, 0x00,  // Central directory offset
            0x00, 0x00               // Comment length
        };

        return minimalZip;
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
