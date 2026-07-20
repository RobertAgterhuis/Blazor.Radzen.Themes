using Agterhuis.Ui.Designer.Model;
using Agterhuis.Ui.Designer.Validation;

namespace Agterhuis.Ui.Tests;

public sealed class DesignDocumentValidatorTests
{
    [Fact]
    public void Validate_ReportsPathsForUnknownTypesUnknownParametersAndMissingAccessibleLabels()
    {
        var document = new DesignDocument
        {
            Name = "Validation",
            Pages =
            [
                new DesignPage
                {
                    Route = "/validation",
                    Title = "Validation",
                    Nodes =
                    [
                        new DesignNode
                        {
                            ComponentType = "AgtTextField",
                            Parameters = new Dictionary<string, DesignParameterValue>(StringComparer.Ordinal)
                            {
                                ["Unknown"] = DesignParameterValue.FromValue(true)
                            }
                        },
                        new DesignNode
                        {
                            ComponentType = "NotARealComponent"
                        }
                    ]
                }
            ]
        };

        var errors = DesignDocumentValidator.Validate(document);

        Assert.Contains(errors, static error => error.Code == "UnknownParameter" && error.Path == "Pages[0]/Nodes[0]/Parameters/Unknown");
        Assert.Contains(errors, static error => error.Code == "MissingAccessibleLabel" && error.Path == "Pages[0]/Nodes[0]");
        Assert.Contains(errors, static error => error.Code == "UnknownComponentType" && error.Path == "Pages[0]/Nodes[1]");
    }
}