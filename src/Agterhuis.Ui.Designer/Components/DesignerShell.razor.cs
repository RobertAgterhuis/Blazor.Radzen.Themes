using Agterhuis.Ui.Designer.Commands;
using Agterhuis.Ui.Designer.Export;
using Agterhuis.Ui.Designer.Model;
using Agterhuis.Ui.Designer.Persistence;
using Agterhuis.Ui.Designer.Registry;
using Agterhuis.Ui.Designer.Serialization;
using Agterhuis.Ui.Services;
using Agterhuis.Ui.Theming;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Radzen;

namespace Agterhuis.Ui.Designer.Components;

public partial class DesignerShell : IDisposable
{
    private const string LocalStorageIndexKey = "agt-designer-documents";
    private const string LocalStoragePrefix = "agt-designer-document-";
    private const string LocalStorageDraftPrefix = "agt-designer-draft-";
    private const string CommandScope = "designer";

    private readonly ProjectExporter _projectExporter = new();
    private readonly DesignDocumentCommandStack _commands;
    private readonly IReadOnlyDictionary<string, int> _viewportWidths = new Dictionary<string, int>(StringComparer.Ordinal)
    {
        ["mobile"] = 360,
        ["tablet"] = 768,
        ["desktop"] = 1200
    };

    private ElementReference _canvasRef;
    private DesignerDragPayload? _activeDrag;
    private string? _selectedNodeId;
    private int _selectedPageIndex;
    private string _paletteFilter = string.Empty;
    private string _canvasTheme;
    private string _viewport;
    private string? _selectedSavedName;
    private string? _selectedEntityName;
    private DesignDocumentTemplateKind _selectedTemplateKind = DesignDocumentTemplateKind.FormPage;
    private List<string> _savedDocumentNames = [];
    private bool _hasRecoveredDraft;
    private string? _editingPageTitle;
    private int? _editingPageIndex;
    private int? _draggedPageIndex;

    public DesignerShell()
    {
        _canvasTheme = DefaultCanvasTheme ?? "plum-dark";
        _viewport = string.IsNullOrWhiteSpace(DefaultViewport) || !_viewportWidths.ContainsKey(DefaultViewport) ? "desktop" : DefaultViewport;
        _commands = new DesignDocumentCommandStack(CreateNewDocument("Untitled", DesignDocumentTemplateKind.Blank));
        _selectedPageIndex = 0;
    }

    [Parameter, EditorRequired]
    public IDesignStore Store { get; set; } = default!;

    [Parameter]
    public DesignerComponentRegistry Registry { get; set; } = DesignerComponentRegistry.Instance;

    [Parameter]
    public string? DefaultCanvasTheme { get; set; }

    [Parameter]
    public string? DefaultViewport { get; set; }

    [Parameter]
    public EventCallback<string> CanvasThemeChanged { get; set; }

    [Parameter]
    public EventCallback<string> SelectedEntityNameChanged { get; set; }

    [Inject]
    public IJSRuntime JS { get; set; } = default!;

    [Inject]
    public IAgtCommandRegistry CommandRegistry { get; set; } = default!;

    [Inject]
    public IAgtConfirmDialog ConfirmDialog { get; set; } = default!;

    [SupplyParameterFromQuery(Name = "template")]
    public string? TemplateQuery { get; set; }

    [SupplyParameterFromQuery(Name = "name")]
    public string? NameQuery { get; set; }

    private IReadOnlyList<string> SavedDocumentNames => _savedDocumentNames;
    private IReadOnlyList<string> CanvasThemeOptions => AgtTheme.All.SelectMany(static theme => new[] { theme.LightVariantId, theme.DarkVariantId }).OrderBy(static id => id, StringComparer.Ordinal).ToArray();
    private IReadOnlyList<DesignDocumentTemplates.TemplateDefinition> TemplateOptions => DesignDocumentTemplates.DefinitionsList;
    private IReadOnlyDictionary<string, IReadOnlyList<DesignerComponentDescriptor>> PaletteByCategory => Registry.Components.Where(static descriptor => descriptor.AllowedInPalette).Where(DescriptorMatchesFilter).GroupBy(static descriptor => descriptor.Category, StringComparer.Ordinal).OrderBy(static group => group.Key, StringComparer.Ordinal).ToDictionary(static group => group.Key, static group => (IReadOnlyList<DesignerComponentDescriptor>)group.OrderBy(static descriptor => descriptor.DisplayName, StringComparer.Ordinal).ToArray(), StringComparer.Ordinal);
    private DesignPage ActivePage => _commands.Document.Pages.Count == 0 ? new DesignPage { Route = "/", Title = "Nieuwe pagina" } : _commands.Document.Pages[Math.Clamp(_selectedPageIndex, 0, _commands.Document.Pages.Count - 1)];
    private int ActivePageIndex => _commands.Document.Pages.Count == 0 ? 0 : Math.Clamp(_selectedPageIndex, 0, _commands.Document.Pages.Count - 1);
    private IReadOnlyList<DesignerTreeNode> TreeRoots => BuildTree(ActivePage.Nodes);
    private DesignNode? SelectedNode => _selectedNodeId is null ? null : TryFindNode(ActivePage.Nodes, _selectedNodeId, out _, out var container, out var index) ? container[index] : null;
    private DesignerComponentDescriptor? SelectedDescriptor => SelectedNode is null ? null : Registry.TryGetDescriptor(SelectedNode.ComponentType, out var descriptor) ? descriptor : null;
    private string SelectionBreadcrumb => BuildBreadcrumb(_selectedNodeId) ?? "Selecteer een node";
    private string CanvasFrameStyle => _viewportWidths.TryGetValue(_viewport, out var width) ? $"max-width: {width}px;" : "max-width: 1200px;";
    private bool HasDuplicateActiveRoute => _commands.Document.Pages.Count(page => string.Equals(page.Route, ActivePage.Route, StringComparison.OrdinalIgnoreCase)) > 1;
    private string RoutePreview => string.Join(" | ", _commands.Document.Pages.Select(static page => string.IsNullOrWhiteSpace(page.Route) ? "/" : page.Route));

    protected override async Task OnInitializedAsync()
    {
        RegisterCommands();
        await LoadInitialDocumentAsync();
        EnsureSelectedPageIndex();
        await RestoreDocumentsFromStorageAsync();
        _hasRecoveredDraft = !string.IsNullOrWhiteSpace(await JS.InvokeAsync<string>("designerInterop.getText", LocalStorageDraftPrefix + _commands.Document.Name));
        await JS.InvokeVoidAsync("designerInterop.setupResizablePanels");
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        CommandRegistry.RemoveScope(CommandScope);
    }

    private async Task RestoreDocumentsFromStorageAsync()
    {
        _savedDocumentNames = (await Store.GetRecentNamesAsync()).ToList();
        _savedDocumentNames = _savedDocumentNames.Where(static name => !string.IsNullOrWhiteSpace(name)).Distinct(StringComparer.Ordinal).OrderBy(static name => name, StringComparer.Ordinal).ToList();
    }

    private void OnPaletteFilterChanged(ChangeEventArgs args) => _paletteFilter = args.Value?.ToString() ?? string.Empty;
    private void OnPaletteDragStart(string componentType) => _activeDrag = DesignerDragPayload.Palette(componentType);
    private Task OnDragStart(DesignerDragPayload payload) { _activeDrag = payload; return Task.CompletedTask; }
    private Task OnDragEnd() { _activeDrag = null; return Task.CompletedTask; }
    private void OnDropZoneDragOver(DragEventArgs args) { }

    private async Task OnDropRequested(DesignerDropTarget target)
    {
        if (_activeDrag is null)
        {
            return;
        }

        var didMutate = false;
        if (string.Equals(_activeDrag.Kind, "palette", StringComparison.Ordinal))
        {
            didMutate = AddFromPalette(target.Location, _activeDrag.Value);
        }
        else if (string.Equals(_activeDrag.Kind, "node", StringComparison.Ordinal))
        {
            didMutate = _commands.Execute(new MoveNodeCommand(ActivePageIndex, _activeDrag.Value, target.Location));
        }

        _activeDrag = null;
        if (didMutate)
        {
            await AutoSaveAsync();
            await InvokeAsync(StateHasChanged);
        }
    }

    private bool AddFromPalette(DesignNodeLocation location, string componentType)
    {
        if (!Registry.TryGetDescriptor(componentType, out var descriptor))
        {
            return false;
        }

        var node = CreateNodeForDescriptor(descriptor);
        var added = _commands.Execute(new AddNodeCommand(ActivePageIndex, location, node));
        if (added)
        {
            _selectedNodeId = node.Id;
        }

        return added;
    }

    private async Task OnSelectNode(string nodeId)
    {
        _selectedNodeId = nodeId;
        await InvokeAsync(StateHasChanged);
    }

    [JSInvokable]
    public async Task OnJavaScriptDrop(string componentType, int dropzoneIndex)
    {
        var didMutate = AddFromPalette(DesignNodeLocation.Root(dropzoneIndex), componentType);
        if (didMutate)
        {
            await AutoSaveAsync();
            await InvokeAsync(StateHasChanged);
        }
    }

    private void OnTreeChange(TreeEventArgs args)
    {
        if (args.Value is string nodeId)
        {
            _selectedNodeId = nodeId;
        }
    }

    private async Task OnUndo()
    {
        if (_commands.Undo())
        {
            EnsureSelectedPageIndex();
            ClearSelectionIfMissing();
            await AutoSaveAsync();
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task OnRedo()
    {
        if (_commands.Redo())
        {
            EnsureSelectedPageIndex();
            ClearSelectionIfMissing();
            await AutoSaveAsync();
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task OnSaveDocument()
    {
        var name = _commands.Document.Name;
        var json = DesignDocumentSerializer.Serialize(_commands.Document);
        await JS.InvokeVoidAsync("designerInterop.saveDesignDocument", $"{name}.agtdesign", json);
        await Store.SaveAsync(name, _commands.Document);
        await JS.InvokeVoidAsync("designerInterop.removeItem", LocalStorageDraftPrefix + name);

        if (!_savedDocumentNames.Contains(name, StringComparer.Ordinal))
        {
            _savedDocumentNames.Add(name);
            _savedDocumentNames = _savedDocumentNames.OrderBy(static item => item, StringComparer.Ordinal).ToList();
            await JS.InvokeVoidAsync("designerInterop.setJson", LocalStorageIndexKey, _savedDocumentNames);
        }

        _commands.MarkSaved();
        _hasRecoveredDraft = false;
        await InvokeAsync(StateHasChanged);
    }

    private async Task OnExportDocument()
    {
        var result = _projectExporter.ExportProject(_commands.Document, _commands.Document.Name, _canvasTheme.Split('-')[0]);
        await JS.InvokeVoidAsync("designerInterop.saveBytesFile", $"{_commands.Document.Name}.zip", "application/zip", result.ZipData);
    }

    private async Task OnOpenDocument()
    {
        var json = await JS.InvokeAsync<string?>("designerInterop.pickDesignDocument");
        if (!string.IsNullOrWhiteSpace(json))
        {
            ApplyLoadedDocument(json, markSaved: true);
        }
    }

    private async Task OnDesignFileChanged(InputFileChangeEventArgs args)
    {
        if (args.FileCount == 0)
        {
            return;
        }

        var file = args.File;
        using var stream = file.OpenReadStream(maxAllowedSize: 2_000_000);
        using var reader = new StreamReader(stream, System.Text.Encoding.UTF8);
        var json = await reader.ReadToEndAsync();
        ApplyLoadedDocument(json, markSaved: true);
    }

    private async Task OnSavedSelectionChanged(object value)
    {
        var selected = value?.ToString();
        if (string.IsNullOrWhiteSpace(selected))
        {
            return;
        }

        _selectedSavedName = selected;
        var json = await Store.LoadAsync(selected);
        if (!string.IsNullOrWhiteSpace(json))
        {
            ApplyLoadedDocument(json, markSaved: true);
        }
    }

    private Task OnSelectedEntityChanged(string value) => SelectedEntityNameChanged.InvokeAsync(value);

    private async Task OnGenerateFormRequested(string entityName)
    {
        var entity = _commands.Document.DataModel.Entities.FirstOrDefault(candidate => string.Equals(candidate.Name, entityName, StringComparison.Ordinal));
        if (entity is null)
        {
            return;
        }

        var containerLocation = ResolveFormInsertLocation();
        var insertIndex = containerLocation.Index;
        foreach (var node in BuildFormNodes(entity))
        {
            if (!_commands.Execute(new AddNodeCommand(ActivePageIndex, containerLocation with { Index = insertIndex++ }, node)))
            {
                return;
            }
        }

        _hasRecoveredDraft = true;
        await AutoSaveAsync();
        await InvokeAsync(StateHasChanged);
    }

    private async Task OnNewDocument()
    {
        var name = $"Document-{DateTime.UtcNow:yyyyMMdd-HHmmss}";
        _commands.ReplaceDocument(CreateNewDocument(name, _selectedTemplateKind), markSaved: true);
        _selectedEntityName = _commands.Document.DataModel.Entities.FirstOrDefault()?.Name;
        _selectedNodeId = null;
        _selectedPageIndex = 0;
        _hasRecoveredDraft = false;
        await AutoSaveAsync();
        await InvokeAsync(StateHasChanged);
    }

    private async Task LoadInitialDocumentAsync()
    {
        if (!string.IsNullOrWhiteSpace(NameQuery))
        {
            var json = await Store.LoadAsync(NameQuery);
            if (!string.IsNullOrWhiteSpace(json))
            {
                ApplyLoadedDocument(json, markSaved: true);
                return;
            }
        }

        if (!string.IsNullOrWhiteSpace(TemplateQuery) && Enum.TryParse<DesignDocumentTemplateKind>(TemplateQuery, out var templateKind))
        {
            _selectedTemplateKind = templateKind;
            _commands.ReplaceDocument(CreateNewDocument($"Document-{DateTime.UtcNow:yyyyMMdd-HHmmss}", templateKind), markSaved: true);
            _selectedEntityName = _commands.Document.DataModel.Entities.FirstOrDefault()?.Name;
            _selectedNodeId = null;
            _selectedPageIndex = 0;
            _hasRecoveredDraft = false;
        }
    }

    private void ApplyLoadedDocument(string json, bool markSaved)
    {
        var document = DesignDocumentSerializer.Deserialize(json);
        _commands.ReplaceDocument(document, markSaved);
        _selectedEntityName = document.DataModel.Entities.FirstOrDefault()?.Name;
        _selectedNodeId = null;
        _selectedPageIndex = 0;
        _editingPageIndex = null;
        _editingPageTitle = null;
        _hasRecoveredDraft = false;
        EnsureSelectedPageIndex();
        StateHasChanged();
    }

    private Task OnTemplateChanged(object value)
    {
        if (value is DesignDocumentTemplateKind kind)
        {
            _selectedTemplateKind = kind;
        }

        return Task.CompletedTask;
    }

    private Task OnCanvasThemeChanged(string value)
    {
        _canvasTheme = string.IsNullOrWhiteSpace(value) ? "plum-dark" : value;
        return CanvasThemeChanged.InvokeAsync(_canvasTheme);
    }

    private Task OnCanvasThemeChanged(object value) => OnCanvasThemeChanged(value?.ToString() ?? "plum-dark");

    private async Task OnPageRouteChanged(string? value)
    {
        if (_commands.Execute(new SetPagePropertyCommand(ActivePageIndex, nameof(DesignPage.Route), value)))
        {
            _hasRecoveredDraft = true;
            await AutoSaveAsync();
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task OnPageTitleChanged(string? value)
    {
        if (_commands.Execute(new SetPagePropertyCommand(ActivePageIndex, nameof(DesignPage.Title), value)))
        {
            _hasRecoveredDraft = true;
            await AutoSaveAsync();
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task OnNodeParameterChanged((ComponentParameterDescriptor Parameter, DesignParameterValue? Value) args)
    {
        if (_selectedNodeId is not null && _commands.Execute(new SetNodeParameterCommand(ActivePageIndex, _selectedNodeId, args.Parameter.Name, args.Value)))
        {
            _hasRecoveredDraft = true;
            await AutoSaveAsync();
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task OnDocumentChanged(DesignDocument document)
    {
        _commands.ReplaceDocument(document, markSaved: false);
        _selectedEntityName = _commands.Document.DataModel.Entities.FirstOrDefault()?.Name;
        EnsureSelectedPageIndex();
        ClearSelectionIfMissing();
        _hasRecoveredDraft = true;
        await AutoSaveAsync();
        await InvokeAsync(StateHasChanged);
    }

    private async Task OnNodeLayoutSlotChanged(DesignLayoutSlot layoutSlot)
    {
        if (_selectedNodeId is not null && _commands.Execute(new SetNodeLayoutSlotCommand(ActivePageIndex, _selectedNodeId, layoutSlot)))
        {
            _hasRecoveredDraft = true;
            await AutoSaveAsync();
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task OnAddColumnNode()
    {
        if (SelectedNode is null || _selectedNodeId is null || SelectedDescriptor?.Slots.Contains("Columns", StringComparer.Ordinal) != true)
        {
            return;
        }

        var descriptor = Registry.TryGetDescriptor("RadzenDataGridColumn", out var gridColumnDescriptor) ? gridColumnDescriptor : Registry.GetDescriptor("RadzenDataGridColumn");
        var node = CreateNodeForDescriptor(descriptor);
        node.Parameters["Title"] = DesignParameterValue.FromValue($"Kolom {GetColumnsInsertIndex(_selectedNodeId) + 1}");
        node.Parameters["Property"] = DesignParameterValue.FromValue("Dossiernummer");

        if (_commands.Execute(new AddNodeCommand(ActivePageIndex, new DesignNodeLocation(_selectedNodeId, "Columns", GetColumnsInsertIndex(_selectedNodeId)), node)))
        {
            _hasRecoveredDraft = true;
            await AutoSaveAsync();
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task OnRemoveColumnNode(string nodeId)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
        {
            return;
        }

        if (_commands.Execute(new RemoveNodeCommand(ActivePageIndex, nodeId)))
        {
            if (string.Equals(_selectedNodeId, nodeId, StringComparison.Ordinal))
            {
                _selectedNodeId = null;
            }

            _hasRecoveredDraft = true;
            await AutoSaveAsync();
            await InvokeAsync(StateHasChanged);
        }
    }

    private void SetViewport(string viewport) => _viewport = _viewportWidths.ContainsKey(viewport) ? viewport : "desktop";

    private async Task OnPageKeyDown(KeyboardEventArgs args)
    {
        if (args.CtrlKey && string.Equals(args.Key, "z", StringComparison.OrdinalIgnoreCase))
        {
            await OnUndo();
            return;
        }

        if (args.CtrlKey && string.Equals(args.Key, "y", StringComparison.OrdinalIgnoreCase))
        {
            await OnRedo();
            return;
        }

        if (string.Equals(args.Key, "Escape", StringComparison.OrdinalIgnoreCase))
        {
            _selectedNodeId = null;
            return;
        }

        if (string.Equals(args.Key, "Delete", StringComparison.OrdinalIgnoreCase) && _selectedNodeId is not null && _commands.Execute(new RemoveNodeCommand(ActivePageIndex, _selectedNodeId)))
        {
            _selectedNodeId = null;
            _hasRecoveredDraft = true;
            await AutoSaveAsync();
            await InvokeAsync(StateHasChanged);
            return;
        }

        if (_selectedNodeId is null)
        {
            return;
        }

        if (args.CtrlKey && string.Equals(args.Key, "ArrowUp", StringComparison.OrdinalIgnoreCase) && _commands.Execute(new ReorderSiblingCommand(ActivePageIndex, _selectedNodeId, -1)))
        {
            _hasRecoveredDraft = true;
            await AutoSaveAsync();
            await InvokeAsync(StateHasChanged);
            return;
        }

        if (args.CtrlKey && string.Equals(args.Key, "ArrowDown", StringComparison.OrdinalIgnoreCase) && _commands.Execute(new ReorderSiblingCommand(ActivePageIndex, _selectedNodeId, 1)))
        {
            _hasRecoveredDraft = true;
            await AutoSaveAsync();
            await InvokeAsync(StateHasChanged);
            return;
        }

        if (string.Equals(args.Key, "ArrowUp", StringComparison.OrdinalIgnoreCase))
        {
            SelectSibling(-1);
            return;
        }

        if (string.Equals(args.Key, "ArrowDown", StringComparison.OrdinalIgnoreCase))
        {
            SelectSibling(1);
        }
    }

    [JSInvokable]
    public Task HandleCanvasKey(string key)
    {
        if (string.Equals(key, "Escape", StringComparison.OrdinalIgnoreCase))
        {
            _selectedNodeId = null;
            StateHasChanged();
        }

        return Task.CompletedTask;
    }

    private void SelectSibling(int delta)
    {
        if (_selectedNodeId is null || !TryFindNode(ActivePage.Nodes, _selectedNodeId, out _, out var container, out var index))
        {
            return;
        }

        var nextIndex = index + delta;
        if (nextIndex < 0 || nextIndex >= container.Count)
        {
            return;
        }

        _selectedNodeId = container[nextIndex].Id;
        StateHasChanged();
    }

    private async Task AutoSaveAsync()
    {
        await JS.InvokeVoidAsync("designerInterop.setJson", LocalStoragePrefix + _commands.Document.Name, DesignDocumentSerializer.Serialize(_commands.Document));
        await JS.InvokeVoidAsync("designerInterop.setJson", LocalStorageDraftPrefix + _commands.Document.Name, DesignDocumentSerializer.Serialize(_commands.Document));
    }

    private bool DescriptorMatchesFilter(DesignerComponentDescriptor descriptor)
    {
        if (string.IsNullOrWhiteSpace(_paletteFilter))
        {
            return true;
        }

        return descriptor.DisplayName.Contains(_paletteFilter, StringComparison.OrdinalIgnoreCase)
            || descriptor.ComponentType.Contains(_paletteFilter, StringComparison.OrdinalIgnoreCase)
            || descriptor.Category.Contains(_paletteFilter, StringComparison.OrdinalIgnoreCase);
    }

    private static DesignDocument CreateNewDocument(string name, DesignDocumentTemplateKind templateKind)
    {
        var template = DesignDocumentTemplates.Create(templateKind, name);

        if (template.Pages.Count > 0)
        {
            return template;
        }

        var root = new DesignNode
        {
            ComponentType = "RadzenRow",
            Parameters = new Dictionary<string, DesignParameterValue>(StringComparer.Ordinal)
            {
                ["Gap"] = DesignParameterValue.FromValue("1rem")
            },
            Children = new Dictionary<string, List<DesignNode>>(StringComparer.Ordinal)
            {
                ["ChildContent"] =
                [
                    new DesignNode
                    {
                        ComponentType = "RadzenColumn",
                        Parameters = new Dictionary<string, DesignParameterValue>(StringComparer.Ordinal)
                        {
                            ["Size"] = DesignParameterValue.FromValue(12)
                        },
                        Children = new Dictionary<string, List<DesignNode>>(StringComparer.Ordinal)
                        {
                            ["ChildContent"] = []
                        }
                    }
                ]
            }
        };

        var document = new DesignDocument
        {
            Name = name,
            Version = "1.0",
            Pages =
            [
                new DesignPage
                {
                    Route = "/",
                    Title = "Nieuw ontwerp",
                    Nodes = [root]
                }
            ]
        };

        return DesignDocumentMigrator.Migrate(document);
    }

    private void RegisterCommands()
    {
        CommandRegistry.SetCommands(CommandScope, new[]
        {
            new AgtCommandItem("designer-add-page", "Pagina toevoegen", "Designer", OnAddPageAsync)
            {
                Description = "Voeg een nieuwe pagina toe vanuit het gekozen patroon.",
                ShortcutHint = "Ctrl+Shift+P"
            },
            new AgtCommandItem("designer-insert-text", "Tekstveld invoegen", "Designer", OnInsertTextFieldAsync)
            {
                Description = "Voeg een tekstveld toe aan de geselecteerde container of de root.",
                ShortcutHint = "Ctrl+K"
            },
            new AgtCommandItem("designer-export", "Exporteren", "Designer", OnExportDocument)
            {
                Description = "Download een geexporteerd projectpakket.",
                ShortcutHint = "Ctrl+E"
            },
            new AgtCommandItem("designer-undo", "Ongedaan maken", "Designer", OnUndo)
            {
                Description = "Maak de laatste ontwerpwijziging ongedaan.",
                ShortcutHint = "Ctrl+Z"
            }
        });
    }

    private async Task OnAddPageAsync()
    {
        var route = GenerateUniqueRoute();
        var title = $"Pagina {_commands.Document.Pages.Count + 1}";
        var page = DesignDocumentTemplates.Create(_selectedTemplateKind, title).Pages.First();
        page.Route = route;
        page.Title = title;

        if (_commands.Execute(new AddPageCommand(page)))
        {
            _selectedPageIndex = _commands.Document.Pages.Count - 1;
            _selectedNodeId = null;
            _hasRecoveredDraft = true;
            await AutoSaveAsync();
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task OnInsertTextFieldAsync()
    {
        var targetLocation = _selectedNodeId is null ? DesignNodeLocation.Root(ActivePage.Nodes.Count) : new DesignNodeLocation(_selectedNodeId, "ChildContent", GetChildInsertIndex(_selectedNodeId));
        var descriptor = Registry.GetDescriptor("AgtTextField");
        var node = CreateNodeForDescriptor(descriptor);
        if (_commands.Execute(new AddNodeCommand(ActivePageIndex, targetLocation, node)))
        {
            _selectedNodeId = node.Id;
            _hasRecoveredDraft = true;
            await AutoSaveAsync();
            await InvokeAsync(StateHasChanged);
        }
    }

    private int GetChildInsertIndex(string parentNodeId) => TryFindNode(ActivePage.Nodes, parentNodeId, out _, out var container, out _) ? container.Count : 0;

    private int GetColumnsInsertIndex(string parentNodeId)
    {
        if (!TryFindNode(ActivePage.Nodes, parentNodeId, out _, out var container, out var index))
        {
            return 0;
        }

        var parent = container[index];
        return parent.Children.TryGetValue("Columns", out var columns) ? columns.Count : 0;
    }

    private DesignNodeLocation ResolveFormInsertLocation() => _selectedNodeId is not null && SelectedDescriptor?.Slots.Contains("ChildContent", StringComparer.Ordinal) == true ? new DesignNodeLocation(_selectedNodeId, "ChildContent", GetChildInsertIndex(_selectedNodeId)) : DesignNodeLocation.Root(ActivePage.Nodes.Count);

    private IReadOnlyList<DesignNode> BuildFormNodes(DesignEntity entity)
    {
        var card = CreateNodeForDescriptor(Registry.GetDescriptor("AgtCard"));
        card.Children["ChildContent"] = entity.Fields.Select(BuildNodeForField).ToList();
        card.Children["ChildContent"].Add(CreateFormActionsNode());
        return [card];
    }

    private DesignNode BuildNodeForField(DesignField field)
    {
        var componentType = field.Type switch
        {
            DesignFieldType.Int or DesignFieldType.Decimal => "AgtNumericField",
            DesignFieldType.Bool => "AgtSwitch",
            DesignFieldType.DateTime => "AgtDatePicker",
            DesignFieldType.Enum => "AgtDropdown",
            _ => "AgtTextField"
        };

        var descriptor = Registry.GetDescriptor(componentType);
        var node = CreateNodeForDescriptor(descriptor);
        node.Parameters["Label"] = DesignParameterValue.FromValue(field.Name);
        node.Parameters["AriaLabel"] = DesignParameterValue.FromValue(field.Name);
        if (field.Type == DesignFieldType.Enum)
        {
            node.Parameters["Placeholder"] = DesignParameterValue.FromValue($"Kies {field.Name.ToLowerInvariant()}");
        }

        return node;
    }

    private DesignNode CreateFormActionsNode()
    {
        var node = CreateNodeForDescriptor(Registry.GetDescriptor("AgtFormActions"));
        node.Parameters["SaveText"] = DesignParameterValue.FromValue("Opslaan");
        node.Parameters["CancelText"] = DesignParameterValue.FromValue("Annuleren");
        return node;
    }

    private DesignNode CreateNodeForDescriptor(DesignerComponentDescriptor descriptor)
    {
        var node = new DesignNode
        {
            ComponentType = descriptor.ComponentType,
            Parameters = new Dictionary<string, DesignParameterValue>(StringComparer.Ordinal),
            Children = new Dictionary<string, List<DesignNode>>(StringComparer.Ordinal)
        };

        foreach (var slot in descriptor.Slots)
        {
            node.Children[slot] = [];
        }

        if (descriptor.Parameters.Any(static parameter => string.Equals(parameter.Name, "Label", StringComparison.Ordinal)))
        {
            node.Parameters["Label"] = DesignParameterValue.FromValue(descriptor.DisplayName);
        }

        if (descriptor.Parameters.Any(static parameter => string.Equals(parameter.Name, "AriaLabel", StringComparison.Ordinal)))
        {
            node.Parameters["AriaLabel"] = DesignParameterValue.FromValue(descriptor.DisplayName);
        }

        var document = DesignDocumentMigrator.Migrate(new DesignDocument
        {
            Pages =
            [
                new DesignPage
                {
                    Route = "/",
                    Title = "seed",
                    Nodes = [node]
                }
            ]
        });

        return document.Pages.First().Nodes.First();
    }

    private string? BuildBreadcrumb(string? nodeId)
    {
        if (string.IsNullOrWhiteSpace(nodeId) || !TryFindNode(ActivePage.Nodes, nodeId, out var path, out _, out _))
        {
            return null;
        }

        return string.Join(" > ", path.Select(static node => node.ComponentType));
    }

    private static IReadOnlyList<DesignerTreeNode> BuildTree(IReadOnlyList<DesignNode> nodes) => nodes.Select(BuildTreeNode).ToArray();

    private static DesignerTreeNode BuildTreeNode(DesignNode node) => new(node.Id, node.ComponentType, node.Children.OrderBy(static pair => pair.Key, StringComparer.Ordinal).SelectMany(static pair => pair.Value).Select(BuildTreeNode).ToArray());

    private static bool TryFindNode(IReadOnlyList<DesignNode> nodes, string targetId, out List<DesignNode> path, out List<DesignNode> container, out int index)
    {
        var mutable = nodes as List<DesignNode> ?? nodes.ToList();
        for (var i = 0; i < mutable.Count; i++)
        {
            if (TryFindNodeRecursive(mutable[i], mutable, i, targetId, [], out path, out container, out index))
            {
                return true;
            }
        }

        path = [];
        container = [];
        index = -1;
        return false;
    }

    private static bool TryFindNodeRecursive(DesignNode node, List<DesignNode> container, int index, string targetId, List<DesignNode> ancestry, out List<DesignNode> path, out List<DesignNode> resultContainer, out int resultIndex)
    {
        var nextAncestry = new List<DesignNode>(ancestry) { node };
        if (string.Equals(node.Id, targetId, StringComparison.Ordinal))
        {
            path = nextAncestry;
            resultContainer = container;
            resultIndex = index;
            return true;
        }

        foreach (var slot in node.Children.Values)
        {
            for (var childIndex = 0; childIndex < slot.Count; childIndex++)
            {
                if (TryFindNodeRecursive(slot[childIndex], slot, childIndex, targetId, nextAncestry, out path, out resultContainer, out resultIndex))
                {
                    return true;
                }
            }
        }

        path = [];
        resultContainer = [];
        resultIndex = -1;
        return false;
    }

    private void EnsureSelectedPageIndex()
    {
        if (_commands.Document.Pages.Count == 0)
        {
            _selectedPageIndex = 0;
            return;
        }

        _selectedPageIndex = Math.Clamp(_selectedPageIndex, 0, _commands.Document.Pages.Count - 1);
    }

    private void ClearSelectionIfMissing()
    {
        if (_selectedNodeId is not null && !TryFindNode(ActivePage.Nodes, _selectedNodeId, out _, out _, out _))
        {
            _selectedNodeId = null;
        }
    }

    private string GenerateUniqueRoute()
    {
        var usedRoutes = _commands.Document.Pages.Select(static page => page.Route).Where(static route => !string.IsNullOrWhiteSpace(route)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var pageNumber = _commands.Document.Pages.Count + 1;

        while (true)
        {
            var candidate = $"/page-{pageNumber}";
            if (!usedRoutes.Contains(candidate))
            {
                return candidate;
            }

            pageNumber++;
        }
    }

    private string GenerateUniqueRouteFromRoute(string route)
    {
        var normalized = string.IsNullOrWhiteSpace(route) ? "/page" : route.Trim();
        if (!normalized.StartsWith("/", StringComparison.Ordinal))
        {
            normalized = "/" + normalized;
        }

        var usedRoutes = _commands.Document.Pages.Select(static page => page.Route).Where(static pageRoute => !string.IsNullOrWhiteSpace(pageRoute)).ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (!usedRoutes.Contains(normalized))
        {
            return normalized;
        }

        for (var index = 2; index < 5000; index++)
        {
            var candidate = $"{normalized}-{index}";
            if (!usedRoutes.Contains(candidate))
            {
                return candidate;
            }
        }

        return GenerateUniqueRoute();
    }

    private string GetPageLabel(DesignPage page)
        => string.IsNullOrWhiteSpace(page.Title) ? (string.IsNullOrWhiteSpace(page.Route) ? "Nieuwe pagina" : page.Route) : page.Title;

    private void SelectPage(int index)
    {
        _selectedPageIndex = index;
        _selectedNodeId = null;
        _editingPageIndex = null;
        _editingPageTitle = null;
        EnsureSelectedPageIndex();
    }

    private void BeginRenamePage(int index)
    {
        if (index < 0 || index >= _commands.Document.Pages.Count)
        {
            return;
        }

        _editingPageIndex = index;
        _editingPageTitle = _commands.Document.Pages[index].Title;
    }

    private async Task SaveRenamePageAsync(int index)
    {
        if (_editingPageIndex != index)
        {
            return;
        }

        var title = _editingPageTitle?.Trim();
        _editingPageIndex = null;
        _editingPageTitle = null;

        if (string.IsNullOrWhiteSpace(title))
        {
            return;
        }

        if (_commands.Execute(new RenamePageCommand(index, title)))
        {
            _hasRecoveredDraft = true;
            await AutoSaveAsync();
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task CancelRenamePageAsync(int index)
    {
        if (_editingPageIndex != index)
        {
            return;
        }

        _editingPageIndex = null;
        _editingPageTitle = null;
        await InvokeAsync(StateHasChanged);
    }

    private async Task OnRenameKeyDown(KeyboardEventArgs args, int index)
    {
        if (string.Equals(args.Key, "Enter", StringComparison.OrdinalIgnoreCase))
        {
            await SaveRenamePageAsync(index);
            return;
        }

        if (string.Equals(args.Key, "Escape", StringComparison.OrdinalIgnoreCase))
        {
            await CancelRenamePageAsync(index);
        }
    }

    private async Task DuplicatePageAsync(int index)
    {
        if (index < 0 || index >= _commands.Document.Pages.Count)
        {
            return;
        }

        var source = _commands.Document.Pages[index];
        var route = GenerateUniqueRouteFromRoute(source.Route);
        var title = string.IsNullOrWhiteSpace(source.Title) ? "Gekopieerde pagina" : source.Title + " kopie";

        if (_commands.Execute(new DuplicatePageCommand(index, route, title)))
        {
            _selectedPageIndex = Math.Clamp(index + 1, 0, _commands.Document.Pages.Count - 1);
            _selectedNodeId = null;
            _hasRecoveredDraft = true;
            await AutoSaveAsync();
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task RemovePageAsync(int index)
    {
        if (_commands.Document.Pages.Count <= 1 || index < 0 || index >= _commands.Document.Pages.Count)
        {
            return;
        }

        var pageName = GetPageLabel(_commands.Document.Pages[index]);
        var confirmed = await ConfirmDialog.ConfirmDeleteAsync(pageName);
        if (!confirmed)
        {
            return;
        }

        var nextPageIndex = index == 0 ? 0 : index - 1;
        if (_commands.Execute(new RemovePageCommand(index)))
        {
            _selectedPageIndex = Math.Clamp(nextPageIndex, 0, _commands.Document.Pages.Count - 1);
            _selectedNodeId = null;
            _hasRecoveredDraft = true;
            await AutoSaveAsync();
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task MovePageByDeltaAsync(int index, int delta)
    {
        if (index < 0 || index >= _commands.Document.Pages.Count)
        {
            return;
        }

        var target = index + delta;
        if (target < 0 || target >= _commands.Document.Pages.Count)
        {
            return;
        }

        if (_commands.Execute(new ReorderPageCommand(index, target)))
        {
            _selectedPageIndex = target;
            _hasRecoveredDraft = true;
            await AutoSaveAsync();
            await InvokeAsync(StateHasChanged);
        }
    }

    private void OnPageDragStart(int index)
    {
        _draggedPageIndex = index;
    }

    private void OnPageDragOver(DragEventArgs args) { }

    private async Task OnPageDropAsync(int targetIndex)
    {
        if (_draggedPageIndex is null)
        {
            return;
        }

        var sourceIndex = _draggedPageIndex.Value;
        _draggedPageIndex = null;

        if (sourceIndex == targetIndex || targetIndex < 0 || targetIndex >= _commands.Document.Pages.Count)
        {
            return;
        }

        if (_commands.Execute(new ReorderPageCommand(sourceIndex, targetIndex)))
        {
            _selectedPageIndex = targetIndex;
            _hasRecoveredDraft = true;
            await AutoSaveAsync();
            await InvokeAsync(StateHasChanged);
        }
    }

    private void OnPageDragEnd()
    {
        _draggedPageIndex = null;
    }

    private sealed record DesignerTreeNode(string Id, string Text, IReadOnlyList<DesignerTreeNode> Children)
    {
        public string Value => Id;
    }
}
