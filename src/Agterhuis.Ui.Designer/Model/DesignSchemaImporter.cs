using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using YamlDotNet.Serialization;

namespace Agterhuis.Ui.Designer.Model;

public static class DesignSchemaImporter
{
    private static readonly JsonDocumentOptions JsonOptions = new()
    {
        AllowTrailingCommas = true,
        CommentHandling = JsonCommentHandling.Skip
    };

    public static SchemaImportResult ParseJsonSchema(string content)
    {
        var node = ParseJson(content);
        var result = new SchemaImportResult();
        var entities = new List<DesignEntity>();
        var rootName = NormalizeName(node["title"]?.GetValue<string?>() ?? "ImportedEntity");
        ParseSchemaEntity(rootName, node, entities, result.Warnings, parentName: null);
        result.Entities = entities;
        return result;
    }

    public static SchemaImportResult ParseOpenApi(string content)
    {
        var json = EnsureJson(content);
        var node = ParseJson(json);
        var result = new SchemaImportResult();

        var schemas = node["components"]?["schemas"] as JsonObject;
        if (schemas is null)
        {
            result.Warnings.Add("OpenAPI bevat geen components.schemas.");
            return result;
        }

        var endpointMap = BuildOpenApiEndpointMap(node);
        var entities = new List<DesignEntity>();
        foreach (var (name, schemaNode) in schemas)
        {
            if (schemaNode is null)
            {
                continue;
            }

            var entityName = NormalizeName(name);
            ParseSchemaEntity(entityName, schemaNode, entities, result.Warnings, parentName: null);
        }

        foreach (var entity in entities)
        {
            if (endpointMap.TryGetValue(entity.Name, out var endpoints))
            {
                entity.Metadata.Endpoints = endpoints;
            }
        }

        result.Entities = entities;
        return result;
    }

    public static SchemaImportResult ParseSampleJson(string content)
    {
        var json = ParseJson(content);
        var result = new SchemaImportResult();
        var entity = BuildEntityFromSample("SampleEntity", json, result.Warnings);
        if (entity is not null)
        {
            result.Entities = [entity];
        }

        return result;
    }

    public static List<SchemaImportPreview> CreatePreview(SchemaImportResult result)
    {
        return result.Entities.Select(entity => new SchemaImportPreview
        {
            EntityName = entity.Name,
            Description = entity.Metadata.Description,
            Endpoints = entity.Metadata.Endpoints.Select(static endpoint => new DesignEntityEndpointMetadata
            {
                Path = endpoint.Path,
                Method = endpoint.Method
            }).ToList(),
            Fields = entity.Fields.Select(static field => new SchemaImportFieldPreview
            {
                Name = field.Name,
                DisplayLabel = field.DisplayLabel,
                Type = field.Type,
                IsRequired = field.IsRequired,
                EnumValues = field.EnumValues.ToList(),
                IsForeignKey = field.IsForeignKey,
                ReferenceEntityName = field.ReferenceEntityName
            }).ToList()
        }).ToList();
    }

    public static IReadOnlyList<DesignEntity> MaterializeEntities(IReadOnlyList<SchemaImportPreview> previews)
    {
        return previews
            .Where(static preview => preview.Include)
            .Select(preview => new DesignEntity
            {
                Name = NormalizeName(preview.EntityName),
                PluralName = NormalizeName(preview.EntityName) + "en",
                Metadata = new DesignEntityMetadata
                {
                    Description = preview.Description,
                    Endpoints = preview.Endpoints
                        .Where(static endpoint => !string.IsNullOrWhiteSpace(endpoint.Path) && !string.IsNullOrWhiteSpace(endpoint.Method))
                        .Select(static endpoint => new DesignEntityEndpointMetadata { Path = endpoint.Path, Method = endpoint.Method })
                        .ToList()
                },
                Fields = preview.Fields
                    .Where(static field => field.Include && !string.IsNullOrWhiteSpace(field.Name))
                    .Select(static field => new DesignField
                    {
                        Name = NormalizeName(field.Name),
                        DisplayLabel = string.IsNullOrWhiteSpace(field.DisplayLabel) ? null : field.DisplayLabel,
                        Type = field.Type,
                        IsRequired = field.IsRequired,
                        EnumValues = field.EnumValues.Where(static value => !string.IsNullOrWhiteSpace(value)).Select(static value => value.Trim()).Distinct(StringComparer.Ordinal).ToList(),
                        IsForeignKey = field.IsForeignKey,
                        ReferenceEntityName = string.IsNullOrWhiteSpace(field.ReferenceEntityName) ? null : NormalizeName(field.ReferenceEntityName)
                    })
                    .ToList(),
                Seed = new DesignSeedSettings { RowCount = 25, Seed = 42 }
            })
            .ToList();
    }

    public static IReadOnlyList<FormFieldSelectionItem> BuildDefaultFormSelection(DesignEntity entity)
    {
        var required = entity.Fields.Where(static field => field.IsRequired).ToList();
        var optional = entity.Fields.Where(static field => !field.IsRequired).Take(5).ToHashSet();

        return entity.Fields.Select(field => new FormFieldSelectionItem
        {
            Name = field.Name,
            Label = field.DisplayLabel ?? field.Name,
            Type = field.Type,
            IsRequired = field.IsRequired,
            Include = required.Contains(field) || optional.Contains(field)
        }).ToList();
    }

    private static string EnsureJson(string content)
    {
        var trimmed = content.TrimStart();
        if (trimmed.StartsWith("{", StringComparison.Ordinal) || trimmed.StartsWith("[", StringComparison.Ordinal))
        {
            return content;
        }

        var deserializer = new DeserializerBuilder().Build();
        var yamlObject = deserializer.Deserialize<object>(content);
        return JsonSerializer.Serialize(yamlObject, new JsonSerializerOptions { WriteIndented = false });
    }

    private static JsonNode ParseJson(string content)
    {
        using var document = JsonDocument.Parse(content, JsonOptions);
        return JsonNode.Parse(document.RootElement.GetRawText()) ?? throw new InvalidOperationException("Kon JSON niet verwerken.");
    }

    private static void ParseSchemaEntity(string entityName, JsonNode schemaNode, ICollection<DesignEntity> entities, ICollection<string> warnings, string? parentName)
    {
        var schemaType = ResolveSchemaType(schemaNode);
        if (schemaType != "object")
        {
            warnings.Add($"Schema '{entityName}' is geen object en is overgeslagen.");
            return;
        }

        var entity = new DesignEntity
        {
            Name = NormalizeName(parentName is null ? entityName : $"{parentName}_{entityName}"),
            PluralName = NormalizeName(parentName is null ? entityName : $"{parentName}_{entityName}") + "en",
            Metadata = new DesignEntityMetadata
            {
                Description = schemaNode["description"]?.GetValue<string?>()
            },
            Seed = new DesignSeedSettings { RowCount = 25, Seed = 42 }
        };

        var requiredFields = (schemaNode["required"] as JsonArray)?.Select(static item => item?.GetValue<string>() ?? string.Empty).Where(static value => !string.IsNullOrWhiteSpace(value)).ToHashSet(StringComparer.OrdinalIgnoreCase)
            ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var properties = schemaNode["properties"] as JsonObject;
        if (properties is null)
        {
            warnings.Add($"Schema '{entity.Name}' bevat geen properties.");
            entities.Add(entity);
            return;
        }

        foreach (var (propertyName, propertyNode) in properties)
        {
            if (propertyNode is null)
            {
                continue;
            }

            var normalizedFieldName = NormalizeName(propertyName);
            var propertyType = ResolveSchemaType(propertyNode);
            var isRequired = requiredFields.Contains(propertyName);

            if (propertyType == "object")
            {
                ParseSchemaEntity(normalizedFieldName, propertyNode, entities, warnings, entity.Name);
                entity.Fields.Add(new DesignField
                {
                    Name = normalizedFieldName + "Id",
                    DisplayLabel = propertyName,
                    Type = DesignFieldType.String,
                    IsRequired = isRequired,
                    IsForeignKey = true,
                    ReferenceEntityName = NormalizeName($"{entity.Name}_{normalizedFieldName}")
                });

                continue;
            }

            if (propertyType == "array" && propertyNode["items"] is JsonNode itemsNode)
            {
                var itemType = ResolveSchemaType(itemsNode);
                if (itemType == "object")
                {
                    ParseSchemaEntity(normalizedFieldName + "Item", itemsNode, entities, warnings, entity.Name);
                    entity.Fields.Add(new DesignField
                    {
                        Name = normalizedFieldName + "RefId",
                        DisplayLabel = propertyName,
                        Type = DesignFieldType.String,
                        IsRequired = false,
                        IsForeignKey = true,
                        ReferenceEntityName = NormalizeName($"{entity.Name}_{normalizedFieldName}Item")
                    });

                    continue;
                }
            }

            entity.Fields.Add(CreateFieldFromSchemaProperty(propertyName, propertyNode, isRequired));
        }

        entities.Add(entity);
    }

    private static DesignField CreateFieldFromSchemaProperty(string fieldName, JsonNode propertyNode, bool isRequired)
    {
        var normalized = NormalizeName(fieldName);
        var fieldType = ResolveFieldType(propertyNode);
        var enumValues = (propertyNode["enum"] as JsonArray)?.Select(static item => item?.ToString() ?? string.Empty).Where(static value => !string.IsNullOrWhiteSpace(value)).ToList() ?? [];

        return new DesignField
        {
            Name = normalized,
            DisplayLabel = fieldName,
            Type = enumValues.Count > 0 ? DesignFieldType.Enum : fieldType,
            IsRequired = isRequired,
            EnumValues = enumValues
        };
    }

    private static DesignEntity? BuildEntityFromSample(string defaultName, JsonNode json, ICollection<string> warnings)
    {
        if (json is JsonArray array)
        {
            var objects = array.OfType<JsonObject>().ToList();
            if (objects.Count == 0)
            {
                warnings.Add("Voorbeeld JSON-array bevat geen objecten.");
                return null;
            }

            var merged = MergeObjects(objects);
            return BuildEntityFromObject(defaultName, merged, warnings);
        }

        if (json is JsonObject jsonObject)
        {
            return BuildEntityFromObject(defaultName, jsonObject, warnings);
        }

        warnings.Add("Voorbeeld JSON moet een object of array van objecten zijn.");
        return null;
    }

    private static DesignEntity BuildEntityFromObject(string name, JsonObject source, ICollection<string> warnings)
    {
        var fields = new List<DesignField>();
        foreach (var (propertyName, propertyValue) in source)
        {
            if (propertyValue is null)
            {
                fields.Add(new DesignField { Name = NormalizeName(propertyName), DisplayLabel = propertyName, Type = DesignFieldType.String, IsRequired = false });
                continue;
            }

            var type = InferFieldType(propertyValue);
            fields.Add(new DesignField
            {
                Name = NormalizeName(propertyName),
                DisplayLabel = propertyName,
                Type = type,
                IsRequired = true
            });
        }

        if (fields.Count == 0)
        {
            warnings.Add("Voorbeeld JSON bevat geen bruikbare velden.");
        }

        return new DesignEntity
        {
            Name = NormalizeName(name),
            PluralName = NormalizeName(name) + "en",
            Fields = fields,
            Seed = new DesignSeedSettings { RowCount = 25, Seed = 42 }
        };
    }

    private static JsonObject MergeObjects(IReadOnlyList<JsonObject> objects)
    {
        var merged = new JsonObject();
        foreach (var obj in objects)
        {
            foreach (var (key, value) in obj)
            {
                if (!merged.ContainsKey(key) && value is not null)
                {
                    merged[key] = value.DeepClone();
                    continue;
                }

                if (merged[key] is null && value is not null)
                {
                    merged[key] = value.DeepClone();
                }
            }
        }

        return merged;
    }

    private static Dictionary<string, List<DesignEntityEndpointMetadata>> BuildOpenApiEndpointMap(JsonNode root)
    {
        var map = new Dictionary<string, List<DesignEntityEndpointMetadata>>(StringComparer.OrdinalIgnoreCase);
        var paths = root["paths"] as JsonObject;
        if (paths is null)
        {
            return map;
        }

        foreach (var (path, pathNode) in paths)
        {
            if (pathNode is not JsonObject pathObject)
            {
                continue;
            }

            foreach (var (method, methodNode) in pathObject)
            {
                if (methodNode is not JsonObject operation)
                {
                    continue;
                }

                foreach (var entityName in FindSchemaRefs(operation))
                {
                    if (!map.TryGetValue(entityName, out var endpoints))
                    {
                        endpoints = [];
                        map[entityName] = endpoints;
                    }

                    endpoints.Add(new DesignEntityEndpointMetadata
                    {
                        Path = path,
                        Method = method.ToUpperInvariant()
                    });
                }
            }
        }

        return map;
    }

    private static IEnumerable<string> FindSchemaRefs(JsonNode node)
    {
        if (node is JsonObject obj)
        {
            foreach (var (key, value) in obj)
            {
                if (string.Equals(key, "$ref", StringComparison.OrdinalIgnoreCase) && value is JsonValue jsonValue)
                {
                    var refValue = jsonValue.GetValue<string>();
                    const string marker = "#/components/schemas/";
                    if (refValue.StartsWith(marker, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return NormalizeName(refValue[marker.Length..]);
                    }
                }

                if (value is null)
                {
                    continue;
                }

                foreach (var nested in FindSchemaRefs(value))
                {
                    yield return nested;
                }
            }
        }
        else if (node is JsonArray array)
        {
            foreach (var item in array)
            {
                if (item is null)
                {
                    continue;
                }

                foreach (var nested in FindSchemaRefs(item))
                {
                    yield return nested;
                }
            }
        }
    }

    private static DesignFieldType ResolveFieldType(JsonNode propertyNode)
    {
        if ((propertyNode["enum"] as JsonArray)?.Count > 0)
        {
            return DesignFieldType.Enum;
        }

        var schemaType = ResolveSchemaType(propertyNode);
        if (schemaType == "string" && string.Equals(propertyNode["format"]?.GetValue<string>(), "date-time", StringComparison.OrdinalIgnoreCase))
        {
            return DesignFieldType.DateTime;
        }

        return schemaType switch
        {
            "integer" => DesignFieldType.Int,
            "number" => DesignFieldType.Decimal,
            "boolean" => DesignFieldType.Bool,
            _ => DesignFieldType.String
        };
    }

    private static DesignFieldType InferFieldType(JsonNode value)
    {
        if (value is JsonValue scalar)
        {
            if (scalar.TryGetValue<bool>(out _))
            {
                return DesignFieldType.Bool;
            }

            if (scalar.TryGetValue<int>(out _))
            {
                return DesignFieldType.Int;
            }

            if (scalar.TryGetValue<decimal>(out _))
            {
                return DesignFieldType.Decimal;
            }

            if (scalar.TryGetValue<double>(out _))
            {
                return DesignFieldType.Decimal;
            }

            if (scalar.TryGetValue<string>(out var text))
            {
                if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out _))
                {
                    return DesignFieldType.DateTime;
                }

                return DesignFieldType.String;
            }
        }

        return DesignFieldType.String;
    }

    private static string ResolveSchemaType(JsonNode schemaNode)
    {
        var typeNode = schemaNode["type"];
        if (typeNode is JsonValue scalar && scalar.TryGetValue<string>(out var typeText))
        {
            return typeText;
        }

        if (typeNode is JsonArray union && union.OfType<JsonValue>().Any(static item => string.Equals(item.ToString(), "object", StringComparison.OrdinalIgnoreCase)))
        {
            return "object";
        }

        if (schemaNode["properties"] is JsonObject)
        {
            return "object";
        }

        if (schemaNode["items"] is not null)
        {
            return "array";
        }

        return "string";
    }

    private static string NormalizeName(string? rawName)
    {
        if (string.IsNullOrWhiteSpace(rawName))
        {
            return "Entity";
        }

        var builder = new StringBuilder(rawName.Length);
        foreach (var ch in rawName.Trim())
        {
            if (char.IsLetterOrDigit(ch) || ch == '_')
            {
                builder.Append(ch);
                continue;
            }

            if (char.IsWhiteSpace(ch) || ch == '-' || ch == '.')
            {
                builder.Append('_');
            }
        }

        var normalized = builder.ToString().Trim('_');
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return "Entity";
        }

        if (!char.IsLetter(normalized[0]) && normalized[0] != '_')
        {
            normalized = "_" + normalized;
        }

        return normalized;
    }
}
