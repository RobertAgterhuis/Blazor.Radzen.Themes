using Agterhuis.Ui.Designer.Commands;
using Agterhuis.Ui.Designer.Export;
using Agterhuis.Ui.Designer.Model;
using Agterhuis.Ui.Designer.Persistence;
using Agterhuis.Ui.Designer.Registry;
using Agterhuis.Ui.Designer.Serialization;
using Agterhuis.Ui.Designer.Validation;
using Agterhuis.Ui.Components.Feedback;
using Agterhuis.Ui.Services;
using Agterhuis.Ui.Theming;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Threading;
using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;

namespace Agterhuis.Ui.Designer.Components;

public partial class DesignerShell : IDisposable
{
    private const string LocalStorageDraftPrefix = "agt-designer-draft-";
    private const string LocalStorageDraftMetaPrefix = "agt-designer-draft-meta-";
    private const string LocalStorageLayoutKey = "agt-designer-layout";
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
    private List<DesignListItem> _savedDocuments = [];
    private bool _hasRecoveredDraft;
    private string? _offlineWarning;
    private bool _showConflictDialog;
    private DesignDocumentEnvelope? _conflictServerEnvelope;
    private bool _showVersionHistory;
    private List<DesignVersionInfo> _versionHistory = [];
    private int? _previewVersion;
    private DesignDocumentEnvelope? _previewEnvelope;
    private bool _showDraftRecoveryChoice;
    private string? _draftJsonCandidate;
    private string? _currentETag;
    private int _currentVersion;
    private DateTimeOffset _lastLocalAutosaveUtc = DateTimeOffset.MinValue;
    private DateTimeOffset _lastRemoteAutosaveUtc = DateTimeOffset.MinValue;
    private string? _editingPageTitle;
    private int? _editingPageIndex;
    private int? _draggedPageIndex;
    private List<DesignValidationError> _validationIssues = [];
    private bool _issuesExpanded;
    private bool _showErrorIssues = true;
    private bool _showWarningIssues = true;
    private bool _showInfoIssues = true;
    private CancellationTokenSource? _validationDebounceCts;
    private string _hardcodedColorFixToken = "var(--agt-color-primary-500)";
    private bool _paletteCollapsed;
    private bool _dataCollapsed = true;
    private bool _treeCollapsed = true;
    private bool _codeCollapsed = true;
    private bool _fileMenuOpen;
    private bool _settingsMenuOpen;
    private bool _showNewDocumentDialog;
    private int? _pageMenuIndex;
    private string? _hoverDropzoneId;
    private string? _uiFeedback;
    private string _liveAnnouncement = "Designer geladen.";

    public DesignerShell()
    {
        _canvasTheme = DefaultCanvasTheme ?? "plum-dark";
        _viewport = string.IsNullOrWhiteSpace(DefaultViewport) || !_viewportWidths.ContainsKey(DefaultViewport) ? "desktop" : DefaultViewport;
        _commands = new DesignDocumentCommandStack(CreateNewDocument("Untitled", DesignDocumentTemplateKind.Blank));
        _commands.DocumentChanged += OnCommandStackDocumentChanged;
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

    [Parameter]
    public EventCallback<IReadOnlyList<DesignValidationError>> ValidationIssuesChanged { get; set; }

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

    private IReadOnlyList<string> SavedDocumentNames => _savedDocuments.Select(static item => item.Name).ToArray();
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
    private IReadOnlyList<DesignValidationError> ValidationIssues => _validationIssues;
    private bool IsDragActive => _activeDrag is not null;
    private IReadOnlyList<DesignValidationError> FilteredIssues => _validationIssues
        .Where(issue => (issue.Severity != DesignValidationSeverity.Error || _showErrorIssues)
            && (issue.Severity != DesignValidationSeverity.Warning || _showWarningIssues)
            && (issue.Severity != DesignValidationSeverity.Info || _showInfoIssues))
        .OrderBy(static issue => issue.Severity)
        .ThenBy(static issue => issue.PageIndex ?? int.MaxValue)
        .ThenBy(static issue => issue.Path, StringComparer.Ordinal)
        .ToArray();
    private int ErrorCount => _validationIssues.Count(static issue => issue.Severity == DesignValidationSeverity.Error);
    private int WarningCount => _validationIssues.Count(static issue => issue.Severity == DesignValidationSeverity.Warning);
    private int InfoCount => _validationIssues.Count(static issue => issue.Severity == DesignValidationSeverity.Info);
    private bool HasErrorIssues => ErrorCount > 0;
    private bool HasWarningIssues => WarningCount > 0;
    private IReadOnlyList<TokenOption> HardcodedColorTokenOptions =>
    [
        new TokenOption("Primair 500", "var(--agt-color-primary-500)"),
        new TokenOption("Accent 400", "var(--agt-color-accent-400)"),
        new TokenOption("Text body", "var(--agt-text-body)"),
        new TokenOption("Surface 1", "var(--agt-surface-1)"),
        new TokenOption("Border", "var(--agt-input-border)")
    ];

    protected override async Task OnInitializedAsync()
    {
        RegisterCommands();
        await RestoreLayoutStateAsync();
        await LoadInitialDocumentAsync();
        EnsureSelectedPageIndex();
        await RestoreDocumentsFromStorageAsync();
        RefreshValidationIssues();
        await EvaluateStoreAvailabilityAsync();
        await EvaluateDraftRecoveryAsync();
        await JS.InvokeVoidAsync("designerInterop.setupResizablePanels");
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        _commands.DocumentChanged -= OnCommandStackDocumentChanged;
        _validationDebounceCts?.Cancel();
        _validationDebounceCts?.Dispose();
        CommandRegistry.RemoveScope(CommandScope);
    }

    private async Task RestoreDocumentsFromStorageAsync()
    {
        _savedDocuments = (await Store.GetRecentAsync())
            .Where(static item => !string.IsNullOrWhiteSpace(item.Name))
            .GroupBy(static item => item.Name, StringComparer.Ordinal)
            .Select(static group => group.OrderByDescending(item => item.LastModified).First())
            .OrderByDescending(static item => item.LastModified)
            .ThenBy(static item => item.Name, StringComparer.Ordinal)
            .ToList();
    }

    private void OnPaletteFilterChanged(ChangeEventArgs args) => _paletteFilter = args.Value?.ToString() ?? string.Empty;
    private void OnPaletteDragStart(string componentType) => _activeDrag = DesignerDragPayload.Palette(componentType);
    private Task OnDragStart(DesignerDragPayload payload)
    {
        _activeDrag = payload;
        _liveAnnouncement = "Sleepbewerking gestart.";
        return Task.CompletedTask;
    }

    private Task OnDragEnd()
    {
        _activeDrag = null;
        _hoverDropzoneId = null;
        return Task.CompletedTask;
    }

    private void OnDropZoneDragOver(DragEventArgs args, string dropzoneId)
    {
        _hoverDropzoneId = dropzoneId;
    }

    private void OnDropZoneDragEnter(string dropzoneId)
    {
        _hoverDropzoneId = dropzoneId;
    }

    private void OnDropZoneDragLeave(string dropzoneId)
    {
        if (string.Equals(_hoverDropzoneId, dropzoneId, StringComparison.Ordinal))
        {
            _hoverDropzoneId = null;
        }
    }

    private async Task OnDropRequested(DesignerDropTarget target, string? dropzoneId = null)
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
        _hoverDropzoneId = null;
        if (didMutate)
        {
            _liveAnnouncement = "Component geplaatst op canvas.";
            await AutoSaveAsync();
            await InvokeAsync(StateHasChanged);
        }
    }

    private Task OnDropRequested(DesignerDropTarget target)
        => OnDropRequested(target, null);

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
        _liveAnnouncement = "Node geselecteerd.";
        await InvokeAsync(StateHasChanged);
        await JS.InvokeVoidAsync("designerInterop.scrollTreeItemIntoView", nodeId);
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

    private async Task OnTreeNodeClicked(string nodeId)
    {
        _selectedNodeId = nodeId;
        _liveAnnouncement = "Node geselecteerd vanuit structuurboom.";
        await InvokeAsync(StateHasChanged);
        await JS.InvokeVoidAsync("designerInterop.scrollTreeItemIntoView", nodeId);
    }

    private async Task OnPaletteItemClickedAsync(string componentType)
    {
        var location = ResolvePaletteClickInsertLocation();
        if (!AddFromPalette(location, componentType))
        {
            return;
        }

        _uiFeedback = "Component toegevoegd.";
        _liveAnnouncement = "Component toegevoegd via klik.";
        await AutoSaveAsync();
        await InvokeAsync(StateHasChanged);
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
        try
        {
            var envelope = await Store.SaveAsync(name, _commands.Document, _currentETag);
            _currentETag = envelope.ETag;
            _currentVersion = envelope.Version;
            _offlineWarning = null;
        }
        catch (DesignConflictException)
        {
            _offlineWarning = null;
            _showConflictDialog = true;
            _conflictServerEnvelope = await Store.LoadAsync(name);
            await InvokeAsync(StateHasChanged);
            return;
        }
        catch
        {
            _offlineWarning = "Offline modus - wijzigingen worden lokaal opgeslagen.";
        }

        await JS.InvokeVoidAsync("designerInterop.removeItem", LocalStorageDraftPrefix + name);
        await JS.InvokeVoidAsync("designerInterop.removeItem", LocalStorageDraftMetaPrefix + name);
        await RestoreDocumentsFromStorageAsync();

        _commands.MarkSaved();
        _hasRecoveredDraft = false;
        _showDraftRecoveryChoice = false;
        await InvokeAsync(StateHasChanged);
    }

    private async Task OnExportDocument()
    {
        if (HasErrorIssues)
        {
            return;
        }

        if (HasWarningIssues)
        {
            var proceed = await ConfirmDialog.ConfirmAsync(
                $"Er zijn {WarningCount} waarschuwingen. Toch exporteren?",
                "Waarschuwingen gevonden",
                new AgtConfirmOptions
                {
                    OkText = "Toch exporteren",
                    CancelText = "Annuleren",
                    Intent = AgtIntent.Secondary
                });

            if (!proceed)
            {
                return;
            }
        }

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
        var envelope = await Store.LoadAsync(selected);
        if (envelope is not null)
        {
            ApplyLoadedDocument(DesignDocumentSerializer.Serialize(envelope.Document), markSaved: true);
            _currentETag = envelope.ETag;
            _currentVersion = envelope.Version;
            await EvaluateDraftRecoveryAsync();
        }
    }

    private Task OnSelectedEntityChanged(string value) => SelectedEntityNameChanged.InvokeAsync(value);

    private async Task OnImportEntitiesRequested((IReadOnlyList<DesignEntity> Entities, SchemaImportApplyOptions Options) args)
    {
        if (args.Entities.Count == 0)
        {
            return;
        }

        if (_commands.Execute(new ImportEntitiesCommand(args.Entities, args.Options)))
        {
            _selectedEntityName = _commands.Document.DataModel.Entities.FirstOrDefault()?.Name;
            _hasRecoveredDraft = true;
            await AutoSaveAsync();
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task OnGenerateFormRequested(string entityName)
    {
        var entity = _commands.Document.DataModel.Entities.FirstOrDefault(candidate => string.Equals(candidate.Name, entityName, StringComparison.Ordinal));
        if (entity is null)
        {
            return;
        }

        var selectedFields = DesignSchemaImporter.BuildDefaultFormSelection(entity)
            .Where(static item => item.Include)
            .ToArray();
        if (selectedFields.Length == 0)
        {
            return;
        }

        var containerLocation = ResolveFormInsertLocation();
        var insertIndex = containerLocation.Index;
        foreach (var node in BuildFormNodes(entity, selectedFields))
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

    private void OpenNewDocumentDialog()
    {
        _showNewDocumentDialog = true;
        _fileMenuOpen = false;
    }

    private void CloseNewDocumentDialog()
    {
        _showNewDocumentDialog = false;
    }

    private async Task CreateNewDocumentFromDialogAsync()
    {
        await OnNewDocument();
        _showNewDocumentDialog = false;
    }

    private async Task LoadInitialDocumentAsync()
    {
        if (!string.IsNullOrWhiteSpace(NameQuery))
        {
            var envelope = await Store.LoadAsync(NameQuery);
            if (envelope is not null)
            {
                ApplyLoadedDocument(DesignDocumentSerializer.Serialize(envelope.Document), markSaved: true);
                _currentETag = envelope.ETag;
                _currentVersion = envelope.Version;
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

    private async Task OpenVersionHistoryAsync()
    {
        _versionHistory = (await Store.GetVersionsAsync(_commands.Document.Name))
            .OrderByDescending(static version => version.Version)
            .ToList();
        _previewVersion = null;
        _previewEnvelope = null;
        _showVersionHistory = true;
        await InvokeAsync(StateHasChanged);
    }

    private void CloseVersionHistory()
    {
        _showVersionHistory = false;
        _previewVersion = null;
        _previewEnvelope = null;
    }

    private async Task PreviewVersionAsync(int version)
    {
        _previewEnvelope = await Store.LoadAsync(_commands.Document.Name, version);
        _previewVersion = version;
        await InvokeAsync(StateHasChanged);
    }

    private async Task RestoreVersionAsync(int version)
    {
        var restored = await Store.RestoreVersionAsync(_commands.Document.Name, version);
        if (restored is null)
        {
            return;
        }

        ApplyLoadedDocument(DesignDocumentSerializer.Serialize(restored.Document), markSaved: true);
        _currentETag = restored.ETag;
        _currentVersion = restored.Version;
        _showVersionHistory = false;
        _previewEnvelope = null;
        _previewVersion = null;
        await JS.InvokeVoidAsync("designerInterop.removeItem", LocalStorageDraftPrefix + _commands.Document.Name);
        await JS.InvokeVoidAsync("designerInterop.removeItem", LocalStorageDraftMetaPrefix + _commands.Document.Name);
        await RestoreDocumentsFromStorageAsync();
        await InvokeAsync(StateHasChanged);
    }

    private async Task SaveConflictMineAsync()
    {
        var envelope = await Store.SaveAsync(_commands.Document.Name, _commands.Document, expectedETag: null);
        _currentETag = envelope.ETag;
        _currentVersion = envelope.Version;
        _showConflictDialog = false;
        _conflictServerEnvelope = null;
        _commands.MarkSaved();
        await RestoreDocumentsFromStorageAsync();
        await InvokeAsync(StateHasChanged);
    }

    private async Task LoadConflictServerAsync()
    {
        var server = _conflictServerEnvelope ?? await Store.LoadAsync(_commands.Document.Name);
        if (server is not null)
        {
            ApplyLoadedDocument(DesignDocumentSerializer.Serialize(server.Document), markSaved: true);
            _currentETag = server.ETag;
            _currentVersion = server.Version;
        }

        _showConflictDialog = false;
        _conflictServerEnvelope = null;
        await InvokeAsync(StateHasChanged);
    }

    private void CancelConflictDialog()
    {
        _showConflictDialog = false;
        _conflictServerEnvelope = null;
    }

    private async Task EvaluateDraftRecoveryAsync()
    {
        var name = _commands.Document.Name;
        var draftJson = await JS.InvokeAsync<string?>("designerInterop.getText", LocalStorageDraftPrefix + name);
        if (string.IsNullOrWhiteSpace(draftJson))
        {
            _hasRecoveredDraft = false;
            _showDraftRecoveryChoice = false;
            _draftJsonCandidate = null;
            return;
        }

        var draftMetaJson = await JS.InvokeAsync<string?>("designerInterop.getText", LocalStorageDraftMetaPrefix + name);
        DateTimeOffset draftUpdated = DateTimeOffset.UtcNow;
        if (!string.IsNullOrWhiteSpace(draftMetaJson))
        {
            try
            {
                var meta = JsonSerializer.Deserialize<DraftMeta>(draftMetaJson);
                if (meta?.UpdatedUtc is DateTimeOffset parsed && parsed > DateTimeOffset.MinValue)
                {
                    draftUpdated = parsed;
                }
            }
            catch
            {
                // Ignore malformed draft metadata.
            }
        }

        var server = await Store.LoadAsync(name);
        if (server is not null && draftUpdated <= server.LastModified)
        {
            _hasRecoveredDraft = false;
            _showDraftRecoveryChoice = false;
            _draftJsonCandidate = null;
            return;
        }

        _hasRecoveredDraft = true;
        _showDraftRecoveryChoice = true;
        _draftJsonCandidate = draftJson;
    }

    private async Task UseLocalDraftAsync()
    {
        if (string.IsNullOrWhiteSpace(_draftJsonCandidate))
        {
            return;
        }

        ApplyLoadedDocument(_draftJsonCandidate, markSaved: false);
        _showDraftRecoveryChoice = false;
        _hasRecoveredDraft = true;
        await InvokeAsync(StateHasChanged);
    }

    private async Task UseServerVersionAsync()
    {
        var server = await Store.LoadAsync(_commands.Document.Name);
        if (server is not null)
        {
            ApplyLoadedDocument(DesignDocumentSerializer.Serialize(server.Document), markSaved: true);
            _currentETag = server.ETag;
            _currentVersion = server.Version;
        }

        await JS.InvokeVoidAsync("designerInterop.removeItem", LocalStorageDraftPrefix + _commands.Document.Name);
        await JS.InvokeVoidAsync("designerInterop.removeItem", LocalStorageDraftMetaPrefix + _commands.Document.Name);
        _showDraftRecoveryChoice = false;
        _hasRecoveredDraft = false;
        _draftJsonCandidate = null;
        await InvokeAsync(StateHasChanged);
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

    private async Task OnUpsertColumnNodes(IReadOnlyList<DataGridColumnConfig> columns)
    {
        if (SelectedNode is null || _selectedNodeId is null)
        {
            return;
        }

        var existingColumnIds = SelectedNode.Children.TryGetValue("Columns", out var existingColumns)
            ? existingColumns.Select(static node => node.Id).ToArray()
            : [];

        foreach (var columnId in existingColumnIds)
        {
            _commands.Execute(new RemoveNodeCommand(ActivePageIndex, columnId));
        }

        var insertIndex = 0;
        foreach (var column in columns.Where(static column => column.IsEnabled).OrderBy(static column => column.Order))
        {
            var descriptor = Registry.TryGetDescriptor("RadzenDataGridColumn", out var gridColumnDescriptor) ? gridColumnDescriptor : Registry.GetDescriptor("RadzenDataGridColumn");
            var node = CreateNodeForDescriptor(descriptor);
            node.Parameters["Title"] = DesignParameterValue.FromValue(column.Title);
            node.Parameters["Property"] = DesignParameterValue.FromValue(column.FieldName);
            node.Parameters["Sortable"] = DesignParameterValue.FromValue(column.Sortable);
            node.Parameters["Filterable"] = DesignParameterValue.FromValue(column.Filterable);
            node.Parameters["Width"] = DesignParameterValue.FromValue(column.Width);

            if (!_commands.Execute(new AddNodeCommand(ActivePageIndex, new DesignNodeLocation(_selectedNodeId, "Columns", insertIndex++), node)))
            {
                return;
            }
        }

        _hasRecoveredDraft = true;
        await AutoSaveAsync();
        await InvokeAsync(StateHasChanged);
    }

    private async Task OnSetDataGridPaging(DataGridPagingConfig paging)
    {
        if (_selectedNodeId is null)
        {
            return;
        }

        var didMutate = false;
        didMutate |= _commands.Execute(new SetNodeParameterCommand(ActivePageIndex, _selectedNodeId, "AllowPaging", DesignParameterValue.FromValue(paging.AllowPaging)));
        didMutate |= _commands.Execute(new SetNodeParameterCommand(ActivePageIndex, _selectedNodeId, "PageSize", DesignParameterValue.FromValue(Math.Max(10, paging.PageSize))));

        if (didMutate)
        {
            _hasRecoveredDraft = true;
            await AutoSaveAsync();
            await InvokeAsync(StateHasChanged);
        }
    }

    private void SetViewport(string viewport) => _viewport = _viewportWidths.ContainsKey(viewport) ? viewport : "desktop";

    private void ToggleFileMenu()
    {
        _fileMenuOpen = !_fileMenuOpen;
        if (_fileMenuOpen)
        {
            _settingsMenuOpen = false;
        }
    }

    private void ToggleSettingsMenu()
    {
        _settingsMenuOpen = !_settingsMenuOpen;
        if (_settingsMenuOpen)
        {
            _fileMenuOpen = false;
        }
    }

    private void TogglePageMenu(int pageIndex)
    {
        _pageMenuIndex = _pageMenuIndex == pageIndex ? null : pageIndex;
    }

    private async Task TogglePaletteCollapsed()
    {
        _paletteCollapsed = !_paletteCollapsed;
        await PersistLayoutStateAsync();
    }

    private async Task ToggleDataCollapsed()
    {
        _dataCollapsed = !_dataCollapsed;
        await PersistLayoutStateAsync();
    }

    private async Task ToggleTreeCollapsed()
    {
        _treeCollapsed = !_treeCollapsed;
        await PersistLayoutStateAsync();
    }

    private async Task ToggleCodeCollapsed()
    {
        _codeCollapsed = !_codeCollapsed;
        await PersistLayoutStateAsync();
    }

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
            _fileMenuOpen = false;
            _settingsMenuOpen = false;
            _pageMenuIndex = null;
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
        var now = DateTimeOffset.UtcNow;

        if (now - _lastLocalAutosaveUtc >= TimeSpan.FromSeconds(5))
        {
            var json = DesignDocumentSerializer.Serialize(_commands.Document);
            await JS.InvokeVoidAsync("designerInterop.setJson", LocalStorageDraftPrefix + _commands.Document.Name, json);
            var draftMeta = JsonSerializer.Serialize(new DraftMeta { UpdatedUtc = now });
            await JS.InvokeVoidAsync("designerInterop.setJson", LocalStorageDraftMetaPrefix + _commands.Document.Name, draftMeta);
            _lastLocalAutosaveUtc = now;
        }

        if (now - _lastRemoteAutosaveUtc >= TimeSpan.FromSeconds(30))
        {
            try
            {
                var envelope = await Store.SaveAsync(_commands.Document.Name, _commands.Document, _currentETag);
                _currentETag = envelope.ETag;
                _currentVersion = envelope.Version;
                _lastRemoteAutosaveUtc = now;
                _offlineWarning = null;
            }
            catch (DesignConflictException)
            {
                _offlineWarning = "Conflictdetectie tijdens autosave. Gebruik handmatig opslaan om op te lossen.";
            }
            catch
            {
                _offlineWarning = "Offline modus - wijzigingen worden lokaal opgeslagen.";
            }
        }

        if (!string.IsNullOrWhiteSpace(_uiFeedback))
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(1600);
                _uiFeedback = null;
                await InvokeAsync(StateHasChanged);
            });
        }
    }

    private async Task RestoreLayoutStateAsync()
    {
        JsonObject? payload = null;
        try
        {
            payload = await JS.InvokeAsync<JsonObject?>("designerInterop.getJson", LocalStorageLayoutKey);
        }
        catch
        {
            var element = await JS.InvokeAsync<JsonElement>("designerInterop.getJson", LocalStorageLayoutKey);
            if (element.ValueKind == JsonValueKind.Object)
            {
                payload = JsonNode.Parse(element.GetRawText()) as JsonObject;
            }
        }

        if (payload is null)
        {
            return;
        }

        _paletteCollapsed = payload.TryGetPropertyValue("paletteCollapsed", out var paletteCollapsed) && paletteCollapsed?.GetValue<bool>() == true;
        _dataCollapsed = !payload.TryGetPropertyValue("dataCollapsed", out var dataCollapsed) || dataCollapsed?.GetValue<bool>() != false;
        _treeCollapsed = !payload.TryGetPropertyValue("treeCollapsed", out var treeCollapsed) || treeCollapsed?.GetValue<bool>() != false;
        _codeCollapsed = !payload.TryGetPropertyValue("codeCollapsed", out var codeCollapsed) || codeCollapsed?.GetValue<bool>() != false;
    }

    private Task PersistLayoutStateAsync()
    {
        var payload = new
        {
            paletteCollapsed = _paletteCollapsed,
            dataCollapsed = _dataCollapsed,
            treeCollapsed = _treeCollapsed,
            codeCollapsed = _codeCollapsed
        };

        return JS.InvokeVoidAsync("designerInterop.setJson", LocalStorageLayoutKey, payload).AsTask();
    }

    private async Task EvaluateStoreAvailabilityAsync()
    {
        try
        {
            _ = await Store.GetRecentAsync();
            _offlineWarning = null;
        }
        catch
        {
            _offlineWarning = "Offline modus - wijzigingen worden lokaal opgeslagen.";
        }
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

    private IReadOnlyList<DesignNode> BuildFormNodes(DesignEntity entity, IReadOnlyList<FormFieldSelectionItem> selectedFields)
    {
        var card = CreateNodeForDescriptor(Registry.GetDescriptor("AgtCard"));
        var header = CreateNodeForDescriptor(Registry.GetDescriptor("AgtPageHeader"));
        header.Parameters["Title"] = DesignParameterValue.FromValue(entity.Name);

        var rowDescriptor = Registry.GetDescriptor("RadzenRow");
        var row = CreateNodeForDescriptor(rowDescriptor);

        var fieldsByName = entity.Fields.ToDictionary(static field => field.Name, StringComparer.OrdinalIgnoreCase);
        foreach (var selected in selectedFields)
        {
            if (!fieldsByName.TryGetValue(selected.Name, out var field))
            {
                continue;
            }

            var columnDescriptor = Registry.GetDescriptor("RadzenColumn");
            var column = CreateNodeForDescriptor(columnDescriptor);
            var fullWidth = ShouldUseFullWidth(field);
            column.Parameters["Size"] = DesignParameterValue.FromValue(fullWidth ? 12 : 6);
            column.Children["ChildContent"] = [BuildNodeForField(entity, field)];
            row.Children["ChildContent"].Add(column);
        }

        card.Children["ChildContent"] =
        [
            header,
            row,
            CreateFormActionsNode()
        ];

        return [card];
    }

    private DesignNode BuildNodeForField(DesignEntity owner, DesignField field)
    {
        var componentType = ResolveFieldComponentType(field);

        var descriptor = Registry.GetDescriptor(componentType);
        var node = CreateNodeForDescriptor(descriptor);
        var label = field.DisplayLabel ?? field.Name;
        node.Parameters["Label"] = DesignParameterValue.FromValue(label);
        node.Parameters["AriaLabel"] = DesignParameterValue.FromValue(label);

        if (componentType is "AgtDropdown")
        {
            if (field.Type == DesignFieldType.Enum)
            {
                node.Parameters["Placeholder"] = DesignParameterValue.FromValue($"Kies {label.ToLowerInvariant()}");
            }
            else if (field.IsForeignKey && !string.IsNullOrWhiteSpace(field.ReferenceEntityName))
            {
                node.Parameters["Data"] = new DesignParameterValue { Expression = $"@entities.{field.ReferenceEntityName}" };
                node.Parameters["TextProperty"] = DesignParameterValue.FromValue("Name");
                node.Parameters["ValueProperty"] = DesignParameterValue.FromValue("Id");
            }
        }

        if (field.IsRequired && descriptor.Parameters.Any(static parameter => string.Equals(parameter.Name, "ValidationMessage", StringComparison.Ordinal)))
        {
            node.Parameters["ValidationMessage"] = DesignParameterValue.FromValue($"{label} is verplicht.");
        }

        if (componentType == "AgtTextArea")
        {
            node.Parameters["Rows"] = DesignParameterValue.FromValue(4L);
        }

        return node;
    }

    private static string ResolveFieldComponentType(DesignField field)
    {
        if (field.IsForeignKey)
        {
            return "AgtDropdown";
        }

        if (field.Type == DesignFieldType.String && IsLongTextField(field))
        {
            return "AgtTextArea";
        }

        return field.Type switch
        {
            DesignFieldType.Int or DesignFieldType.Decimal => "AgtNumericField",
            DesignFieldType.Bool => "AgtSwitch",
            DesignFieldType.DateTime => "AgtDatePicker",
            DesignFieldType.Enum => "AgtDropdown",
            _ => "AgtTextField"
        };
    }

    private static bool IsLongTextField(DesignField field)
    {
        var source = field.DisplayLabel ?? field.Name;
        return source.Contains("Opmerkingen", StringComparison.OrdinalIgnoreCase)
            || source.Contains("Beschrijving", StringComparison.OrdinalIgnoreCase)
            || source.Contains("Notes", StringComparison.OrdinalIgnoreCase);
    }

    private static bool ShouldUseFullWidth(DesignField field)
        => field.IsForeignKey || field.Type == DesignFieldType.Enum || IsLongTextField(field);

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

    private static DesignerTreeNode BuildTreeNode(DesignNode node) => new(node.Id, node.ComponentType, node.ComponentType, node.Children.OrderBy(static pair => pair.Key, StringComparer.Ordinal).SelectMany(static pair => pair.Value).Select(BuildTreeNode).ToArray());

    private RenderFragment RenderTreeNodes(IReadOnlyList<DesignerTreeNode> nodes)
    {
        return builder =>
        {
            var sequence = 0;
            foreach (var node in nodes)
            {
                RenderTreeNode(builder, node, ref sequence, 0);
            }
        };
    }

    private void RenderTreeNode(RenderTreeBuilder builder, DesignerTreeNode node, ref int sequence, int depth)
    {
        builder.OpenElement(sequence++, "button");
        builder.AddAttribute(sequence++, "type", "button");
        builder.AddAttribute(sequence++, "class", string.Equals(node.Id, _selectedNodeId, StringComparison.Ordinal) ? "designer-tree__item designer-tree__item--selected" : "designer-tree__item");
        builder.AddAttribute(sequence++, "style", $"padding-left: calc(var(--agt-spacing-2) + {depth} * 0.75rem);");
        builder.AddAttribute(sequence++, "data-agt-tree-node-id", node.Id);
        builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, () => OnTreeNodeClicked(node.Id)));

        builder.OpenComponent<RadzenIcon>(sequence++);
        builder.AddAttribute(sequence++, "Icon", ResolveTreeIcon(node.ComponentType));
        builder.CloseComponent();

        builder.OpenElement(sequence++, "span");
        builder.AddContent(sequence++, node.Text);
        builder.CloseElement();
        builder.CloseElement();

        for (var index = 0; index < node.Children.Count; index++)
        {
            RenderTreeNode(builder, node.Children[index], ref sequence, depth + 1);
        }
    }

    private string ResolveTreeIcon(string componentType)
    {
        return Registry.TryGetDescriptor(componentType, out var descriptor)
            ? descriptor.Icon
            : "widgets";
    }

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
            _pageMenuIndex = null;
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
            _pageMenuIndex = null;
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
            _pageMenuIndex = null;
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

    private void OnCommandStackDocumentChanged(object? sender, DesignDocumentChangedEventArgs args)
    {
        _ = DebounceValidationAsync();
    }

    private DesignNodeLocation ResolvePaletteClickInsertLocation()
    {
        if (_selectedNodeId is null)
        {
            return DesignNodeLocation.Root(ActivePage.Nodes.Count);
        }

        if (SelectedDescriptor?.Slots.Contains("ChildContent", StringComparer.Ordinal) == true)
        {
            return new DesignNodeLocation(_selectedNodeId, "ChildContent", GetChildInsertIndex(_selectedNodeId));
        }

        return DesignNodeLocation.Root(ActivePage.Nodes.Count);
    }

    private async Task OnInlineAddRequested((string ParentNodeId, string SlotName, string ComponentType) request)
    {
        if (string.IsNullOrWhiteSpace(request.ParentNodeId)
            || string.IsNullOrWhiteSpace(request.SlotName)
            || string.IsNullOrWhiteSpace(request.ComponentType))
        {
            return;
        }

        if (!TryFindNode(ActivePage.Nodes, request.ParentNodeId, out _, out var container, out var index))
        {
            return;
        }

        var parent = container[index];
        parent.Children.TryGetValue(request.SlotName, out var slotNodes);
        var insertIndex = slotNodes?.Count ?? 0;
        var location = new DesignNodeLocation(request.ParentNodeId, request.SlotName, insertIndex);
        if (!AddFromPalette(location, request.ComponentType))
        {
            return;
        }

        _uiFeedback = "Component toegevoegd in slot.";
        _liveAnnouncement = "Component toegevoegd in leeg slot.";
        await AutoSaveAsync();
        await InvokeAsync(StateHasChanged);
    }

    private string GetRootDropzoneClass(int index)
    {
        var id = $"root-{index}";
        var classes = new StringBuilder("designer-dropzone designer-dropzone--root");
        if (IsDragActive)
        {
            classes.Append(" designer-dropzone--ready");
        }

        if (string.Equals(_hoverDropzoneId, id, StringComparison.Ordinal))
        {
            classes.Append(" designer-dropzone--hover");
        }

        return classes.ToString();
    }

    private async Task DebounceValidationAsync()
    {
        _validationDebounceCts?.Cancel();
        _validationDebounceCts?.Dispose();
        _validationDebounceCts = new CancellationTokenSource();
        var token = _validationDebounceCts.Token;

        try
        {
            await Task.Delay(200, token);
            if (token.IsCancellationRequested)
            {
                return;
            }

            await InvokeAsync(() =>
            {
                RefreshValidationIssues();
                StateHasChanged();
            });
        }
        catch (OperationCanceledException)
        {
            // Debounce was restarted; ignore.
        }
    }

    private void RefreshValidationIssues()
    {
        _validationIssues = DesignDocumentValidator.Validate(_commands.Document, Registry).ToList();
        if (_validationIssues.Count > 0)
        {
            _issuesExpanded = true;
        }

        _ = ValidationIssuesChanged.InvokeAsync(_validationIssues);
    }

    internal async Task RunValidationNowAsync()
    {
        RefreshValidationIssues();
        await InvokeAsync(StateHasChanged);
    }

    private async Task OnIssueClicked(DesignValidationError issue)
    {
        if (issue.PageIndex is int pageIndex && pageIndex >= 0 && pageIndex < _commands.Document.Pages.Count)
        {
            SelectPage(pageIndex);
        }

        if (!string.IsNullOrWhiteSpace(issue.NodeId))
        {
            _selectedNodeId = issue.NodeId;
        }

        await InvokeAsync(StateHasChanged);

        if (!string.IsNullOrWhiteSpace(issue.ParameterName))
        {
            await JS.InvokeVoidAsync("designerInterop.scrollToPropertyParameter", issue.ParameterName);
        }
    }

    private static string GetIssueIcon(DesignValidationSeverity severity) => severity switch
    {
        DesignValidationSeverity.Error => "error",
        DesignValidationSeverity.Warning => "warning",
        _ => "info"
    };

    private static string GetSeverityLabel(DesignValidationSeverity severity) => severity switch
    {
        DesignValidationSeverity.Error => "Fout",
        DesignValidationSeverity.Warning => "Waarschuwing",
        _ => "Info"
    };

    private string GetIssueLocation(DesignValidationError issue)
    {
        if (issue.PageIndex is not int pageIndex || pageIndex < 0 || pageIndex >= _commands.Document.Pages.Count)
        {
            return issue.Path;
        }

        var page = _commands.Document.Pages[pageIndex];
        var pageLabel = GetPageLabel(page);

        if (!string.IsNullOrWhiteSpace(issue.NodeId) && TryFindNode(page.Nodes, issue.NodeId, out var nodePath, out _, out _))
        {
            return $"{pageLabel} > {string.Join(" > ", nodePath.Select(static node => node.ComponentType))}";
        }

        return pageLabel;
    }

    private bool CanAutoFix(DesignValidationError issue)
    {
        return issue.Code is "MissingFormLabel" or "EmptyButtonText" or "DuplicateRoute" or "HardcodedColor";
    }

    private async Task ApplyIssueFixAsync(DesignValidationError issue)
    {
        var didMutate = false;

        switch (issue.Code)
        {
            case "MissingFormLabel":
                didMutate = await ApplyMissingFormLabelFixAsync(issue);
                break;
            case "EmptyButtonText":
                didMutate = await ApplyEmptyButtonTextFixAsync(issue);
                break;
            case "DuplicateRoute":
                didMutate = await ApplyDuplicateRouteFixAsync(issue);
                break;
            case "HardcodedColor":
                didMutate = await ApplyHardcodedColorFixAsync(issue);
                break;
        }

        if (didMutate)
        {
            _hasRecoveredDraft = true;
            await AutoSaveAsync();
            await InvokeAsync(StateHasChanged);
        }
    }

    private Task<bool> ApplyDuplicateRouteFixAsync(DesignValidationError issue)
    {
        if (issue.PageIndex is not int pageIndex || pageIndex < 0 || pageIndex >= _commands.Document.Pages.Count)
        {
            return Task.FromResult(false);
        }

        var sourceRoute = _commands.Document.Pages[pageIndex].Route;
        var uniqueRoute = GenerateUniqueRouteFromRoute(sourceRoute);
        var updated = _commands.Execute(new SetPagePropertyCommand(pageIndex, nameof(DesignPage.Route), uniqueRoute));
        return Task.FromResult(updated);
    }

    private Task<bool> ApplyMissingFormLabelFixAsync(DesignValidationError issue)
    {
        if (!TryResolveIssueNode(issue, out var pageIndex, out var node))
        {
            return Task.FromResult(false);
        }

        var label = GetPreferredNodeLabel(node);
        if (string.IsNullOrWhiteSpace(label))
        {
            label = node.ComponentType;
        }

        var changed = _commands.Execute(new SetNodeParameterCommand(pageIndex, node.Id, "Label", DesignParameterValue.FromValue(label)));
        return Task.FromResult(changed);
    }

    private Task<bool> ApplyEmptyButtonTextFixAsync(DesignValidationError issue)
    {
        if (!TryResolveIssueNode(issue, out var pageIndex, out var node))
        {
            return Task.FromResult(false);
        }

        var text = GetPreferredNodeLabel(node);
        if (string.IsNullOrWhiteSpace(text))
        {
            text = node.ComponentType;
        }

        var changed = _commands.Execute(new SetNodeParameterCommand(pageIndex, node.Id, "Text", DesignParameterValue.FromValue(text)));
        return Task.FromResult(changed);
    }

    private Task<bool> ApplyHardcodedColorFixAsync(DesignValidationError issue)
    {
        if (!TryResolveIssueNode(issue, out var pageIndex, out var node)
            || string.IsNullOrWhiteSpace(issue.ParameterName)
            || string.IsNullOrWhiteSpace(_hardcodedColorFixToken))
        {
            return Task.FromResult(false);
        }

        var changed = _commands.Execute(new SetNodeParameterCommand(pageIndex, node.Id, issue.ParameterName, DesignParameterValue.FromValue(_hardcodedColorFixToken)));
        return Task.FromResult(changed);
    }

    private bool TryResolveIssueNode(DesignValidationError issue, out int pageIndex, out DesignNode node)
    {
        pageIndex = -1;
        node = default!;

        if (issue.PageIndex is not int resolvedPageIndex || string.IsNullOrWhiteSpace(issue.NodeId))
        {
            return false;
        }

        if (resolvedPageIndex < 0 || resolvedPageIndex >= _commands.Document.Pages.Count)
        {
            return false;
        }

        var page = _commands.Document.Pages[resolvedPageIndex];
        if (!TryFindNode(page.Nodes, issue.NodeId, out _, out var container, out var index))
        {
            return false;
        }

        pageIndex = resolvedPageIndex;
        node = container[index];
        return true;
    }

    private static string GetPreferredNodeLabel(DesignNode node)
    {
        if (node.Parameters.TryGetValue("Label", out var labelValue) && labelValue?.Literal is JsonValue labelLiteral && labelLiteral.TryGetValue<string>(out var labelText) && !string.IsNullOrWhiteSpace(labelText))
        {
            return labelText;
        }

        if (node.Parameters.TryGetValue("Title", out var titleValue) && titleValue?.Literal is JsonValue titleLiteral && titleLiteral.TryGetValue<string>(out var titleText) && !string.IsNullOrWhiteSpace(titleText))
        {
            return titleText;
        }

        return node.ComponentType;
    }

    private sealed record DesignerTreeNode(string Id, string Text, string ComponentType, IReadOnlyList<DesignerTreeNode> Children)
    {
        public string Value => Id;
    }

    private sealed record InlineAddRequest(string ParentNodeId, string SlotName, string ComponentType);

    private sealed record TokenOption(string Label, string Value);

    private sealed class DraftMeta
    {
        public DateTimeOffset UpdatedUtc { get; set; }
    }
}
