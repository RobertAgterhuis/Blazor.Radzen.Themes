# Designer Phase 4 API Reference

## RazorCodeGenerator

```csharp
// Basic usage
var generator = new RazorCodeGenerator();
string razorCode = generator.GeneratePageCode(page, document);

// With custom registry
var registry = DesignerComponentRegistry.Instance;
var generator = new RazorCodeGenerator(registry);

// For deterministic testing
var testTime = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
var generator = new RazorCodeGenerator(testTimestamp: testTime);
```

### Key Methods
- `GeneratePageCode(DesignPage page, DesignDocument document) → string`
  - Returns: Clean, deterministic Razor markup
  - Throws: `ArgumentNullException` if page or document is null

## RazorFormatter

```csharp
var formatter = new RazorFormatter(indentSize: 4);

string indented = formatter.Indent("text", level: 2);        // 8 spaces + "text"
string param = formatter.FormatParameter("Label", "My Label"); // @Label="My Label"
string normalized = formatter.Normalize(textWithNewlines);   // Cleaned
```

## ProjectExporter

```csharp
var exporter = new ProjectExporter();

// Basic export
var result = exporter.ExportProject(document, "MyApp", "plum");

// With custom code generator
var codeGen = new RazorCodeGenerator();
var exporter = new ProjectExporter(codeGen);

// Result usage
byte[] zipBytes = result.ZipData;
File.WriteAllBytes($"{result.ProjectName}.zip", zipBytes);
```

### Validation
Throws `ArgumentException` if:
- Theme is not in: plum, ocean, dagobah, dathomir, hoth, tatooine
- Project name is empty or whitespace

Throws `ArgumentNullException` if:
- `document` is null

## DesignerCodePanel Component

```razor
<DesignerCodePanel 
    CurrentPage="selectedPage"
    Document="designDocument"
    OnDocumentChanged="@HandleDocumentChanged" />

@code {
    private DesignPage? selectedPage;
    private DesignDocument? designDocument;

    private async Task HandleDocumentChanged(DesignDocument newDocument)
    {
        designDocument = newDocument;
        // Apply changes to the design
    }
}
```

### Parameters
- `CurrentPage` (DesignPage?): Currently selected page to generate code for
- `Document` (DesignDocument?): The design document
- `OnDocumentChanged` (EventCallback<DesignDocument>): Fired when JSON model is applied

### Behavior
- **Code Tab**: Shows generated Razor (read-only)
- **Model Tab**: Allows editing JSON with validation

---

## Data Models

### ExportResult
```csharp
public record ExportResult(
    string ProjectName,
    string ThemeFamily,
    byte[] ZipData,
    DateTime ExportedAt);
```

### DesignDocument (Existing)
```csharp
public class DesignDocument
{
    public string Name { get; set; }
    public string Version { get; set; }
    public int SchemaVersion { get; set; } = 1;
    public List<DesignPage> Pages { get; set; } = [];
}
```

### DesignPage (Existing)
```csharp
public class DesignPage
{
    public string Route { get; set; } = "/";
    public string Title { get; set; }
    public List<DesignNode> Nodes { get; set; } = [];
}
```

---

## Error Handling

### RazorCodeGenerator
- **Unknown component type**: Logged, component skipped
- **Missing registry descriptor**: No error, safe fallback
- **Invalid parameter value**: Uses last known-good value

### ProjectExporter
- **Invalid theme**: `ArgumentException` with valid theme list
- **Null input**: `ArgumentNullException`
- **Export failure**: Returns empty minimal zip (graceful degradation)

### DesignerCodePanel
- **Invalid JSON**: Error message shown in "Model" tab
- **Deserialization failure**: Preserves last valid model
- **Application failure**: Message displayed inline

---

## Testing Helpers

### RazorCodeGenerator Testing
```csharp
// Deterministic output testing
var timestamp = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
var gen1 = new RazorCodeGenerator(testTimestamp: timestamp);
var gen2 = new RazorCodeGenerator(testTimestamp: timestamp);

var code1 = gen1.GeneratePageCode(page, doc);
var code2 = gen2.GeneratePageCode(page, doc);

// Should be byte-identical
Assert.Equal(Encoding.UTF8.GetBytes(code1), Encoding.UTF8.GetBytes(code2));
```

### ProjectExporter Testing
```csharp
// All valid themes
var themes = new[] { "plum", "ocean", "dagobah", "dathomir", "hoth", "tatooine" };
foreach (var theme in themes)
{
    var result = exporter.ExportProject(document, "Test", theme);
    Assert.NotNull(result);
}
```

---

## Performance Notes

| Operation | Time | Notes |
|-----------|------|-------|
| Code generation | <1ms | Per page |
| Determinism check | <1ms | Same input, comparable |
| Export validation | <1ms | Theme + name checks |
| JSON validation | ~5ms | Depends on model size |

---

## Thread Safety

- `RazorCodeGenerator`: Thread-safe (stateless)
- `RazorFormatter`: Thread-safe (stateless)
- `ProjectExporter`: Thread-safe (stateless)
- `DesignerCodePanel`: Use in Blazor component lifecycle only

---

## Future API Changes (Phase 5+)

Planned but not yet implemented:

```csharp
// Monaco integration
public class MonacoOptions
{
    public string Theme { get; set; } // "vs" or "vs-dark"
    public int TabSize { get; set; } = 4;
}

// Template embedding
public class TemplateEmbedder
{
    public Dictionary<string, string> LoadEmbeddedTemplates() { ... }
}

// Full zip generation
public class ZipProjectBuilder
{
    public byte[] BuildZip(Dictionary<string, string> files) { ... }
}
```

---

## Version History

| Version | Date | Notes |
|---------|------|-------|
| 1.0 | 2024-Q1 | Initial implementation (phase 51) |
| 1.5 | TBD | Full zip, template embedding (phase 5) |
| 2.0 | TBD | Monaco editor, advanced features |

