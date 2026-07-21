using Agterhuis.Ui.Designer.Model;

namespace Agterhuis.Ui.Tests;

public sealed class DesignSchemaImporterTests
{
    [Fact]
    public void ParseJsonSchema_MapsAllCoreTypesAndNestedStructures()
    {
        const string schema = """
        {
          "title": "Werkorder",
          "description": "Werkorder schema",
          "type": "object",
          "required": ["Naam", "Aantal"],
          "properties": {
            "Naam": { "type": "string" },
            "Aantal": { "type": "integer" },
            "Prijs": { "type": "number" },
            "Actief": { "type": "boolean" },
            "GeplandOp": { "type": "string", "format": "date-time" },
            "Status": { "type": "string", "enum": ["Nieuw", "Gereed"] },
            "Klant": {
              "type": "object",
              "properties": {
                "Id": { "type": "string" }
              }
            },
            "Regels": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "Omschrijving": { "type": "string" }
                }
              }
            }
          }
        }
        """;

        var result = DesignSchemaImporter.ParseJsonSchema(schema);
        var root = Assert.Single(result.Entities, entity => entity.Name == "Werkorder");

        Assert.Equal("Werkorder schema", root.Metadata.Description);
        Assert.Contains(root.Fields, field => field.Name == "Naam" && field.Type == DesignFieldType.String && field.IsRequired);
        Assert.Contains(root.Fields, field => field.Name == "Aantal" && field.Type == DesignFieldType.Int && field.IsRequired);
        Assert.Contains(root.Fields, field => field.Name == "Prijs" && field.Type == DesignFieldType.Decimal);
        Assert.Contains(root.Fields, field => field.Name == "Actief" && field.Type == DesignFieldType.Bool);
        Assert.Contains(root.Fields, field => field.Name == "GeplandOp" && field.Type == DesignFieldType.DateTime);
        Assert.Contains(root.Fields, field => field.Name == "Status" && field.Type == DesignFieldType.Enum && field.EnumValues.Count == 2);
        Assert.Contains(root.Fields, field => field.Name == "KlantId" && field.IsForeignKey);
        Assert.Contains(root.Fields, field => field.Name == "RegelsRefId" && field.IsForeignKey);

        Assert.Contains(result.Entities, entity => entity.Name == "Werkorder_Klant");
        Assert.Contains(result.Entities, entity => entity.Name == "Werkorder_RegelsItem");
    }

    [Fact]
    public void ParseOpenApi_ExtractsSchemasAndEndpointMetadata()
    {
        const string openApi = """
        openapi: 3.0.1
        info:
          title: Demo
          version: 1.0.0
        paths:
          /klanten:
            get:
              responses:
                "200":
                  description: ok
                  content:
                    application/json:
                      schema:
                        $ref: '#/components/schemas/Klant'
          /werkorders:
            post:
              requestBody:
                content:
                  application/json:
                    schema:
                      $ref: '#/components/schemas/Werkorder'
        components:
          schemas:
            Klant:
              type: object
              properties:
                Klantnaam:
                  type: string
            Werkorder:
              type: object
              properties:
                Nummer:
                  type: integer
        """;

        var result = DesignSchemaImporter.ParseOpenApi(openApi);

        var klant = Assert.Single(result.Entities, entity => entity.Name == "Klant");
        var werkorder = Assert.Single(result.Entities, entity => entity.Name == "Werkorder");
        Assert.Contains(klant.Metadata.Endpoints, endpoint => endpoint.Path == "/klanten" && endpoint.Method == "GET");
        Assert.Contains(werkorder.Metadata.Endpoints, endpoint => endpoint.Path == "/werkorders" && endpoint.Method == "POST");
    }

    [Fact]
    public void ParseSampleJson_InfersUnionOfArrayFields()
    {
        const string sample = """
        [
          { "Naam": "A", "Aantal": 1, "Actief": true },
          { "Naam": "B", "Prijs": 12.5, "GeplandOp": "2026-07-21T12:00:00Z" }
        ]
        """;

        var result = DesignSchemaImporter.ParseSampleJson(sample);
        var entity = Assert.Single(result.Entities);

        Assert.Contains(entity.Fields, field => field.Name == "Naam" && field.Type == DesignFieldType.String);
        Assert.Contains(entity.Fields, field => field.Name == "Aantal" && field.Type == DesignFieldType.Int);
        Assert.Contains(entity.Fields, field => field.Name == "Actief" && field.Type == DesignFieldType.Bool);
        Assert.Contains(entity.Fields, field => field.Name == "Prijs" && field.Type == DesignFieldType.Decimal);
        Assert.Contains(entity.Fields, field => field.Name == "GeplandOp" && field.Type == DesignFieldType.DateTime);
    }
}
