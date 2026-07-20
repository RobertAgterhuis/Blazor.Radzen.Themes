using Agterhuis.Ui.Designer.Model;

namespace Agterhuis.Ui.Tests;

public sealed class DesignerStartScreenTests
{
    [Fact]
    public void AllTemplatesRemainValidDesignDocuments()
    {
        foreach (var definition in DesignDocumentTemplates.DefinitionsList)
        {
            var document = DesignDocumentTemplates.Create(definition.Kind, definition.DisplayName + " demo");
            var errors = Agterhuis.Ui.Designer.Validation.DesignDocumentValidator.Validate(document);

            Assert.Empty(errors);
        }
    }
}