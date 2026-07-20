using Agterhuis.Ui.Designer.Model;

namespace Agterhuis.Ui.Tests;

public sealed class DesignerTemplateTests
{
    [Fact]
    public void AllTemplatesProduceValidDocumentsWithAccessibleSeedContent()
    {
        foreach (var definition in DesignDocumentTemplates.DefinitionsList)
        {
            var document = DesignDocumentTemplates.Create(definition.Kind, $"{definition.DisplayName} demo");

            Assert.NotEmpty(document.Pages);
            var errors = Agterhuis.Ui.Designer.Validation.DesignDocumentValidator.Validate(document);
            Assert.DoesNotContain(errors, static error => error.Code == "MissingAccessibleLabel");
            Assert.DoesNotContain(errors, static error => error.Code == "UnknownComponentType");
        }
    }

    [Fact]
    public void BlankTemplateCreatesBaseCanvasStructure()
    {
        var document = DesignDocumentTemplates.Create(DesignDocumentTemplateKind.Blank, "Blank demo");

        Assert.Equal("Blank demo", document.Name);
        Assert.Single(document.Pages);
        Assert.Equal("RadzenRow", document.Pages[0].Nodes[0].ComponentType);
    }
}