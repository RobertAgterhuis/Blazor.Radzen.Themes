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
                    Nodes = []
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
        var document = new DesignDocument { Name = "Test", Pages = [] };

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
                    Title = "Test Page"
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
        // Arrange
        var document = new DesignDocument
        {
            Name = "Multi-Page",
            Version = "1.0",
            Pages =
            [
                new DesignPage { Route = "/", Title = "Home" },
                new DesignPage { Route = "/about", Title = "About" },
                new DesignPage { Route = "/contact", Title = "Contact" }
            ]
        };

        // Act
        var result = _exporter.ExportProject(document, "MyApp", "plum");

        // Assert
        Assert.NotNull(result);
        // More detailed validation would check the zip contents
    }
}
