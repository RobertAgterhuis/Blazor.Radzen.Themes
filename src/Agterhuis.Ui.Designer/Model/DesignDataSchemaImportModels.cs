namespace Agterhuis.Ui.Designer.Model;

public enum SchemaImportSourceType
{
    JsonSchema,
    OpenApi,
    SampleJson
}

public enum SchemaImportConflictResolution
{
    Cancel,
    Overwrite,
    Rename
}

public sealed class SchemaImportResult
{
    public List<DesignEntity> Entities { get; set; } = [];

    public List<string> Warnings { get; set; } = [];
}

public sealed class SchemaImportPreview
{
    public string EntityName { get; set; } = string.Empty;

    public bool Include { get; set; } = true;

    public string? Description { get; set; }

    public List<SchemaImportFieldPreview> Fields { get; set; } = [];

    public List<DesignEntityEndpointMetadata> Endpoints { get; set; } = [];
}

public sealed class SchemaImportFieldPreview
{
    public string Name { get; set; } = string.Empty;

    public string? DisplayLabel { get; set; }

    public bool Include { get; set; } = true;

    public DesignFieldType Type { get; set; } = DesignFieldType.String;

    public bool IsRequired { get; set; }

    public List<string> EnumValues { get; set; } = [];

    public bool IsForeignKey { get; set; }

    public string? ReferenceEntityName { get; set; }
}

public sealed class SchemaImportApplyOptions
{
    public SchemaImportConflictResolution ConflictResolution { get; set; } = SchemaImportConflictResolution.Rename;
}

public sealed class SchemaImportApplyResult
{
    public int ImportedCount { get; set; }

    public List<string> SkippedEntities { get; set; } = [];
}

public sealed class FormFieldSelectionItem
{
    public string Name { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public DesignFieldType Type { get; set; }

    public bool IsRequired { get; set; }

    public bool Include { get; set; }
}

public sealed class DataGridColumnConfig
{
    public string FieldName { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;

    public string Title { get; set; } = string.Empty;

    public string Format { get; set; } = "text";

    public bool Sortable { get; set; } = true;

    public bool Filterable { get; set; } = true;

    public string Width { get; set; } = "auto";

    public int Order { get; set; }
}

public sealed class DataGridPagingConfig
{
    public bool AllowPaging { get; set; } = true;

    public int PageSize { get; set; } = 10;
}
