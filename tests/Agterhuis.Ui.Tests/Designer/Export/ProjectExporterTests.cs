using System.IO.Compression;
using System.Text;
using System.Text.Json;
using Agterhuis.Ui.Designer.Export;
using Agterhuis.Ui.Designer.Model;
using Agterhuis.Ui.Designer.Serialization;

namespace Agterhuis.Ui.Designer.Tests.Export;

public class ProjectExporterTests
{
    private readonly ProjectExporter _exporter;

    public ProjectExporterTests()
    {
        _exporter = new ProjectExporter();
    }

    [Fact]
    public void ExportProject_WithValidDocument_ProducesResult()
    {
        // Arrange
        var document = new DesignDocument
        {
            Name = "Export Test",
            Version = "1.0",
            Pages =
            [
                new DesignPage
                {
                    Route = "/",
                    Title = "Home",
                    Nodes = [CreateValidNode("home-node")]
                }
            ]
        };

        // Act
        var result = _exporter.ExportProject(document, "MyProject", "plum");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("MyProject", result.ProjectName);
        Assert.Equal("plum", result.ThemeFamily);
    }

    [Fact]
    public void ExportProject_WithInvalidTheme_ThrowsException()
    {
        // Arrange
        var document = new DesignDocument { Name = "Test", Pages = [] };

        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => _exporter.ExportProject(document, "MyProject", "invalid-theme"));
    }

    [Fact]
    public void ExportProject_WithEmptyProjectName_ThrowsException()
    {
        // Arrange
        var document = new DesignDocument { Name = "Test", Pages = [] };

        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => _exporter.ExportProject(document, "", "plum"));
    }

    [Fact]
    public void ExportProject_WithNullDocument_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => _exporter.ExportProject(null!, "MyProject", "plum"));
    }

    [Fact]
    public void ExportProject_WithInvalidAccessibleDocument_ThrowsException()
    {
        var document = new DesignDocument
        {
            Name = "Invalid Export",
            Pages =
            [
                new DesignPage
                {
                    Route = "/",
                    Title = "Home",
                    Nodes =
                    [
                        new DesignNode
                        {
                            Id = "invalid-accessibility-node",
                            ComponentType = "AgtTextField",
                            Parameters = new Dictionary<string, DesignParameterValue>(StringComparer.Ordinal)
                        }
                    ]
                }
            ]
        };

        var ex = Assert.Throws<InvalidOperationException>(() => _exporter.ExportProject(document, "MyProject", "plum"));
    Assert.Contains("Label of AriaLabel", ex.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("plum")]
    [InlineData("ocean")]
    [InlineData("dagobah")]
    [InlineData("dathomir")]
    [InlineData("hoth")]
    [InlineData("tatooine")]
    public void ExportProject_WithValidThemes_Succeeds(string theme)
    {
        // Arrange
        var document = new DesignDocument
        {
            Name = "Test",
            Pages =
            [
                new DesignPage
                {
                    Route = "/",
                    Title = "Home",
                    Nodes = [CreateValidNode("theme-node")]
                }
            ]
        };

        // Act
        var result = _exporter.ExportProject(document, "MyProject", theme);

        // Assert
        Assert.Equal(theme, result.ThemeFamily);
    }

    [Fact]
    public void ExportProject_IncludesDesignDocumentAsJson()
    {
        // Arrange
        var document = new DesignDocument
        {
            Name = "Document Export",
            Version = "2.0",
            Pages =
            [
                new DesignPage
                {
                    Route = "/test",
                    Title = "Test Page",
                    Nodes = [CreateValidNode("doc-node")]
                }
            ]
        };

        // Act
        var result = _exporter.ExportProject(document, "TestProject", "ocean");

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.ZipData);

        using var archive = new ZipArchive(new MemoryStream(result.ZipData), ZipArchiveMode.Read);
        Assert.Contains(archive.Entries, entry => entry.FullName == "TestProject/design/document.json");
        Assert.Contains(archive.Entries, entry => entry.FullName == "TestProject/README.md");
        Assert.Contains(archive.Entries, entry => entry.FullName == "TestProject/Program.cs");

        using var documentEntryStream = archive.GetEntry("TestProject/design/document.json")!.Open();
        using var reader = new StreamReader(documentEntryStream, Encoding.UTF8);
        var json = reader.ReadToEnd();
        var roundTripped = DesignDocumentSerializer.Deserialize(json);

        Assert.Equal(document.Name, roundTripped.Name);
        Assert.Equal(document.Version, roundTripped.Version);
        Assert.Equal(document.Pages[0].Route, roundTripped.Pages[0].Route);
    }

    [Fact]
    public void ExportProject_IncludesGeneratedDataServiceContracts()
    {
        var document = DesignDocumentTemplates.Create(DesignDocumentTemplateKind.FormPage, "Data demo");

        var result = _exporter.ExportProject(document, "DataProject", "plum");

        using var archive = new ZipArchive(new MemoryStream(result.ZipData), ZipArchiveMode.Read);
        Assert.Contains(archive.Entries, entry => entry.FullName == "DataProject/Services/DesignDataContracts.cs");
        Assert.Contains(archive.Entries, entry => entry.FullName == "DataProject/Services/DesignDataService.cs");
        Assert.Contains(archive.Entries, entry => entry.FullName == "DataProject/Services/IDataProvider.cs");
        Assert.Contains(archive.Entries, entry => entry.FullName == "DataProject/Services/SeedDataProvider.cs");
    }

    [Fact]
    public void ExportProject_RegistersGeneratedDataServiceInProgram()
    {
        var document = DesignDocumentTemplates.Create(DesignDocumentTemplateKind.FormPage, "Data demo");

        var result = _exporter.ExportProject(document, "DataProject", "plum");

        using var archive = new ZipArchive(new MemoryStream(result.ZipData), ZipArchiveMode.Read);
        using var programStream = archive.GetEntry("DataProject/Program.cs")!.Open();
        using var reader = new StreamReader(programStream, Encoding.UTF8);
        var programSource = reader.ReadToEnd();

        Assert.Contains("using ExportedApp.Services;", programSource, StringComparison.Ordinal);
        Assert.Contains("var useSeedData = builder.Configuration.GetValue<bool>(\"UseSeedData\", true);", programSource, StringComparison.Ordinal);
        Assert.Contains("builder.Services.AddScoped<DesignDataService>();", programSource, StringComparison.Ordinal);
        Assert.Contains("if (useSeedData)", programSource, StringComparison.Ordinal);
        Assert.Contains("builder.Services.AddScoped<IDataProvider, SeedDataProvider>();", programSource, StringComparison.Ordinal);
    }

    [Fact]
    public void ExportProject_WithSeedDataDisabled_OmitsDataServiceFilesAndDisablesRegistration()
    {
        var document = DesignDocumentTemplates.Create(DesignDocumentTemplateKind.FormPage, "Data demo");

        var result = _exporter.ExportProject(document, "DataProject", "plum", includeSeedData: false);

        using var archive = new ZipArchive(new MemoryStream(result.ZipData), ZipArchiveMode.Read);
        Assert.DoesNotContain(archive.Entries, entry => entry.FullName == "DataProject/Services/DesignDataContracts.cs");
        Assert.DoesNotContain(archive.Entries, entry => entry.FullName == "DataProject/Services/DesignDataService.cs");
        Assert.DoesNotContain(archive.Entries, entry => entry.FullName == "DataProject/Services/IDataProvider.cs");
        Assert.DoesNotContain(archive.Entries, entry => entry.FullName == "DataProject/Services/SeedDataProvider.cs");

        using var programStream = archive.GetEntry("DataProject/Program.cs")!.Open();
        using var programReader = new StreamReader(programStream, Encoding.UTF8);
        var programSource = programReader.ReadToEnd();

        Assert.DoesNotContain("using ExportedApp.Services;", programSource, StringComparison.Ordinal);
        Assert.DoesNotContain("builder.Services.AddScoped<DesignDataService>();", programSource, StringComparison.Ordinal);
        Assert.DoesNotContain("builder.Services.AddScoped<IDataProvider, SeedDataProvider>();", programSource, StringComparison.Ordinal);
        Assert.Contains("var useSeedData = builder.Configuration.GetValue<bool>(\"UseSeedData\", false);", programSource, StringComparison.Ordinal);

        using var appSettingsStream = archive.GetEntry("DataProject/appsettings.json")!.Open();
        using var appSettingsReader = new StreamReader(appSettingsStream, Encoding.UTF8);
        var appSettings = appSettingsReader.ReadToEnd();
        Assert.Contains("\"UseSeedData\": false", appSettings, StringComparison.Ordinal);
    }

    [Fact]
    public void ExportProject_WithSeedDataEnabled_WritesAppSettingsAndRealisticSeeds()
    {
        var document = DesignDocumentTemplates.Create(DesignDocumentTemplateKind.FormPage, "Data demo");

        var result = _exporter.ExportProject(document, "DataProject", "plum", includeSeedData: true);

        using var archive = new ZipArchive(new MemoryStream(result.ZipData), ZipArchiveMode.Read);

        using var appSettingsStream = archive.GetEntry("DataProject/appsettings.json")!.Open();
        using var appSettingsReader = new StreamReader(appSettingsStream, Encoding.UTF8);
        var appSettings = appSettingsReader.ReadToEnd();
        Assert.Contains("\"UseSeedData\": true", appSettings, StringComparison.Ordinal);

        using var dataServiceStream = archive.GetEntry("DataProject/Services/DesignDataService.cs")!.Open();
        using var dataServiceReader = new StreamReader(dataServiceStream, Encoding.UTF8);
        var dataService = dataServiceReader.ReadToEnd();
        Assert.Contains("ATG-2024", dataService, StringComparison.Ordinal);
        Assert.Contains("klant{index + 1}@voorbeeld.nl", dataService, StringComparison.Ordinal);

        using var providerStream = archive.GetEntry("DataProject/Services/SeedDataProvider.cs")!.Open();
        using var providerReader = new StreamReader(providerStream, Encoding.UTF8);
        var provider = providerReader.ReadToEnd();
        Assert.Contains("public sealed class SeedDataProvider : IDataProvider", provider, StringComparison.Ordinal);
        Assert.Contains("private readonly DesignDataService _dataService;", provider, StringComparison.Ordinal);
    }

    [Fact]
    public void ExportProject_ProducesRunnableProjectSkeleton()
    {
        var document = DesignDocumentTemplates.Create(DesignDocumentTemplateKind.FormPage, "Smoke demo");

        var result = _exporter.ExportProject(document, "SmokeProject", "plum");

        using var archive = new ZipArchive(new MemoryStream(result.ZipData), ZipArchiveMode.Read);

        Assert.Contains(archive.Entries, entry => entry.FullName == "SmokeProject/design/document.json");
        Assert.Contains(archive.Entries, entry => entry.FullName == "SmokeProject/Program.cs");
        Assert.Contains(archive.Entries, entry => entry.FullName == "SmokeProject/Components/App.razor");
        Assert.Contains(archive.Entries, entry => entry.FullName == "SmokeProject/Components/Routes.razor");
        Assert.Contains(archive.Entries, entry => entry.FullName.EndsWith(".razor", StringComparison.Ordinal));
    }

    [Fact]
    public void ExportProject_WithMultiplePages_GeneratesAllPages()
    {
        var document = new DesignDocument
        {
            Name = "Multi-Page",
            Version = "1.0",
            Pages =
            [
                new DesignPage { Route = "/", Title = "Home" },
                new DesignPage { Route = "/about", Title = "About", Nodes = [CreateValidNode("about-node")] },
                new DesignPage { Route = "/contact", Title = "Contact", Nodes = [CreateValidNode("contact-node")] }
            ]
        };

        document.Pages[0].Nodes = [CreateValidNode("root-node")];

        var result = _exporter.ExportProject(document, "MyApp", "plum");

        Assert.NotNull(result);

        using var archive = new ZipArchive(new MemoryStream(result.ZipData), ZipArchiveMode.Read);
        Assert.Contains(archive.Entries, entry => entry.FullName == "MyApp/Components/Pages/.razor");
        Assert.Contains(archive.Entries, entry => entry.FullName == "MyApp/Components/Pages/About.razor");
        Assert.Contains(archive.Entries, entry => entry.FullName == "MyApp/Components/Pages/Contact.razor");
    }

    private static DesignNode CreateValidNode(string id)
    {
        return new DesignNode
        {
            Id = id,
            ComponentType = "AgtTextField",
            Parameters = new Dictionary<string, DesignParameterValue>(StringComparer.Ordinal)
            {
                ["Label"] = DesignParameterValue.FromValue("Naam"),
                ["AriaLabel"] = DesignParameterValue.FromValue("Naam")
            }
        };
    }
}
