using Agterhuis.Ui.Designer.Model;

namespace Agterhuis.Ui.Designer.Commands;

public sealed class ImportEntitiesCommand : IDesignDocumentCommand
{
    public ImportEntitiesCommand(IReadOnlyList<DesignEntity> entities, SchemaImportApplyOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(entities);
        Entities = entities;
        Options = options ?? new SchemaImportApplyOptions();
    }

    public IReadOnlyList<DesignEntity> Entities { get; }

    public SchemaImportApplyOptions Options { get; }

    public string Name => "Import entities";

    public bool Apply(DesignDocument document)
    {
        if (Entities.Count == 0)
        {
            return false;
        }

        var didMutate = false;
        foreach (var entity in Entities)
        {
            var normalized = CloneEntity(entity);
            if (string.IsNullOrWhiteSpace(normalized.Name))
            {
                continue;
            }

            var existingIndex = document.DataModel.Entities.FindIndex(candidate => string.Equals(candidate.Name, normalized.Name, StringComparison.OrdinalIgnoreCase));
            if (existingIndex >= 0)
            {
                if (Options.ConflictResolution == SchemaImportConflictResolution.Cancel)
                {
                    continue;
                }

                if (Options.ConflictResolution == SchemaImportConflictResolution.Overwrite)
                {
                    document.DataModel.Entities[existingIndex] = normalized;
                    didMutate = true;
                    continue;
                }

                normalized.Name = CreateUniqueEntityName(document.DataModel, normalized.Name);
                normalized.PluralName = normalized.Name + "en";
                document.DataModel.Entities.Add(normalized);
                didMutate = true;
                continue;
            }

            document.DataModel.Entities.Add(normalized);
            didMutate = true;
        }

        return didMutate;
    }

    private static DesignEntity CloneEntity(DesignEntity entity)
    {
        return new DesignEntity
        {
            Name = entity.Name.Trim(),
            PluralName = string.IsNullOrWhiteSpace(entity.PluralName) ? entity.Name.Trim() + "en" : entity.PluralName.Trim(),
            Seed = new DesignSeedSettings { RowCount = Math.Max(1, entity.Seed.RowCount), Seed = Math.Max(1, entity.Seed.Seed) },
            Metadata = new DesignEntityMetadata
            {
                Description = entity.Metadata.Description,
                Endpoints = entity.Metadata.Endpoints
                    .Where(static endpoint => !string.IsNullOrWhiteSpace(endpoint.Path) && !string.IsNullOrWhiteSpace(endpoint.Method))
                    .Select(static endpoint => new DesignEntityEndpointMetadata { Path = endpoint.Path.Trim(), Method = endpoint.Method.Trim().ToUpperInvariant() })
                    .ToList()
            },
            Fields = entity.Fields
                .Where(static field => !string.IsNullOrWhiteSpace(field.Name))
                .Select(static field => new DesignField
                {
                    Name = field.Name.Trim(),
                    DisplayLabel = string.IsNullOrWhiteSpace(field.DisplayLabel) ? null : field.DisplayLabel.Trim(),
                    Type = field.Type,
                    IsRequired = field.IsRequired,
                    IsForeignKey = field.IsForeignKey,
                    ReferenceEntityName = field.ReferenceEntityName,
                    Pattern = field.Pattern,
                    EnumValues = field.EnumValues.Where(static value => !string.IsNullOrWhiteSpace(value)).Select(static value => value.Trim()).Distinct(StringComparer.Ordinal).ToList()
                })
                .ToList()
        };
    }

    private static string CreateUniqueEntityName(DesignDataModel model, string baseName)
    {
        var index = 2;
        var candidate = baseName;
        while (model.Entities.Any(existing => string.Equals(existing.Name, candidate, StringComparison.OrdinalIgnoreCase)))
        {
            candidate = $"{baseName}_{index++}";
        }

        return candidate;
    }
}
