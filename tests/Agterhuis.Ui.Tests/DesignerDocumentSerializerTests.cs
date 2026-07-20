using Agterhuis.Ui.Designer.Model;
using Agterhuis.Ui.Designer.Serialization;

namespace Agterhuis.Ui.Tests;

public sealed class DesignerDocumentSerializerTests
{
    [Fact]
    public void SerializeRoundtrip_PreservesDocumentShapeAndDeterministicIds()
    {
        var document = new DesignDocument
        {
            Name = "Form actions",
            Pages =
            [
                new DesignPage
                {
                    Route = "/components/forms/form-actions",
                    Title = "Form actions",
                    Nodes =
                    [
                        new DesignNode
                        {
                            ComponentType = "AgtPageHeader",
                            Parameters = new Dictionary<string, DesignParameterValue>(StringComparer.Ordinal)
                            {
                                ["Title"] = DesignParameterValue.FromValue("AgtFormActions"),
                                ["Description"] = DesignParameterValue.FromValue("Rechts uitgelijnde save/cancel rij.")
                            }
                        },
                        new DesignNode
                        {
                            ComponentType = "AgtCard",
                            Children = new Dictionary<string, List<DesignNode>>(StringComparer.Ordinal)
                            {
                                ["ChildContent"] =
                                [
                                    new DesignNode
                                    {
                                        ComponentType = "AgtFormActions",
                                        Parameters = new Dictionary<string, DesignParameterValue>(StringComparer.Ordinal)
                                        {
                                            ["SaveText"] = DesignParameterValue.FromValue("Opslaan"),
                                            ["CancelText"] = DesignParameterValue.FromValue("Annuleren")
                                        },
                                        LayoutSlot = new DesignLayoutSlot(Row: 1, Column: 1)
                                    }
                                ]
                            }
                        }
                    ]
                }
            ]
        };

        var firstJson = DesignDocumentSerializer.Serialize(document);
        var roundTripped = DesignDocumentSerializer.Deserialize(firstJson);
        var secondJson = DesignDocumentSerializer.Serialize(roundTripped);

        Assert.Equal(firstJson, secondJson);
        Assert.All(roundTripped.Pages.SelectMany(static page => page.Nodes), static node => Assert.False(string.IsNullOrWhiteSpace(node.Id)));
        Assert.Equal(roundTripped.Pages[0].Nodes[0].Id, DesignDocumentSerializer.Deserialize(secondJson).Pages[0].Nodes[0].Id);
    }
}