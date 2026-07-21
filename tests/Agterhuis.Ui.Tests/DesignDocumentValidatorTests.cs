using Agterhuis.Ui.Designer.Model;
using Agterhuis.Ui.Designer.Validation;

namespace Agterhuis.Ui.Tests;

public sealed class DesignDocumentValidatorTests
{
    [Fact]
    public void Validate_ReportsDuplicateRoutesAcrossPages()
    {
        var document = new DesignDocument
        {
            Name = "Validation",
            Pages =
            [
                new DesignPage { Route = "/same", Title = "One" },
                new DesignPage { Route = "/same", Title = "Two" }
            ]
        };

        var errors = DesignDocumentValidator.Validate(document);

        Assert.Contains(errors, static error => error.Code == "DuplicateRoute" && error.Path == "Pages[0]/Route" && error.Severity == DesignValidationSeverity.Error);
        Assert.Contains(errors, static error => error.Code == "DuplicateRoute" && error.Path == "Pages[1]/Route" && error.Severity == DesignValidationSeverity.Error);
    }

    [Fact]
    public void Validate_ReportsEmptyDocument()
    {
        var document = new DesignDocument { Name = "Empty", Pages = [] };

        var errors = DesignDocumentValidator.Validate(document);

        Assert.Contains(errors, static error => error.Code == "EmptyDocument" && error.Severity == DesignValidationSeverity.Error);
    }

    [Fact]
    public void Validate_ReportsEmptyPageAsWarning()
    {
        var document = new DesignDocument
        {
            Name = "Validation",
            Pages = [new DesignPage { Route = "/ok", Title = "Page", Nodes = [] }]
        };

        var errors = DesignDocumentValidator.Validate(document);

        Assert.Contains(errors, static error => error.Code == "EmptyPage" && error.Severity == DesignValidationSeverity.Warning);
    }

    [Fact]
    public void Validate_ReportsInvalidRoute()
    {
        var document = new DesignDocument
        {
            Name = "Validation",
            Pages =
            [
                new DesignPage
                {
                    Route = "invalid route",
                    Title = "Page",
                    Nodes =
                    [
                        new DesignNode { Id = "invalid-route-node", ComponentType = "RadzenCard" }
                    ]
                }
            ]
        };

        var errors = DesignDocumentValidator.Validate(document);

        Assert.Contains(errors, static error => error.Code == "InvalidRoute" && error.Severity == DesignValidationSeverity.Error);
    }

    [Fact]
    public void Validate_ReportsDuplicateNodeIds()
    {
        var duplicateId = "node-1";
        var document = new DesignDocument
        {
            Name = "Validation",
            Pages =
            [
                new DesignPage
                {
                    Route = "/one",
                    Title = "One",
                    Nodes =
                    [
                        new DesignNode { Id = duplicateId, ComponentType = "AgtCard" },
                        new DesignNode { Id = duplicateId, ComponentType = "AgtCard" }
                    ]
                }
            ]
        };

        var errors = DesignDocumentValidator.Validate(document);

        Assert.Equal(2, errors.Count(static error => error.Code == "DuplicateNodeId"));
    }

    [Fact]
    public void Validate_ReportsRequiredSlotEmpty()
    {
        var document = new DesignDocument
        {
            Name = "Validation",
            Pages =
            [
                new DesignPage
                {
                    Route = "/slot",
                    Title = "Slot",
                    Nodes =
                    [
                        new DesignNode
                        {
                            Id = "slot-node-root",
                            ComponentType = "AgtCard",
                            Children = new Dictionary<string, List<DesignNode>>(StringComparer.Ordinal)
                            {
                                ["ChildContent"] = []
                            }
                        }
                    ]
                }
            ]
        };

        var errors = DesignDocumentValidator.Validate(document);

        Assert.Contains(errors, static error => error.Code == "RequiredSlotEmpty" && error.Severity == DesignValidationSeverity.Warning);
    }

    [Fact]
    public void Validate_ReportsIncompatibleNestingForColumnsSlot()
    {
        var document = new DesignDocument
        {
            Name = "Validation",
            Pages =
            [
                new DesignPage
                {
                    Route = "/nesting",
                    Title = "Nesting",
                    Nodes =
                    [
                        new DesignNode
                        {
                            Id = "grid-node-root",
                            ComponentType = "RadzenDataGrid",
                            Children = new Dictionary<string, List<DesignNode>>(StringComparer.Ordinal)
                            {
                                ["Columns"] =
                                [
                                    new DesignNode
                                    {
                                        Id = "bad-column-node",
                                        ComponentType = "AgtCard",
                                        Children = new Dictionary<string, List<DesignNode>>(StringComparer.Ordinal)
                                        {
                                            ["ChildContent"] = []
                                        }
                                    }
                                ]
                            },
                            Parameters = new Dictionary<string, DesignParameterValue>(StringComparer.Ordinal)
                            {
                                ["Data"] = DesignParameterValue.FromValue("Schadedossier")
                            }
                        }
                    ]
                }
            ]
        };

        var errors = DesignDocumentValidator.Validate(document);

        Assert.Contains(errors, static error => error.Code == "IncompatibleNesting" && error.Severity == DesignValidationSeverity.Warning);
    }

    [Fact]
    public void Validate_ReportsBrokenEntityReference()
    {
        var document = CreateGridDocument("UnknownEntity", "Dossiernummer");

        var errors = DesignDocumentValidator.Validate(document);

        Assert.Contains(errors, static error => error.Code == "BrokenEntityReference" && error.Severity == DesignValidationSeverity.Error);
    }

    [Fact]
    public void Validate_ReportsBrokenFieldReference()
    {
        var document = CreateGridDocument("Schadedossier", "BestaatNiet");

        var errors = DesignDocumentValidator.Validate(document);

        Assert.Contains(errors, static error => error.Code == "BrokenFieldReference" && error.Severity == DesignValidationSeverity.Error);
    }

    [Fact]
    public void Validate_DoesNotReportBrokenFieldReference_ForKnownField()
    {
        var document = CreateGridDocument("Schadedossier", "Dossiernummer");

        var errors = DesignDocumentValidator.Validate(document);

        Assert.DoesNotContain(errors, static error => error.Code == "BrokenFieldReference");
    }

    [Fact]
    public void Validate_ReportsUnboundDataGridAsInfo()
    {
        var document = CreateGridDocument(null, "Dossiernummer");

        var errors = DesignDocumentValidator.Validate(document);

        Assert.Contains(errors, static error => error.Code == "UnboundDataGrid" && error.Severity == DesignValidationSeverity.Info);
    }

    [Fact]
    public void Validate_ReportsHardcodedColor()
    {
        var document = new DesignDocument
        {
            Name = "Validation",
            Pages =
            [
                new DesignPage
                {
                    Route = "/color",
                    Title = "Color",
                    Nodes =
                    [
                        new DesignNode
                        {
                            Id = "hardcoded-color-node",
                            ComponentType = "RadzenButton",
                            Parameters = new Dictionary<string, DesignParameterValue>(StringComparer.Ordinal)
                            {
                                ["Text"] = DesignParameterValue.FromValue("Actie"),
                                ["AriaLabel"] = DesignParameterValue.FromValue("Actie"),
                                ["Style"] = DesignParameterValue.FromValue("color: #ff00ff;")
                            }
                        }
                    ]
                }
            ]
        };

        var errors = DesignDocumentValidator.Validate(document);

        Assert.Contains(errors, static error => error.Code == "HardcodedColor" && error.Severity == DesignValidationSeverity.Warning);
    }

    [Fact]
    public void Validate_ReportsInvalidTokenReference()
    {
        var document = new DesignDocument
        {
            Name = "Validation",
            Pages =
            [
                new DesignPage
                {
                    Route = "/token",
                    Title = "Token",
                    Nodes =
                    [
                        new DesignNode
                        {
                            Id = "invalid-token-node",
                            ComponentType = "RadzenButton",
                            Parameters = new Dictionary<string, DesignParameterValue>(StringComparer.Ordinal)
                            {
                                ["Text"] = DesignParameterValue.FromValue("Actie"),
                                ["AriaLabel"] = DesignParameterValue.FromValue("Actie"),
                                ["Style"] = DesignParameterValue.FromValue("var(--agt-does-not-exist)")
                            }
                        }
                    ]
                }
            ]
        };

        var errors = DesignDocumentValidator.Validate(document);

        Assert.Contains(errors, static error => error.Code == "InvalidTokenReference" && error.Severity == DesignValidationSeverity.Warning);
    }

    [Fact]
    public void Validate_ReportsMissingFormLabel()
    {
        var document = new DesignDocument
        {
            Name = "Validation",
            Pages =
            [
                new DesignPage
                {
                    Route = "/form",
                    Title = "Form",
                    Nodes =
                    [
                        new DesignNode
                        {
                            Id = "missing-label-node",
                            ComponentType = "AgtTextField",
                            Parameters = new Dictionary<string, DesignParameterValue>(StringComparer.Ordinal)
                        }
                    ]
                }
            ]
        };

        var errors = DesignDocumentValidator.Validate(document);

        Assert.Contains(errors, static error => error.Code == "MissingFormLabel" && error.Severity == DesignValidationSeverity.Error);
    }

    [Fact]
    public void Validate_ReportsEmptyButtonText()
    {
        var document = new DesignDocument
        {
            Name = "Validation",
            Pages =
            [
                new DesignPage
                {
                    Route = "/button",
                    Title = "Button",
                    Nodes =
                    [
                        new DesignNode
                        {
                            Id = "empty-button-node",
                            ComponentType = "RadzenButton",
                            Parameters = new Dictionary<string, DesignParameterValue>(StringComparer.Ordinal)
                        }
                    ]
                }
            ]
        };

        var errors = DesignDocumentValidator.Validate(document);

        Assert.Contains(errors, static error => error.Code == "EmptyButtonText" && error.Severity == DesignValidationSeverity.Warning);
    }

    [Fact]
    public void Validate_ReportsImageWithoutAlt()
    {
        var document = new DesignDocument
        {
            Name = "Validation",
            Pages =
            [
                new DesignPage
                {
                    Route = "/image",
                    Title = "Image",
                    Nodes =
                    [
                        new DesignNode
                        {
                            Id = "image-node",
                            ComponentType = "RadzenImage",
                            Parameters = new Dictionary<string, DesignParameterValue>(StringComparer.Ordinal)
                            {
                                ["Src"] = DesignParameterValue.FromValue("/images/sample.png")
                            }
                        }
                    ]
                }
            ]
        };

        var errors = DesignDocumentValidator.Validate(document);

        Assert.Contains(errors, static error => error.Code == "ImageWithoutAlt" && error.Severity == DesignValidationSeverity.Warning);
    }

    private static DesignDocument CreateGridDocument(string? entityName, string fieldName)
    {
        var grid = new DesignNode
        {
            Id = "grid-1",
            ComponentType = "RadzenDataGrid",
            Parameters = new Dictionary<string, DesignParameterValue>(StringComparer.Ordinal),
            Children = new Dictionary<string, List<DesignNode>>(StringComparer.Ordinal)
            {
                ["Columns"] =
                [
                    new DesignNode
                    {
                        Id = "column-1",
                        ComponentType = "RadzenDataGridColumn",
                        Parameters = new Dictionary<string, DesignParameterValue>(StringComparer.Ordinal)
                        {
                            ["Property"] = DesignParameterValue.FromValue(fieldName),
                            ["Title"] = DesignParameterValue.FromValue(fieldName)
                        },
                        Children = new Dictionary<string, List<DesignNode>>(StringComparer.Ordinal)
                    }
                ]
            }
        };

        if (!string.IsNullOrWhiteSpace(entityName))
        {
            grid.Parameters["Data"] = DesignParameterValue.FromValue(entityName);
        }

        return new DesignDocument
        {
            Name = "Grid",
            DataModel = new DesignDataModel
            {
                Entities =
                [
                    new DesignEntity
                    {
                        Name = "Schadedossier",
                        PluralName = "Schadedossiers",
                        Fields =
                        [
                            new DesignField { Name = "Dossiernummer", Type = DesignFieldType.String },
                            new DesignField { Name = "Status", Type = DesignFieldType.String }
                        ]
                    }
                ]
            },
            Pages =
            [
                new DesignPage
                {
                    Route = "/grid",
                    Title = "Grid",
                    Nodes = [grid]
                }
            ]
        };
    }
}
