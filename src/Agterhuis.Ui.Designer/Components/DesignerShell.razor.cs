using Agterhuis.Ui.Designer.Commands;
using Agterhuis.Ui.Designer.Export;
using Agterhuis.Ui.Designer.Model;
using Agterhuis.Ui.Designer.Persistence;
using Agterhuis.Ui.Designer.Registry;
using Agterhuis.Ui.Designer.Serialization;
using Agterhuis.Ui.Designer.Validation;
using Agterhuis.Ui.Designer.Services;
using Agterhuis.Ui.Components.Feedback;
using Agterhuis.Ui.Services;
using Agterhuis.Ui.Theming;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Threading;
using System.Text;
using System.Net;
using System.Globalization;
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
    private const string LocalStorageOnboardedKey = "agt-designer-onboarded";
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
    private LeftPanelTab _leftTab = LeftPanelTab.Palette;
    private RightPanelTab _rightTab = RightPanelTab.Properties;
    private bool _leftPanelCollapsed;
    private bool _rightPanelCollapsed = true;
    private bool _codeCollapsed = true;
    private bool _fileMenuOpen;
    private bool _settingsMenuOpen;
    private bool _showNewDocumentDialog;
    private bool _showStartScreen;
    private bool _previewMode;
    private bool _showOnboardingOverlay;
    private bool _showExportDialog;
    private bool _exportIncludeSeedData = true;
    private bool _showShortcutsOverlay;
    private bool _showDesignerCommandPalette;
    private bool _interactionMode;
    private bool _editingDocumentName;
    private string _editingDocumentNameValue = string.Empty;
    private readonly HashSet<string> _collapsedTreeNodes = new(StringComparer.Ordinal);
    private string? _dragSourcePaletteComponentType;
    private int _dragVisualEpoch;
    private string _dragVisualState = "resting";
    private string? _treeContextMenuNodeId;
    private string _treeContextMenuXpx = "0px";
    private string _treeContextMenuYpx = "0px";
    private int? _pageMenuIndex;
    private int? _pageDragOverIndex;
    private bool _pageAddMenuOpen;
    private string? _hoverDropzoneId;
    private string? _hoveredNodeId;
    private readonly List<ToastMessage> _toasts = [];
    private readonly HashSet<string> _selectedNodeIds = new(StringComparer.Ordinal);
    private string _liveAnnouncement = "Designer geladen.";
    private string _selectedRowLayout = "12";
    private string? _clipboardNodeJson;
    private string _commandSearchQuery = string.Empty;
    private int _commandSelectedIndex;
    private DesignDataContext? _designDataContext;

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
    private IReadOnlyList<DesignDocumentTemplates.TemplateDefinition> AddPageTemplateOptions => TemplateOptions
        .Where(static template => template.Kind is not DesignDocumentTemplateKind.Blank and not DesignDocumentTemplateKind.SidebarApp)
        .ToArray();
    private IReadOnlyDictionary<string, IReadOnlyList<DesignerComponentDescriptor>> PaletteByCategory => Registry.Components
        .Where(static descriptor => descriptor.AllowedInPalette)
        .Where(static descriptor => !DesignerComponentDisplayMap.IsHiddenFromPalette(descriptor.ComponentType))
        .Where(static descriptor => !string.IsNullOrWhiteSpace(descriptor.DesignerCategory ?? descriptor.Category))
        .Where(DescriptorMatchesFilter)
        .GroupBy(static descriptor => descriptor.DesignerCategory ?? descriptor.Category, StringComparer.Ordinal)
        .OrderBy(static group => group.Key, StringComparer.Ordinal)
        .ToDictionary(
            static group => group.Key,
            static group => (IReadOnlyList<DesignerComponentDescriptor>)group.OrderBy(static descriptor => descriptor.DesignerDisplayName ?? descriptor.DisplayName, StringComparer.Ordinal).ToArray(),
            StringComparer.Ordinal);
    private DesignPage ActivePage => _commands.Document.Pages.Count == 0 ? new DesignPage { Route = "/", Title = "Nieuwe pagina" } : _commands.Document.Pages[Math.Clamp(_selectedPageIndex, 0, _commands.Document.Pages.Count - 1)];
    private int ActivePageIndex => _commands.Document.Pages.Count == 0 ? 0 : Math.Clamp(_selectedPageIndex, 0, _commands.Document.Pages.Count - 1);
    private IReadOnlyList<DesignerTreeNode> TreeRoots => BuildTree(ActivePage.Nodes);
    private DesignNode? SelectedNode => _selectedNodeId is null ? null : TryFindNode(ActivePage.Nodes, _selectedNodeId, out _, out var container, out var index) ? container[index] : null;
    private DesignerComponentDescriptor? SelectedDescriptor => SelectedNode is null ? null : Registry.TryGetDescriptor(SelectedNode.ComponentType, out var descriptor) ? descriptor : null;
    private IReadOnlyList<SelectionBreadcrumbPart> SelectionBreadcrumbParts => BuildBreadcrumbParts(_selectedNodeId);
    private string CanvasFrameStyle => _viewportWidths.TryGetValue(_viewport, out var width) ? $"max-width: {width}px;" : "max-width: 1200px;";
    private string CanvasFrameClass => _viewport switch
    {
        "mobile" => "designer-canvas-frame--mobile",
        "tablet" => "designer-canvas-frame--tablet",
        _ => string.Empty
    };
    private bool HasDuplicateActiveRoute => _commands.Document.Pages.Count(page => string.Equals(page.Route, ActivePage.Route, StringComparison.OrdinalIgnoreCase)) > 1;
    private IReadOnlyList<DesignValidationError> ValidationIssues => _validationIssues;
    private bool IsDragActive => _activeDrag is not null;
    private string DragStateCssClass => $"designer-page--drag-{_dragVisualState}";
    private string InteractionModeCssClass => _interactionMode && !_previewMode ? "designer-page--interaction" : string.Empty;
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
    private bool CanShowStartScreen => string.IsNullOrWhiteSpace(NameQuery)
        && string.IsNullOrWhiteSpace(TemplateQuery)
        && !_commands.IsDirty
        && string.Equals(_commands.Document.Name, "Untitled", StringComparison.Ordinal)
        && _commands.Document.Pages.Count == 1
        && _commands.Document.Pages[0].Nodes.Count > 0
        && _commands.Document.Pages[0].Nodes.All(static node => string.Equals(node.ComponentType, "RadzenRow", StringComparison.Ordinal));
    private int TotalComponentCount => _commands.Document.Pages.Sum(static page => CountNodes(page.Nodes));
    private IReadOnlyList<DesignerCommandPaletteItem> FilteredCommandItems => BuildFilteredCommandItems();
    private IReadOnlyList<DesignerCommandGroup> FilteredCommandGroups => GroupFilteredCommandItems(FilteredCommandItems);
    private IReadOnlyList<string> UsedEntities => _commands.Document.DataModel.Entities
        .Where(static entity => !string.IsNullOrWhiteSpace(entity.Name))
        .Select(static entity => entity.Name)
        .OrderBy(static name => name, StringComparer.Ordinal)
        .ToArray();
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
        _designDataContext = new DesignDataContext(_commands.Document.DataModel);
        await RestoreLayoutStateAsync();
        await LoadInitialDocumentAsync();
        EnsureSelectedPageIndex();
        await RestoreDocumentsFromStorageAsync();
        RefreshValidationIssues();
        await EvaluateStoreAvailabilityAsync();
        await EvaluateDraftRecoveryAsync();
        await EvaluateOnboardingAsync();
        UpdateStartScreenState();
        await InvokeAsync(StateHasChanged);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        await JS.InvokeVoidAsync("designerInterop.setupResizablePanels");
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
            UpdateStartScreenState();
    }

    private void OnPaletteFilterChanged(ChangeEventArgs args) => _paletteFilter = args.Value?.ToString() ?? string.Empty;

    private async Task OnPaletteDragStart(DragEventArgs args, DesignerComponentDescriptor descriptor)
    {
        _activeDrag = DesignerDragPayload.Palette(descriptor.ComponentType);
        _dragSourcePaletteComponentType = descriptor.ComponentType;
        await JS.InvokeVoidAsync(
            "designerInterop.setPaletteDragImage",
            descriptor.DesignerIcon ?? descriptor.Icon,
            descriptor.DesignerDisplayName ?? descriptor.DisplayName);
    }
    private Task OnDragStart(DesignerDragPayload payload)
    {
        _activeDrag = payload;
        _liveAnnouncement = "Sleepbewerking gestart.";
        SetDragVisualState("grabbed");
        return Task.CompletedTask;
    }

    private Task OnDragEnd()
    {
        var hadDrag = _activeDrag is not null;
        _activeDrag = null;
        _hoverDropzoneId = null;
        _dragSourcePaletteComponentType = null;

        if (hadDrag)
        {
            _ = SetTransientDragVisualStateAsync("cancelled", 260);
        }

        return Task.CompletedTask;
    }

    private void OnDropZoneDragOver(DragEventArgs args, string dropzoneId)
    {
        _hoverDropzoneId = dropzoneId;
        if (_activeDrag is not null)
        {
            SetDragVisualState("in-transit");
        }
    }

    private void OnDropZoneDragEnter(string dropzoneId)
    {
        _hoverDropzoneId = dropzoneId;
        if (_activeDrag is not null)
        {
            SetDragVisualState("in-transit");
        }
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
        _dragSourcePaletteComponentType = null;
        if (didMutate)
        {
            _liveAnnouncement = "Component geplaatst op canvas.";
            await AutoSaveAsync();
            await InvokeAsync(StateHasChanged);
            await JS.InvokeVoidAsync("designerInterop.flashNode", _selectedNodeId);
            await SetTransientDragVisualStateAsync("dropped", 300);
        }
        else
        {
            await SetTransientDragVisualStateAsync("cancelled", 260);
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
        => await OnSelectNode(nodeId, shiftKey: false, ctrlKey: false);

    private async Task OnSelectNode((string NodeId, bool ShiftKey, bool CtrlKey) args)
        => await OnSelectNode(args.NodeId, args.ShiftKey, args.CtrlKey);

    private async Task OnSelectNode(string nodeId, bool shiftKey, bool ctrlKey)
    {
        if (ctrlKey)
        {
            if (!_selectedNodeIds.Remove(nodeId))
            {
                _selectedNodeIds.Add(nodeId);
            }

            _selectedNodeId = _selectedNodeIds.Count == 0 ? null : _selectedNodeIds.Last();
        }
        else if (shiftKey)
        {
            _selectedNodeIds.Add(nodeId);
            _selectedNodeId = nodeId;
        }
        else
        {
            _selectedNodeIds.Clear();
            _selectedNodeId = nodeId;
        }

        var selectedCount = GetEffectiveSelectedNodeIds().Count;
        _liveAnnouncement = selectedCount > 1
            ? $"{selectedCount} nodes geselecteerd."
            : "Node geselecteerd.";
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

    private async Task OnTreeNodeClicked(string nodeId)
    {
        _selectedNodeIds.Clear();
        _selectedNodeId = nodeId;
        CloseTreeContextMenu();
        _liveAnnouncement = "Node geselecteerd vanuit structuurboom.";
        await InvokeAsync(StateHasChanged);
        await JS.InvokeVoidAsync("designerInterop.scrollTreeItemIntoView", nodeId);
    }

    private void OnTreeNodeHover(string? nodeId)
    {
        _hoveredNodeId = nodeId;
    }

    private async Task OnPaletteItemClickedAsync(string componentType)
    {
        var location = ResolvePaletteClickInsertLocation(componentType);
        if (!AddFromPalette(location, componentType))
        {
            return;
        }

        ShowToast("Component toegevoegd", ToastType.Success);
        _liveAnnouncement = "Component toegevoegd via klik.";
        await AutoSaveAsync();
        await InvokeAsync(StateHasChanged);
        await JS.InvokeVoidAsync("designerInterop.flashNode", _selectedNodeId);
    }

    private Task OpenDesignerCommandPaletteAsync()
    {
        _showDesignerCommandPalette = true;
        _commandSearchQuery = string.Empty;
        _commandSelectedIndex = 0;
        return InvokeAsync(StateHasChanged);
    }

    private void CloseDesignerCommandPalette()
    {
        _showDesignerCommandPalette = false;
        _commandSearchQuery = string.Empty;
        _commandSelectedIndex = 0;
    }

    private async Task OnUndo()
    {
        var commandName = _commands.LastCommandName;
        if (_commands.Undo())
        {
            EnsureSelectedPageIndex();
            ClearSelectionIfMissing();
            ShowToast($"Ongedaan: {commandName ?? "actie"}", ToastType.Info);
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
            ShowToast($"Opnieuw: {_commands.LastCommandName ?? "actie"}", ToastType.Info);
            await AutoSaveAsync();
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task OnSaveDocument()
    {
        CloseAllMenus();
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
        CloseAllMenus();
        _showExportDialog = true;
        await InvokeAsync(StateHasChanged);
    }

    private async Task ConfirmExportAsync()
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

        var result = _projectExporter.ExportProject(
            _commands.Document,
            _commands.Document.Name,
            _canvasTheme.Split('-')[0],
            _exportIncludeSeedData);
        await JS.InvokeVoidAsync("designerInterop.saveBytesFile", $"{_commands.Document.Name}.zip", "application/zip", result.ZipData);
        _showExportDialog = false;
    }

    private Task OnExportIncludeSeedDataChanged(bool value)
    {
        _exportIncludeSeedData = value;
        return Task.CompletedTask;
    }

    private async Task ExportDesignSpecAsync()
    {
        var html = BuildDesignSpecHtml();
        var bytes = Encoding.UTF8.GetBytes(html);
        await JS.InvokeVoidAsync("designerInterop.saveBytesFile", $"{_commands.Document.Name}-design-spec.html", "text/html;charset=utf-8", bytes);
    }

    private void CloseExportDialog()
    {
        _showExportDialog = false;
        StateHasChanged();
    }

    private void TogglePreviewMode()
    {
        _previewMode = !_previewMode;
        if (_previewMode)
        {
            _interactionMode = false;
        }

        _liveAnnouncement = _previewMode ? "Preview modus actief." : "Bewerkmodus actief.";
        CloseAllMenus();
        StateHasChanged();
    }

    private void ToggleInteractionMode()
    {
        if (_previewMode)
        {
            return;
        }

        _interactionMode = !_interactionMode;
        _liveAnnouncement = _interactionMode
            ? "Interactie modus actief in bewerkcanvas."
            : "Interactie modus uitgeschakeld.";
        StateHasChanged();
    }

    private async Task DismissOnboardingAsync()
    {
        _showOnboardingOverlay = false;
        await JS.InvokeVoidAsync("designerInterop.setJson", LocalStorageOnboardedKey, new { onboarded = true, updatedUtc = DateTimeOffset.UtcNow });
        await InvokeAsync(StateHasChanged);
    }

    private async Task OnOpenDocument()
    {
        CloseAllMenus();
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
        UpdateStartScreenState();
    }

    private async Task OnSavedSelectionChanged(string? value)
    {
        var selected = value;
        if (string.IsNullOrWhiteSpace(selected))
        {
            return;
        }

        _selectedSavedName = selected;
        CloseAllMenus();
        var envelope = await Store.LoadAsync(selected);
        if (envelope is not null)
        {
            ApplyLoadedDocument(DesignDocumentSerializer.Serialize(envelope.Document), markSaved: true);
            _currentETag = envelope.ETag;
            _currentVersion = envelope.Version;
            await EvaluateDraftRecoveryAsync();
            UpdateStartScreenState();
        }
    }

    private async Task OnTemplateStartSelected(DesignDocumentTemplateKind kind)
    {
        _selectedTemplateKind = kind;
        var name = $"Document-{DateTime.UtcNow:yyyyMMdd-HHmmss}";
        _commands.ReplaceDocument(CreateNewDocument(name, kind), markSaved: false);
        _selectedEntityName = _commands.Document.DataModel.Entities.FirstOrDefault()?.Name;
        _selectedNodeId = null;
        _selectedPageIndex = 0;
        _showStartScreen = false;
        _hasRecoveredDraft = true;
        await AutoSaveAsync();
        await InvokeAsync(StateHasChanged);
    }

    private async Task OpenSavedFromStartAsync(string name)
    {
        _selectedSavedName = name;
        var envelope = await Store.LoadAsync(name);
        if (envelope is null)
        {
            return;
        }

        ApplyLoadedDocument(DesignDocumentSerializer.Serialize(envelope.Document), markSaved: true);
        _currentETag = envelope.ETag;
        _currentVersion = envelope.Version;
        _showStartScreen = false;
        await EvaluateDraftRecoveryAsync();
        await InvokeAsync(StateHasChanged);
    }

    private Task OnStartScreenImportRequested(InputFileChangeEventArgs args)
        => OnDesignFileChanged(args);

    private async Task OnSelectedEntityChanged(string value)
    {
        _selectedEntityName = string.IsNullOrWhiteSpace(value) ? null : value;
        await SelectedEntityNameChanged.InvokeAsync(value);
        await InvokeAsync(StateHasChanged);
    }

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
        _showStartScreen = false;
        await AutoSaveAsync();
        await InvokeAsync(StateHasChanged);
    }

    private void OpenNewDocumentDialog()
    {
        CloseAllMenus();
        _showNewDocumentDialog = true;
    }

    private void CloseNewDocumentDialog()
    {
        _showNewDocumentDialog = false;
        StateHasChanged();
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
                UpdateStartScreenState();
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
            UpdateStartScreenState();
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
        UpdateStartScreenState();
        StateHasChanged();
    }

    private async Task OpenVersionHistoryAsync()
    {
        CloseAllMenus();
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

    private async Task OnCanvasThemeChanged(string value)
    {
        _canvasTheme = string.IsNullOrWhiteSpace(value) ? "plum-dark" : value;
        await CanvasThemeChanged.InvokeAsync(_canvasTheme);
        // No explicit StateHasChanged — Blazor auto-renders after EventCallback completes.
        // No CloseAllMenus — the Radzen dropdown manages its own popup lifecycle.
    }

    private Task ToggleDarkLight()
    {
        var normalized = string.IsNullOrWhiteSpace(_canvasTheme) ? "plum-dark" : _canvasTheme;
        var isDark = normalized.EndsWith("-dark", StringComparison.OrdinalIgnoreCase);
        var family = normalized.EndsWith("-dark", StringComparison.OrdinalIgnoreCase)
            ? normalized[..^5]
            : normalized.EndsWith("-light", StringComparison.OrdinalIgnoreCase)
                ? normalized[..^6]
                : normalized;

        var targetTheme = isDark ? $"{family}-light" : $"{family}-dark";
        return OnCanvasThemeChanged(targetTheme);
    }

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

    private async Task OnSplitColumnNode()
    {
        if (SelectedNode is null
            || _selectedNodeId is null
            || !string.Equals(SelectedNode.ComponentType, "RadzenColumn", StringComparison.Ordinal)
            || !TryFindNode(ActivePage.Nodes, _selectedNodeId, out _, out var container, out var index))
        {
            return;
        }

        var currentSize = 12;
        if (SelectedNode.Parameters.TryGetValue("Size", out var currentSizeValue)
            && currentSizeValue?.Literal is not null
            && currentSizeValue.Literal is JsonValue sizeValue
            && sizeValue.TryGetValue<int>(out var parsedSize))
        {
            currentSize = Math.Max(1, parsedSize);
        }

        var left = Math.Max(1, currentSize / 2);
        var right = Math.Max(1, currentSize - left);

        _commands.Execute(new SetNodeParameterCommand(ActivePageIndex, _selectedNodeId, "Size", DesignParameterValue.FromValue(left)));

        var clone = DesignNodeDeepClone(SelectedNode);
        clone.Id = Guid.NewGuid().ToString("n");
        clone.Parameters["Size"] = DesignParameterValue.FromValue(right);
        clone.Children["ChildContent"] = [];

        if (_commands.Execute(new AddNodeCommand(ActivePageIndex, DesignNodeLocation.Root(index + 1), clone)))
        {
            _hasRecoveredDraft = true;
            _selectedNodeId = clone.Id;
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
            _pageAddMenuOpen = false;
        }
    }

    private void ToggleSettingsMenu()
    {
        _settingsMenuOpen = !_settingsMenuOpen;
        if (_settingsMenuOpen)
        {
            _fileMenuOpen = false;
            _pageAddMenuOpen = false;
        }
    }

    private void TogglePageAddMenu()
    {
        _pageAddMenuOpen = !_pageAddMenuOpen;
        if (_pageAddMenuOpen)
        {
            _fileMenuOpen = false;
            _settingsMenuOpen = false;
            _pageMenuIndex = null;
            CloseTreeContextMenu();
        }
    }

    private void TogglePageMenu(int pageIndex)
    {
        _fileMenuOpen = false;
        _settingsMenuOpen = false;
        _pageAddMenuOpen = false;
        _pageMenuIndex = _pageMenuIndex == pageIndex ? null : pageIndex;
        CloseTreeContextMenu();
    }

    private void CloseAllMenus()
    {
        _fileMenuOpen = false;
        _settingsMenuOpen = false;
        _pageAddMenuOpen = false;
        _pageMenuIndex = null;
        _showDesignerCommandPalette = false;
        CloseTreeContextMenu();
    }

    private async Task ToggleLeftPanelCollapsed()
    {
        _leftPanelCollapsed = !_leftPanelCollapsed;
        await PersistLayoutStateAsync();
    }

    private async Task ToggleRightPanelCollapsed()
    {
        if (_rightPanelCollapsed)
        {
            _rightTab = RightPanelTab.Data;
        }

        _rightPanelCollapsed = !_rightPanelCollapsed;
        await PersistLayoutStateAsync();
    }

    private async Task ToggleCodeCollapsed()
    {
        _codeCollapsed = !_codeCollapsed;
        await PersistLayoutStateAsync();
    }

    private async Task OnPageKeyDown(KeyboardEventArgs args)
    {
        if (args.CtrlKey && string.Equals(args.Key, "k", StringComparison.OrdinalIgnoreCase))
        {
            await OpenDesignerCommandPaletteAsync();
            return;
        }

        if (args.CtrlKey && string.Equals(args.Key, "e", StringComparison.OrdinalIgnoreCase))
        {
            await OnExportDocument();
            return;
        }

        if (args.CtrlKey && string.Equals(args.Key, "s", StringComparison.OrdinalIgnoreCase))
        {
            await OnSaveDocument();
            return;
        }

        if (args.CtrlKey && string.Equals(args.Key, "d", StringComparison.OrdinalIgnoreCase) && _selectedNodeId is not null)
        {
            var selected = GetEffectiveSelectedNodeIds();
            if (selected.Count > 1)
            {
                foreach (var nodeId in selected)
                {
                    await OnDuplicateNode(nodeId);
                }
            }
            else
            {
                await OnDuplicateNode(_selectedNodeId);
            }

            return;
        }

        if (args.CtrlKey && string.Equals(args.Key, "c", StringComparison.OrdinalIgnoreCase) && _selectedNodeId is not null)
        {
            await CopySelectedNodeAsync();
            return;
        }

        if (args.CtrlKey && string.Equals(args.Key, "v", StringComparison.OrdinalIgnoreCase))
        {
            await PasteNodeAsync();
            return;
        }

        if (args.CtrlKey && string.Equals(args.Key, "/", StringComparison.OrdinalIgnoreCase))
        {
            ToggleShortcutsOverlay();
            await InvokeAsync(StateHasChanged);
            return;
        }

        if (args.CtrlKey && args.ShiftKey && string.Equals(args.Key, "p", StringComparison.OrdinalIgnoreCase))
        {
            await OnAddPageAsync();
            return;
        }

        if (args.CtrlKey && string.Equals(args.Key, "p", StringComparison.OrdinalIgnoreCase))
        {
            TogglePreviewMode();
            await InvokeAsync(StateHasChanged);
            return;
        }

        if (args.CtrlKey && string.Equals(args.Key, "i", StringComparison.OrdinalIgnoreCase))
        {
            ToggleInteractionMode();
            await InvokeAsync(StateHasChanged);
            return;
        }

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
            if (_fileMenuOpen || _settingsMenuOpen || _pageMenuIndex is not null)
            {
                CloseAllMenus();
                await InvokeAsync(StateHasChanged);
                return;
            }

            if (_showDesignerCommandPalette)
            {
                CloseDesignerCommandPalette();
                await InvokeAsync(StateHasChanged);
                return;
            }

            if (_showShortcutsOverlay)
            {
                ToggleShortcutsOverlay();
                await InvokeAsync(StateHasChanged);
                return;
            }

            _selectedNodeId = null;
            _selectedNodeIds.Clear();
            await InvokeAsync(StateHasChanged);
            return;
        }

        if (_showDesignerCommandPalette)
        {
            await OnDesignerCommandSearchKeyDown(args);
            return;
        }

        if (string.Equals(args.Key, "Delete", StringComparison.OrdinalIgnoreCase))
        {
            var toDelete = GetEffectiveSelectedNodeIds();
            var removed = 0;
            foreach (var nodeId in toDelete)
            {
                if (_commands.Execute(new RemoveNodeCommand(ActivePageIndex, nodeId)))
                {
                    removed++;
                }
            }

            if (removed > 0)
            {
                _selectedNodeId = null;
                _selectedNodeIds.Clear();
                _hasRecoveredDraft = true;
                ShowToast($"{removed} component(en) verwijderd", ToastType.Warning);
                await AutoSaveAsync();
                await InvokeAsync(StateHasChanged);
            }

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
            _selectedNodeIds.Clear();
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
        _selectedNodeIds.Clear();
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

        _leftPanelCollapsed = payload.TryGetPropertyValue("leftPanelCollapsed", out var leftPanelCollapsed) && leftPanelCollapsed?.GetValue<bool>() == true;
        _rightPanelCollapsed = payload.TryGetPropertyValue("rightPanelCollapsed", out var rightPanelCollapsed) && rightPanelCollapsed?.GetValue<bool>() == true;

        if (payload.TryGetPropertyValue("leftTab", out var leftTabNode)
            && Enum.TryParse<LeftPanelTab>(leftTabNode?.GetValue<string>(), out var leftTab))
        {
            _leftTab = leftTab;
        }

        if (payload.TryGetPropertyValue("rightTab", out var rightTabNode)
            && Enum.TryParse<RightPanelTab>(rightTabNode?.GetValue<string>(), out var rightTab))
        {
            _rightTab = rightTab;
        }

        if (payload.TryGetPropertyValue("paletteCollapsed", out var legacyPaletteCollapsed))
        {
            _leftPanelCollapsed = legacyPaletteCollapsed?.GetValue<bool>() == true;
        }

        if (payload.TryGetPropertyValue("dataCollapsed", out var legacyDataCollapsed)
            && legacyDataCollapsed?.GetValue<bool>() == false)
        {
            _rightTab = RightPanelTab.Data;
        }

        if (payload.TryGetPropertyValue("treeCollapsed", out var legacyTreeCollapsed)
            && legacyTreeCollapsed?.GetValue<bool>() == false)
        {
            _leftTab = LeftPanelTab.Navigator;
        }

        _codeCollapsed = !payload.TryGetPropertyValue("codeCollapsed", out var codeCollapsed) || codeCollapsed?.GetValue<bool>() != false;
    }

    private Task PersistLayoutStateAsync()
    {
        var payload = new
        {
            leftPanelCollapsed = _leftPanelCollapsed,
            rightPanelCollapsed = _rightPanelCollapsed,
            leftTab = _leftTab.ToString(),
            rightTab = _rightTab.ToString(),
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

        return (descriptor.DesignerDisplayName ?? descriptor.DisplayName).Contains(_paletteFilter, StringComparison.OrdinalIgnoreCase)
            || descriptor.ComponentType.Contains(_paletteFilter, StringComparison.OrdinalIgnoreCase)
            || (descriptor.DesignerCategory ?? descriptor.Category).Contains(_paletteFilter, StringComparison.OrdinalIgnoreCase)
            || (!string.IsNullOrWhiteSpace(descriptor.DesignerDescription) && descriptor.DesignerDescription.Contains(_paletteFilter, StringComparison.OrdinalIgnoreCase));
    }

    private static DesignDocument CreateNewDocument(string name, DesignDocumentTemplateKind templateKind)
    {
        return DesignDocumentTemplates.Create(templateKind, name);
    }

    private void RegisterCommands()
    {
        var dynamicCommands = new List<AgtCommandItem>
        {
            new("designer-add-page", "Pagina toevoegen", "Designer", OnAddPageAsync)
            {
                Description = "Voeg een nieuwe pagina toe vanuit het gekozen patroon.",
                ShortcutHint = "Ctrl+Shift+P"
            },
            new("designer-insert-text", "Tekstveld invoegen", "Designer", OnInsertTextFieldAsync)
            {
                Description = "Voeg een tekstveld toe aan de geselecteerde container of de root.",
                ShortcutHint = "Ctrl+K"
            },
            new("designer-export", "Exporteren", "Designer", OnExportDocument)
            {
                Description = "Download een geexporteerd projectpakket.",
                ShortcutHint = "Ctrl+E"
            },
            new("designer-undo", "Ongedaan maken", "Designer", OnUndo)
            {
                Description = "Maak de laatste ontwerpwijziging ongedaan.",
                ShortcutHint = "Ctrl+Z"
            },
            new("designer-preview", "Preview modus", "Designer", TogglePreviewCommandAsync)
            {
                Description = "Wissel tussen bewerken en preview.",
                ShortcutHint = "Ctrl+P"
            }
        };

        foreach (var descriptor in Registry.Components
                     .Where(static descriptor => descriptor.AllowedInPalette)
                     .Where(static descriptor => !DesignerComponentDisplayMap.IsHiddenFromPalette(descriptor.ComponentType))
                     .OrderBy(static descriptor => descriptor.DesignerDisplayName ?? descriptor.DisplayName, StringComparer.OrdinalIgnoreCase)
                     .Take(200))
        {
            var componentType = descriptor.ComponentType;
            var title = $"Voeg toe: {descriptor.DesignerDisplayName ?? descriptor.DisplayName}";
            dynamicCommands.Add(new AgtCommandItem($"designer-add-component-{componentType}", title, "Componenten", () => OnPaletteItemClickedAsync(componentType))
            {
                Description = descriptor.DesignerDescription ?? componentType,
                Keywords = [componentType, descriptor.Category, descriptor.DesignerCategory ?? string.Empty]
            });
        }

        for (var i = 0; i < _commands.Document.Pages.Count; i++)
        {
            var pageIndex = i;
            var pageLabel = GetPageLabel(_commands.Document.Pages[pageIndex]);
            dynamicCommands.Add(new AgtCommandItem($"designer-open-page-{pageIndex}", $"Ga naar: {pageLabel}", "Pagina's", () => InvokeAsync(() =>
            {
                SelectPage(pageIndex);
                StateHasChanged();
            }))
            {
                Description = _commands.Document.Pages[pageIndex].Route
            });
        }

        if (SelectedDescriptor is not null)
        {
            foreach (var parameter in SelectedDescriptor.Parameters
                         .Where(static parameter => !parameter.IsEventCallback)
                         .OrderBy(static parameter => parameter.Name, StringComparer.OrdinalIgnoreCase)
                         .Take(80))
            {
                var parameterName = parameter.Name;
                dynamicCommands.Add(new AgtCommandItem($"designer-property-{parameterName}", $"Eigenschap: {GetParameterDisplayName(parameterName)}", "Eigenschappen", () => JS.InvokeVoidAsync("designerInterop.scrollToPropertyParameter", parameterName).AsTask())
                {
                    Description = $"Spring naar {parameterName}"
                });
            }
        }

        CommandRegistry.SetCommands(CommandScope, dynamicCommands);
    }

    private Task TogglePreviewCommandAsync()
    {
        TogglePreviewMode();
        return InvokeAsync(StateHasChanged);
    }

    private async Task OnAddPageAsync()
        => await OnAddPageFromTemplateAsync(DesignDocumentTemplateKind.Blank);

    private async Task OnAddPageFromTemplateAsync(DesignDocumentTemplateKind templateKind)
    {
        CloseAllMenus();
        var route = GenerateUniqueRoute();
        var title = $"Pagina {_commands.Document.Pages.Count + 1}";
        var page = DesignDocumentTemplates.Create(templateKind, title).Pages.First();
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

    private Task OnPreviewNavigate(string route)
    {
        if (string.IsNullOrWhiteSpace(route))
        {
            return Task.CompletedTask;
        }

        var targetIndex = _commands.Document.Pages
            .Select((page, index) => new { page, index })
            .FirstOrDefault(item => string.Equals(item.page.Route, route, StringComparison.OrdinalIgnoreCase))
            ?.index;

        if (targetIndex is int index)
        {
            SelectPage(index);
            return InvokeAsync(StateHasChanged);
        }

        return Task.CompletedTask;
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
            node.Parameters["Label"] = DesignParameterValue.FromValue(descriptor.DesignerDisplayName ?? descriptor.DisplayName);
        }

        if (descriptor.Parameters.Any(static parameter => string.Equals(parameter.Name, "AriaLabel", StringComparison.Ordinal)))
        {
            node.Parameters["AriaLabel"] = DesignParameterValue.FromValue(descriptor.DesignerDisplayName ?? descriptor.DisplayName);
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

    private IReadOnlyList<SelectionBreadcrumbPart> BuildBreadcrumbParts(string? nodeId)
    {
        if (string.IsNullOrWhiteSpace(nodeId) || !TryFindNode(ActivePage.Nodes, nodeId, out var path, out _, out _))
        {
            return [];
        }

        return path.Select(static node => new SelectionBreadcrumbPart(node.Id, node.ComponentType)).ToArray();
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
        var hasChildren = node.Children.Count > 0;
        var isCollapsed = hasChildren && _collapsedTreeNodes.Contains(node.Id);

        builder.OpenElement(sequence++, "button");
        builder.AddAttribute(sequence++, "type", "button");
        var classes = "designer-tree__item";
        if (string.Equals(node.Id, _selectedNodeId, StringComparison.Ordinal))
        {
            classes += " designer-tree__item--selected";
        }
        else if (string.Equals(node.Id, _hoveredNodeId, StringComparison.Ordinal))
        {
            classes += " designer-tree__item--hover";
        }

        builder.AddAttribute(sequence++, "class", classes);
        builder.AddAttribute(sequence++, "style", $"padding-left: calc(var(--agt-spacing-2) + {depth} * 0.75rem);");
        builder.AddAttribute(sequence++, "data-agt-tree-node-id", node.Id);
        builder.AddAttribute(sequence++, "draggable", "true");
        builder.AddAttribute(sequence++, "ondragstart", EventCallback.Factory.Create<DragEventArgs>(this, _ => OnTreeDragStart(node.Id)));
        builder.AddAttribute(sequence++, "ondragover", EventCallback.Factory.Create<DragEventArgs>(this, args => OnTreeDragOver(args, node.Id)));
        builder.AddEventPreventDefaultAttribute(sequence++, "ondragover", true);
        builder.AddAttribute(sequence++, "ondrop", EventCallback.Factory.Create<DragEventArgs>(this, _ => OnTreeDrop(node.Id)));
        builder.AddEventPreventDefaultAttribute(sequence++, "ondrop", true);
        builder.AddAttribute(sequence++, "ondragend", EventCallback.Factory.Create<DragEventArgs>(this, _ => OnDragEnd()));
        builder.AddAttribute(sequence++, "oncontextmenu", EventCallback.Factory.Create<MouseEventArgs>(this, args => OpenTreeContextMenu(args, node.Id)));
        builder.AddEventPreventDefaultAttribute(sequence++, "oncontextmenu", true);
        builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, () => OnTreeNodeClicked(node.Id)));
        builder.AddAttribute(sequence++, "onmouseenter", EventCallback.Factory.Create(this, () => OnTreeNodeHover(node.Id)));
        builder.AddAttribute(sequence++, "onmouseleave", EventCallback.Factory.Create(this, () => OnTreeNodeHover(null)));

        builder.OpenElement(sequence++, "span");
        builder.AddAttribute(sequence++, "class", hasChildren ? "designer-tree__toggle" : "designer-tree__toggle designer-tree__toggle--empty");
        if (hasChildren)
        {
            builder.AddAttribute(sequence++, "role", "button");
            builder.AddAttribute(sequence++, "aria-label", isCollapsed ? "Uitvouwen" : "Invouwen");
            builder.AddAttribute(sequence++, "onclick:stopPropagation", true);
            builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, args => ToggleTreeNodeCollapse(args, node.Id)));
        }
        builder.AddContent(sequence++, hasChildren ? (isCollapsed ? "▸" : "▾") : "•");
        builder.CloseElement();

        builder.OpenComponent<RadzenIcon>(sequence++);
        builder.AddAttribute(sequence++, "Icon", ResolveTreeIcon(node.ComponentType));
        builder.CloseComponent();

        builder.OpenElement(sequence++, "span");
        builder.AddContent(sequence++, node.Text);
        builder.CloseElement();

        var status = ResolveTreeNodeStatus(node.Id);
        builder.OpenElement(sequence++, "span");
        builder.AddAttribute(sequence++, "class", $"designer-tree__status designer-tree__status--{status.CssClass}");
        builder.AddAttribute(sequence++, "title", status.Tooltip);
        builder.AddContent(sequence++, status.Glyph);
        builder.CloseElement();

        builder.CloseElement();

        if (isCollapsed)
        {
            return;
        }

        for (var index = 0; index < node.Children.Count; index++)
        {
            RenderTreeNode(builder, node.Children[index], ref sequence, depth + 1);
        }
    }

    private void ToggleTreeNodeCollapse(MouseEventArgs args, string nodeId)
    {
        if (!_collapsedTreeNodes.Add(nodeId))
        {
            _collapsedTreeNodes.Remove(nodeId);
        }
    }

    private void OnTreeDragStart(string nodeId)
    {
        _activeDrag = DesignerDragPayload.Node(nodeId);
        _selectedNodeId = nodeId;
        _hoverDropzoneId = null;
        SetDragVisualState("grabbed");
    }

    private void OnTreeDragOver(DragEventArgs args, string targetNodeId)
    {
        _hoverDropzoneId = $"tree:{targetNodeId}";
        if (_activeDrag is not null)
        {
            SetDragVisualState("in-transit");
        }
    }

    private async Task OnTreeDrop(string targetNodeId)
    {
        if (_activeDrag is null || !string.Equals(_activeDrag.Kind, "node", StringComparison.Ordinal))
        {
            return;
        }

        var sourceNodeId = _activeDrag.Value;
        if (string.Equals(sourceNodeId, targetNodeId, StringComparison.Ordinal))
        {
            await OnDragEnd();
            return;
        }

        if (!TryResolveNodePlacement(sourceNodeId, out _, out _, out _, out _)
            || !TryResolveNodePlacement(targetNodeId, out var targetParentNodeId, out var targetSlotName, out var targetIndex, out _))
        {
            await OnDragEnd();
            return;
        }

        var targetLocation = targetParentNodeId is null
            ? DesignNodeLocation.Root(targetIndex + 1)
            : new DesignNodeLocation(targetParentNodeId, targetSlotName, targetIndex + 1);

        if (_commands.Execute(new MoveNodeCommand(ActivePageIndex, sourceNodeId, targetLocation)))
        {
            _selectedNodeId = sourceNodeId;
            _hasRecoveredDraft = true;
            ShowToast("Structuur bijgewerkt", ToastType.Info);
            await AutoSaveAsync();
            await SetTransientDragVisualStateAsync("dropped", 300);
        }
        else
        {
            await SetTransientDragVisualStateAsync("cancelled", 260);
        }

        _activeDrag = null;
        _hoverDropzoneId = null;
        await InvokeAsync(StateHasChanged);
    }

    private void OpenTreeContextMenu(MouseEventArgs args, string nodeId)
    {
        _treeContextMenuNodeId = nodeId;
        _treeContextMenuXpx = $"{Math.Round(args.ClientX, MidpointRounding.AwayFromZero)}px";
        _treeContextMenuYpx = $"{Math.Round(args.ClientY, MidpointRounding.AwayFromZero)}px";
        _selectedNodeId = nodeId;
    }

    private void CloseTreeContextMenu()
    {
        _treeContextMenuNodeId = null;
    }

    private Task OnTreeContextDuplicateAsync()
    {
        var nodeId = _treeContextMenuNodeId;
        CloseTreeContextMenu();
        return string.IsNullOrWhiteSpace(nodeId)
            ? Task.CompletedTask
            : OnDuplicateNode(nodeId);
    }

    private Task OnTreeContextDeleteAsync()
    {
        var nodeId = _treeContextMenuNodeId;
        CloseTreeContextMenu();
        return string.IsNullOrWhiteSpace(nodeId)
            ? Task.CompletedTask
            : OnDeleteNode(nodeId);
    }

    private Task OnTreeContextMoveUpAsync()
    {
        var nodeId = _treeContextMenuNodeId;
        CloseTreeContextMenu();
        return string.IsNullOrWhiteSpace(nodeId)
            ? Task.CompletedTask
            : OnMoveNodeUp(nodeId);
    }

    private Task OnTreeContextMoveDownAsync()
    {
        var nodeId = _treeContextMenuNodeId;
        CloseTreeContextMenu();
        return string.IsNullOrWhiteSpace(nodeId)
            ? Task.CompletedTask
            : OnMoveNodeDown(nodeId);
    }

    private Task OnTreeContextWrapCardAsync()
    {
        var nodeId = _treeContextMenuNodeId;
        CloseTreeContextMenu();
        return string.IsNullOrWhiteSpace(nodeId)
            ? Task.CompletedTask
            : WrapNodeAsync(nodeId, "AgtCard", "RadzenCard");
    }

    private Task OnTreeContextWrapRowAsync()
    {
        var nodeId = _treeContextMenuNodeId;
        CloseTreeContextMenu();
        return string.IsNullOrWhiteSpace(nodeId)
            ? Task.CompletedTask
            : WrapNodeInRowAsync(nodeId);
    }

    private string GetPaletteCardClass(string componentType)
    {
        var classes = "designer-palette-card";
        if (string.Equals(_dragSourcePaletteComponentType, componentType, StringComparison.Ordinal))
        {
            classes += " designer-palette-card--dragging";
        }

        return classes;
    }

    private void SetDragVisualState(string state)
    {
        _dragVisualEpoch++;
        _dragVisualState = state;
    }

    private async Task SetTransientDragVisualStateAsync(string state, int durationMs)
    {
        var epoch = ++_dragVisualEpoch;
        _dragVisualState = state;
        await InvokeAsync(StateHasChanged);
        await Task.Delay(durationMs);

        if (epoch == _dragVisualEpoch)
        {
            _dragVisualState = "resting";
            await InvokeAsync(StateHasChanged);
        }
    }

    private bool TryResolveNodePlacement(
        string nodeId,
        out string? parentNodeId,
        out string slotName,
        out int index,
        out List<DesignNode> container)
    {
        parentNodeId = null;
        slotName = DesignNodeLocation.RootSlotName;
        index = -1;
        container = [];

        if (!TryFindNode(ActivePage.Nodes, nodeId, out var path, out var nodeContainer, out var nodeIndex))
        {
            return false;
        }

        index = nodeIndex;
        container = nodeContainer;

        if (path.Count < 2)
        {
            return true;
        }

        var parent = path[^2];
        parentNodeId = parent.Id;
        foreach (var slot in parent.Children)
        {
            if (ReferenceEquals(slot.Value, nodeContainer))
            {
                slotName = slot.Key;
                return true;
            }
        }

        slotName = "ChildContent";
        return true;
    }

    private async Task WrapNodeAsync(string nodeId, params string[] wrapperTypes)
    {
        if (!TryResolveNodePlacement(nodeId, out var parentNodeId, out var slotName, out var index, out _)
            || !TryFindNode(ActivePage.Nodes, nodeId, out _, out var container, out var nodeIndex))
        {
            return;
        }

        var wrapperDescriptor = wrapperTypes
            .Select(type => Registry.TryGetDescriptor(type, out var descriptor) ? descriptor : null)
            .FirstOrDefault(static descriptor => descriptor is not null);

        if (wrapperDescriptor is null)
        {
            return;
        }

        var selectedNode = container[nodeIndex];
        var wrapperNode = CreateNodeForDescriptor(wrapperDescriptor);
        if (!wrapperNode.Children.ContainsKey("ChildContent"))
        {
            wrapperNode.Children["ChildContent"] = [];
        }

        wrapperNode.Children["ChildContent"].Add(DesignNodeDeepClone(selectedNode));

        if (!_commands.Execute(new RemoveNodeCommand(ActivePageIndex, nodeId)))
        {
            return;
        }

        var addLocation = parentNodeId is null
            ? DesignNodeLocation.Root(index)
            : new DesignNodeLocation(parentNodeId, slotName, index);
        if (!_commands.Execute(new AddNodeCommand(ActivePageIndex, addLocation, wrapperNode)))
        {
            return;
        }

        _selectedNodeId = wrapperNode.Id;
        _hasRecoveredDraft = true;
        ShowToast("Component gewrapt", ToastType.Info);
        await AutoSaveAsync();
        await InvokeAsync(StateHasChanged);
    }

    private async Task WrapNodeInRowAsync(string nodeId)
    {
        if (!Registry.TryGetDescriptor("RadzenRow", out var rowDescriptor)
            || !Registry.TryGetDescriptor("RadzenColumn", out var columnDescriptor)
            || !TryResolveNodePlacement(nodeId, out var parentNodeId, out var slotName, out var index, out _)
            || !TryFindNode(ActivePage.Nodes, nodeId, out _, out var container, out var nodeIndex))
        {
            return;
        }

        var selectedNode = container[nodeIndex];
        var rowNode = CreateNodeForDescriptor(rowDescriptor);
        var columnNode = CreateNodeForDescriptor(columnDescriptor);

        if (!rowNode.Children.ContainsKey("ChildContent"))
        {
            rowNode.Children["ChildContent"] = [];
        }

        if (!columnNode.Children.ContainsKey("ChildContent"))
        {
            columnNode.Children["ChildContent"] = [];
        }

        columnNode.Children["ChildContent"].Add(DesignNodeDeepClone(selectedNode));
        rowNode.Children["ChildContent"].Add(columnNode);

        if (!_commands.Execute(new RemoveNodeCommand(ActivePageIndex, nodeId)))
        {
            return;
        }

        var addLocation = parentNodeId is null
            ? DesignNodeLocation.Root(index)
            : new DesignNodeLocation(parentNodeId, slotName, index);
        if (!_commands.Execute(new AddNodeCommand(ActivePageIndex, addLocation, rowNode)))
        {
            return;
        }

        _selectedNodeId = rowNode.Id;
        _hasRecoveredDraft = true;
        ShowToast("Component in rij gewrapt", ToastType.Info);
        await AutoSaveAsync();
        await InvokeAsync(StateHasChanged);
    }

    private TreeNodeStatus ResolveTreeNodeStatus(string nodeId)
    {
        var severities = _validationIssues
            .Where(issue => string.Equals(issue.NodeId, nodeId, StringComparison.Ordinal))
            .Select(static issue => issue.Severity)
            .ToArray();

        if (severities.Contains(DesignValidationSeverity.Error))
        {
            return new TreeNodeStatus("error", "✕", "Node bevat fouten");
        }

        if (severities.Contains(DesignValidationSeverity.Warning))
        {
            return new TreeNodeStatus("warning", "!", "Node bevat waarschuwingen");
        }

        if (severities.Contains(DesignValidationSeverity.Info))
        {
            return new TreeNodeStatus("info", "i", "Node heeft informatieve meldingen");
        }

        return new TreeNodeStatus("ok", "✓", "Geen issues");
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

        _selectedNodeIds.RemoveWhere(nodeId => !TryFindNode(ActivePage.Nodes, nodeId, out _, out _, out _));
    }

    private IReadOnlyList<string> GetEffectiveSelectedNodeIds()
    {
        if (_selectedNodeIds.Count > 0)
        {
            return _selectedNodeIds
                .Where(nodeId => TryFindNode(ActivePage.Nodes, nodeId, out _, out _, out _))
                .Distinct(StringComparer.Ordinal)
                .ToArray();
        }

        return _selectedNodeId is not null ? [_selectedNodeId] : [];
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
        CloseAllMenus();
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
        CloseAllMenus();
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
        CloseAllMenus();
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
        _pageDragOverIndex = null;
    }

    private void OnPageDragOver(int targetIndex, DragEventArgs args)
    {
        if (_draggedPageIndex is null)
        {
            _pageDragOverIndex = null;
            return;
        }

        _pageDragOverIndex = targetIndex;
        if (args.DataTransfer is not null)
        {
            args.DataTransfer.DropEffect = "move";
        }
    }

    private void OnPageDragLeave(int targetIndex)
    {
        if (_pageDragOverIndex == targetIndex)
        {
            _pageDragOverIndex = null;
        }
    }

    private async Task OnPageDropAsync(int targetIndex)
    {
        if (_draggedPageIndex is null)
        {
            return;
        }

        var sourceIndex = _draggedPageIndex.Value;
        _draggedPageIndex = null;
        _pageDragOverIndex = null;

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
        _pageDragOverIndex = null;
    }

    private void OnCommandStackDocumentChanged(object? sender, DesignDocumentChangedEventArgs args)
    {
        _designDataContext = new DesignDataContext(_commands.Document.DataModel);
        _ = DebounceValidationAsync();
    }

    private DesignNodeLocation ResolvePaletteClickInsertLocation(string? componentType = null)
    {
        if (_selectedNodeId is null)
        {
            return DesignNodeLocation.Root(ActivePage.Nodes.Count);
        }

        if (IsColumnType(componentType) && IsColumnType(SelectedNode?.ComponentType))
        {
            var parentRow = FindParentOfType(_selectedNodeId, "RadzenRow");
            if (parentRow is not null)
            {
                var siblingCount = parentRow.Children.TryGetValue("ChildContent", out var siblings)
                    ? siblings.Count
                    : 0;
                return new DesignNodeLocation(parentRow.Id, "ChildContent", siblingCount);
            }
        }

        if (IsColumnType(componentType) && string.Equals(SelectedNode?.ComponentType, "RadzenRow", StringComparison.Ordinal))
        {
            return new DesignNodeLocation(_selectedNodeId, "ChildContent", GetChildInsertIndex(_selectedNodeId));
        }

        if (SelectedDescriptor?.Slots.Contains("ChildContent", StringComparer.Ordinal) == true)
        {
            return new DesignNodeLocation(_selectedNodeId, "ChildContent", GetChildInsertIndex(_selectedNodeId));
        }

        return DesignNodeLocation.Root(ActivePage.Nodes.Count);
    }

    private static bool IsColumnType(string? componentType)
    {
        return string.Equals(componentType, "RadzenColumn", StringComparison.Ordinal);
    }

    private DesignNode? FindParentOfType(string childNodeId, string parentComponentType)
    {
        return FindParentOfTypeRecursive(ActivePage.Nodes, childNodeId, parentComponentType, null);
    }

    private static DesignNode? FindParentOfTypeRecursive(
        IReadOnlyList<DesignNode> nodes,
        string targetId,
        string parentType,
        DesignNode? currentParent)
    {
        foreach (var node in nodes)
        {
            if (string.Equals(node.Id, targetId, StringComparison.Ordinal))
            {
                return string.Equals(currentParent?.ComponentType, parentType, StringComparison.Ordinal)
                    ? currentParent
                    : null;
            }

            foreach (var slot in node.Children.Values)
            {
                var result = FindParentOfTypeRecursive(slot, targetId, parentType, node);
                if (result is not null)
                {
                    return result;
                }
            }
        }

        return null;
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

        ShowToast("Component toegevoegd in slot", ToastType.Success);
        _liveAnnouncement = "Component toegevoegd in leeg slot.";
        await AutoSaveAsync();
        await InvokeAsync(StateHasChanged);
        await JS.InvokeVoidAsync("designerInterop.flashNode", _selectedNodeId);
    }

    private Task AddRowLayoutPreset(string layout)
    {
        _selectedRowLayout = layout;
        return AddRowLayoutAsync();
    }

    private async Task AddRowLayoutAsync()
    {
        var segments = _selectedRowLayout.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var row = CreateNodeForDescriptor(Registry.GetDescriptor("RadzenRow"));
        row.Children["ChildContent"] = [];

        foreach (var segment in segments)
        {
            if (!int.TryParse(segment, out var size))
            {
                continue;
            }

            var column = CreateNodeForDescriptor(Registry.GetDescriptor("RadzenColumn"));
            column.Parameters["Size"] = DesignParameterValue.FromValue(size);
            column.Children["ChildContent"] = [];
            row.Children["ChildContent"].Add(column);
        }

        if (_commands.Execute(new AddNodeCommand(ActivePageIndex, DesignNodeLocation.Root(ActivePage.Nodes.Count), row)))
        {
            _selectedNodeId = row.Id;
            _hasRecoveredDraft = true;
            ShowToast("Nieuwe rij toegevoegd", ToastType.Success);
            await AutoSaveAsync();
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task OnMoveNodeUp(string nodeId)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
        {
            return;
        }

        if (_commands.Execute(new ReorderSiblingCommand(ActivePageIndex, nodeId, -1)))
        {
            _selectedNodeId = nodeId;
            _hasRecoveredDraft = true;
            await AutoSaveAsync();
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task OnMoveNodeDown(string nodeId)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
        {
            return;
        }

        if (_commands.Execute(new ReorderSiblingCommand(ActivePageIndex, nodeId, 1)))
        {
            _selectedNodeId = nodeId;
            _hasRecoveredDraft = true;
            await AutoSaveAsync();
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task OnDuplicateNode(string nodeId)
    {
        if (string.IsNullOrWhiteSpace(nodeId)
            || !TryFindNode(ActivePage.Nodes, nodeId, out _, out var container, out var index))
        {
            return;
        }

        var parentNodeId = TryFindNode(ActivePage.Nodes, nodeId, out var path, out _, out _)
            ? (path.Count >= 2 ? path[^2].Id : null)
            : null;
        var targetLocation = path.Count >= 2
            ? new DesignNodeLocation(parentNodeId, "ChildContent", index + 1)
            : DesignNodeLocation.Root(index + 1);

        if (_commands.Execute(new DuplicateNodeCommand(ActivePageIndex, nodeId, targetLocation)))
        {
            _selectedNodeId = null;
            _selectedNodeIds.Clear();

            _hasRecoveredDraft = true;
            ShowToast("Component gedupliceerd", ToastType.Info);
            await AutoSaveAsync();
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task OnDeleteNode(string nodeId)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
        {
            return;
        }

        if (_commands.Execute(new RemoveNodeCommand(ActivePageIndex, nodeId)))
        {
            _selectedNodeId = null;
            _selectedNodeIds.Clear();
            _hasRecoveredDraft = true;
            ShowToast("Component verwijderd", ToastType.Warning);
            await AutoSaveAsync();
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task OnInlineEditCommitted((string ParameterName, string Value) args)
    {
        if (_selectedNodeId is null || string.IsNullOrWhiteSpace(args.ParameterName))
        {
            return;
        }

        if (_commands.Execute(new SetNodeParameterCommand(ActivePageIndex, _selectedNodeId, args.ParameterName, DesignParameterValue.FromValue(args.Value))))
        {
            _hasRecoveredDraft = true;
            ShowToast("Inline wijziging opgeslagen", ToastType.Success);
            await AutoSaveAsync();
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task CopySelectedNodeAsync()
    {
        if (_selectedNodeId is null || !TryFindNode(ActivePage.Nodes, _selectedNodeId, out _, out var container, out var index))
        {
            return;
        }

        var node = container[index];
        _clipboardNodeJson = DesignDocumentSerializer.SerializeNode(node);
        await JS.InvokeVoidAsync("designerInterop.copyToClipboard", _clipboardNodeJson);
        ShowToast("Component gekopieerd", ToastType.Info);
        _liveAnnouncement = "Component gekopieerd naar klembord.";
    }

    private async Task PasteNodeAsync()
    {
        var clipboardJson = await JS.InvokeAsync<string?>("designerInterop.readFromClipboard");
        clipboardJson ??= _clipboardNodeJson;
        if (string.IsNullOrWhiteSpace(clipboardJson))
        {
            return;
        }

        var pasted = DesignDocumentSerializer.DeserializeNode(clipboardJson);
        if (pasted is null)
        {
            return;
        }

        RegenerateIds(pasted);

        var location = _selectedNodeId is not null && TryFindNode(ActivePage.Nodes, _selectedNodeId, out _, out var container, out var index)
            ? DesignNodeLocation.Root(index + 1)
            : DesignNodeLocation.Root(ActivePage.Nodes.Count);

        if (_commands.Execute(new AddNodeCommand(ActivePageIndex, location, pasted)))
        {
            _selectedNodeId = pasted.Id;
            _selectedNodeIds.Clear();
            _hasRecoveredDraft = true;
            ShowToast("Component geplakt", ToastType.Success);
            _liveAnnouncement = "Component geplakt.";
            await AutoSaveAsync();
            await InvokeAsync(StateHasChanged);
            await JS.InvokeVoidAsync("designerInterop.flashNode", pasted.Id);
        }
    }

    private static void RegenerateIds(DesignNode node)
    {
        node.Id = Guid.NewGuid().ToString("n", CultureInfo.InvariantCulture);
        foreach (var slot in node.Children.Values)
        {
            foreach (var child in slot)
            {
                RegenerateIds(child);
            }
        }
    }

    private void ToggleShortcutsOverlay()
    {
        _showShortcutsOverlay = !_showShortcutsOverlay;
    }

    private void BeginEditDocumentName()
    {
        _editingDocumentName = true;
        _editingDocumentNameValue = _commands.Document.Name;
    }

    private async Task OnDocumentNameKeyDown(KeyboardEventArgs args)
    {
        if (string.Equals(args.Key, "Enter", StringComparison.OrdinalIgnoreCase))
        {
            await CommitDocumentNameAsync();
            return;
        }

        if (string.Equals(args.Key, "Escape", StringComparison.OrdinalIgnoreCase))
        {
            CancelEditDocumentName();
        }
    }

    private void CancelEditDocumentName()
    {
        _editingDocumentName = false;
        _editingDocumentNameValue = string.Empty;
    }

    private async Task CommitDocumentNameAsync()
    {
        var nextName = _editingDocumentNameValue.Trim();
        _editingDocumentName = false;
        _editingDocumentNameValue = string.Empty;

        if (string.IsNullOrWhiteSpace(nextName) || string.Equals(nextName, _commands.Document.Name, StringComparison.Ordinal))
        {
            await InvokeAsync(StateHasChanged);
            return;
        }

        if (_commands.Execute(new SetDocumentNameCommand(nextName)))
        {
            _hasRecoveredDraft = true;
            ShowToast("Documentnaam bijgewerkt", ToastType.Info);
            await AutoSaveAsync();
        }

        await InvokeAsync(StateHasChanged);
    }

    private static string GetParameterDisplayName(string parameterName)
    {
        return parameterName switch
        {
            "Label" => "Labeltekst",
            "Placeholder" => "Voorbeeldtekst",
            "Value" => "Waarde",
            "Text" => "Tekst",
            "Title" => "Titel",
            "Description" => "Omschrijving",
            "Icon" => "Icoon",
            "AriaLabel" => "Toegankelijkheidslabel",
            "Disabled" => "Uitgeschakeld",
            "Visible" => "Zichtbaar",
            "Required" => "Verplicht",
            _ => parameterName
        };
    }

    private void OnCommandSearchChanged(ChangeEventArgs args)
    {
        _commandSearchQuery = args.Value?.ToString() ?? string.Empty;
        _commandSelectedIndex = 0;
    }

    private async Task OnDesignerCommandSearchKeyDown(KeyboardEventArgs args)
    {
        if (!_showDesignerCommandPalette)
        {
            return;
        }

        if (string.Equals(args.Key, "Escape", StringComparison.OrdinalIgnoreCase))
        {
            CloseDesignerCommandPalette();
            await InvokeAsync(StateHasChanged);
            return;
        }

        var items = FilteredCommandItems;
        if (items.Count == 0)
        {
            return;
        }

        if (string.Equals(args.Key, "ArrowDown", StringComparison.OrdinalIgnoreCase))
        {
            _commandSelectedIndex = (_commandSelectedIndex + 1) % items.Count;
            await InvokeAsync(StateHasChanged);
            return;
        }

        if (string.Equals(args.Key, "ArrowUp", StringComparison.OrdinalIgnoreCase))
        {
            _commandSelectedIndex = _commandSelectedIndex == 0 ? items.Count - 1 : _commandSelectedIndex - 1;
            await InvokeAsync(StateHasChanged);
            return;
        }

        if (string.Equals(args.Key, "Enter", StringComparison.OrdinalIgnoreCase))
        {
            var item = items[Math.Clamp(_commandSelectedIndex, 0, items.Count - 1)];
            await ExecuteCommandItemAsync(item);
        }
    }

    private async Task ExecuteCommandItemAsync(DesignerCommandPaletteItem item)
    {
        await CommandRegistry.ExecuteAsync(item.Command);
        CloseDesignerCommandPalette();
        await InvokeAsync(StateHasChanged);
    }

    private IReadOnlyList<DesignerCommandPaletteItem> BuildFilteredCommandItems()
    {
        var query = _commandSearchQuery.Trim();
        var ranked = CommandRegistry.Commands
            .Select(command => new
            {
                Command = command,
                Score = ScoreCommandForDesignerPalette(command, query)
            })
            .Where(entry => entry.Score > 0)
            .OrderByDescending(entry => entry.Score)
            .ThenBy(entry => entry.Command.Section, StringComparer.OrdinalIgnoreCase)
            .ThenBy(entry => entry.Command.Title, StringComparer.OrdinalIgnoreCase)
            .Select(entry => entry.Command)
            .ToList();

        return ranked
            .Select((command, index) => new DesignerCommandPaletteItem(command, index))
            .ToArray();
    }

    private static int ScoreCommandForDesignerPalette(AgtCommandItem command, string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return 1;
        }

        var score = 0;
        if (command.Title.Contains(query, StringComparison.OrdinalIgnoreCase))
        {
            score += 120;
        }

        if (FuzzyMatch(query, command.Title))
        {
            score += 80;
        }

        if (!string.IsNullOrWhiteSpace(command.Description))
        {
            if (command.Description.Contains(query, StringComparison.OrdinalIgnoreCase))
            {
                score += 60;
            }

            if (FuzzyMatch(query, command.Description))
            {
                score += 40;
            }
        }

        if (command.Section.Contains(query, StringComparison.OrdinalIgnoreCase) || FuzzyMatch(query, command.Section))
        {
            score += 30;
        }

        foreach (var keyword in command.Keywords)
        {
            if (keyword.Contains(query, StringComparison.OrdinalIgnoreCase) || FuzzyMatch(query, keyword))
            {
                score += 20;
            }
        }

        return score;
    }

    private static bool FuzzyMatch(string query, string target)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return true;
        }

        var q = query.Trim();
        var qi = 0;
        foreach (var ch in target)
        {
            if (qi < q.Length && char.ToLowerInvariant(ch) == char.ToLowerInvariant(q[qi]))
            {
                qi++;
            }
        }

        return qi == q.Length;
    }

    private static IReadOnlyList<DesignerCommandGroup> GroupFilteredCommandItems(IReadOnlyList<DesignerCommandPaletteItem> items)
    {
        return items
            .GroupBy(item => string.IsNullOrWhiteSpace(item.Command.Section) ? "Algemeen" : item.Command.Section, StringComparer.OrdinalIgnoreCase)
            .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
            .Select(group => new DesignerCommandGroup(group.Key, group.ToArray()))
            .ToArray();
    }

    private void ShowToast(string message, ToastType type = ToastType.Success)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        var toast = new ToastMessage(Guid.NewGuid(), message, type, DateTimeOffset.UtcNow, false);
        _toasts.Add(toast);
        StateHasChanged();
        _ = DismissToastAfterDelay(toast);
    }

    private async Task DismissToastAfterDelay(ToastMessage toast)
    {
        await Task.Delay(3000);
        await DismissToast(toast);
    }

    private async Task DismissToast(ToastMessage toast)
    {
        var index = _toasts.FindIndex(candidate => candidate.Id == toast.Id);
        if (index < 0)
        {
            return;
        }

        _toasts[index] = _toasts[index] with { IsExiting = true };
        await InvokeAsync(StateHasChanged);
        await Task.Delay(200);
        _toasts.RemoveAll(candidate => candidate.Id == toast.Id);
        await InvokeAsync(StateHasChanged);
    }

    private static string GetToastClass(ToastType type)
    {
        return type switch
        {
            ToastType.Warning => "designer-toast--warning",
            ToastType.Info => "designer-toast--info",
            _ => "designer-toast--success"
        };
    }

    private sealed record SelectionBreadcrumbPart(string NodeId, string Label);

    private sealed record StatusBarMessage(string Icon, string Text, RenderFragment? Actions, bool Dismissable);

    private sealed record ToastMessage(Guid Id, string Text, ToastType Type, DateTimeOffset Created, bool IsExiting);

    private sealed record DesignerCommandPaletteItem(AgtCommandItem Command, int Index);

    private sealed record DesignerCommandGroup(string Key, IReadOnlyList<DesignerCommandPaletteItem> Items);

    private sealed record TreeNodeStatus(string CssClass, string Glyph, string Tooltip);

    private enum LeftPanelTab
    {
        Palette,
        Navigator
    }

    private enum RightPanelTab
    {
        Properties,
        Data
    }

    private enum ToastType
    {
        Success,
        Info,
        Warning
    }

    private string GetRootDropzoneClass(int index)
    {
        var id = $"root-{index}";
        var classes = new StringBuilder("designer-root-dropzone");

        if (string.Equals(_hoverDropzoneId, id, StringComparison.Ordinal))
        {
            classes.Append(" designer-root-dropzone--active");
        }

        return classes.ToString();
    }

    private StatusBarMessage? GetStatusMessage()
    {
        if (_showDraftRecoveryChoice)
        {
            return new StatusBarMessage("history", "Hersteld werk uit localStorage gevonden. Lokale conceptversie beschikbaar.", RenderDraftRecoveryActions(), false);
        }

        if (!string.IsNullOrWhiteSpace(_offlineWarning))
        {
            return new StatusBarMessage("cloud_off", _offlineWarning, null, true);
        }

        if (_hasRecoveredDraft)
        {
            return new StatusBarMessage("restore", "Hersteld werk uit localStorage gevonden. Sla het bestand op om het definitief te bewaren.", null, true);
        }

        return null;
    }

    private RenderFragment RenderDraftRecoveryActions()
    {
        return builder =>
        {
            builder.OpenComponent<RadzenButton>(0);
            builder.AddAttribute(1, "Text", "Lokale draft gebruiken");
            builder.AddAttribute(2, "ButtonStyle", ButtonStyle.Primary);
            builder.AddAttribute(3, "Variant", Variant.Flat);
            builder.AddAttribute(4, "Click", EventCallback.Factory.Create<MouseEventArgs>(this, _ => UseLocalDraftAsync()));
            builder.CloseComponent();

            builder.OpenComponent<RadzenButton>(5);
            builder.AddAttribute(6, "Text", "Serverversie laden");
            builder.AddAttribute(7, "ButtonStyle", ButtonStyle.Base);
            builder.AddAttribute(8, "Variant", Variant.Flat);
            builder.AddAttribute(9, "Click", EventCallback.Factory.Create<MouseEventArgs>(this, _ => UseServerVersionAsync()));
            builder.CloseComponent();
        };
    }

    private void DismissStatusBar()
    {
        _offlineWarning = null;
        _hasRecoveredDraft = false;
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

    private static int CountNodes(IReadOnlyList<DesignNode> nodes)
    {
        var total = 0;
        foreach (var node in nodes)
        {
            total++;
            foreach (var slot in node.Children.Values)
            {
                total += CountNodes(slot);
            }
        }

        return total;
    }

    private static DesignNode DesignNodeDeepClone(DesignNode source)
    {
        return new DesignNode
        {
            Id = source.Id,
            ComponentType = source.ComponentType,
            Parameters = source.Parameters.ToDictionary(static pair => pair.Key, static pair => pair.Value is null
                ? null!
                : new DesignParameterValue
                {
                    Literal = pair.Value.Literal is null ? null : JsonNode.Parse(pair.Value.Literal.ToJsonString()),
                    Expression = pair.Value.Expression
                }, StringComparer.Ordinal),
            Children = source.Children.ToDictionary(
                static pair => pair.Key,
                static pair => pair.Value.Select(DesignNodeDeepClone).ToList(),
                StringComparer.Ordinal),
            LayoutSlot = source.LayoutSlot
        };
    }

    private string BuildDesignSpecHtml()
    {
        var title = string.IsNullOrWhiteSpace(_commands.Document.Name) ? "Design spec" : _commands.Document.Name;
        var now = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm");
        var sb = new StringBuilder();
        sb.AppendLine("<!doctype html>");
        sb.AppendLine("<html lang=\"nl\">");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset=\"utf-8\" />");
        sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />");
        sb.AppendLine($"<title>{WebUtility.HtmlEncode(title)} - Design spec</title>");
        sb.AppendLine("<style>");
        sb.AppendLine("body{font-family:Segoe UI,Arial,sans-serif;margin:24px;color:#1f2937;background:#f8fafc}");
        sb.AppendLine("h1,h2,h3{margin:0 0 8px}");
        sb.AppendLine("section{background:#fff;border:1px solid #d1d5db;border-radius:10px;padding:16px;margin:0 0 16px}");
        sb.AppendLine("table{width:100%;border-collapse:collapse;margin-top:8px}");
        sb.AppendLine("th,td{border:1px solid #e5e7eb;padding:8px;vertical-align:top;text-align:left}");
        sb.AppendLine("th{background:#f3f4f6}");
        sb.AppendLine("code{background:#f3f4f6;padding:2px 6px;border-radius:6px}");
        sb.AppendLine(".meta{display:grid;grid-template-columns:repeat(auto-fit,minmax(220px,1fr));gap:8px}");
        sb.AppendLine(".muted{color:#6b7280}");
        sb.AppendLine("ul{margin:8px 0 0 20px}");
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine($"<h1>Design spec - {WebUtility.HtmlEncode(title)}</h1>");
        sb.AppendLine("<section>");
        sb.AppendLine("<h2>Samenvatting</h2>");
        sb.AppendLine("<div class=\"meta\">");
        sb.AppendLine($"<div><strong>Gegenereerd:</strong> {WebUtility.HtmlEncode(now)}</div>");
        sb.AppendLine($"<div><strong>Thema:</strong> {WebUtility.HtmlEncode(_canvasTheme)}</div>");
        sb.AppendLine($"<div><strong>Pagina's:</strong> {_commands.Document.Pages.Count}</div>");
        sb.AppendLine($"<div><strong>Componenten:</strong> {TotalComponentCount}</div>");
        sb.AppendLine($"<div><strong>Entiteiten:</strong> {(UsedEntities.Count == 0 ? "Geen" : WebUtility.HtmlEncode(string.Join(", ", UsedEntities)))}</div>");
        sb.AppendLine("</div>");
        sb.AppendLine("</section>");

        for (var pageIndex = 0; pageIndex < _commands.Document.Pages.Count; pageIndex++)
        {
            var page = _commands.Document.Pages[pageIndex];
            sb.AppendLine("<section>");
            sb.AppendLine($"<h2>Pagina {pageIndex + 1}: {WebUtility.HtmlEncode(GetPageLabel(page))}</h2>");
            sb.AppendLine($"<p><strong>Route:</strong> <code>{WebUtility.HtmlEncode(page.Route)}</code></p>");

            sb.AppendLine("<h3>Componenten</h3>");
            sb.AppendLine("<table>");
            sb.AppendLine("<thead><tr><th>Pad</th><th>Type</th><th>Properties</th><th>Databinding</th></tr></thead>");
            sb.AppendLine("<tbody>");
            WriteNodeRows(sb, page.Nodes, "Root");
            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");
            sb.AppendLine("</section>");
        }

        sb.AppendLine("<section>");
        sb.AppendLine("<h2>Datamodel</h2>");
        if (_commands.Document.DataModel.Entities.Count == 0)
        {
            sb.AppendLine("<p class=\"muted\">Geen entiteiten beschikbaar.</p>");
        }
        else
        {
            foreach (var entity in _commands.Document.DataModel.Entities)
            {
                sb.AppendLine($"<h3>{WebUtility.HtmlEncode(entity.Name)}</h3>");
                sb.AppendLine("<ul>");
                foreach (var field in entity.Fields)
                {
                    sb.AppendLine($"<li><strong>{WebUtility.HtmlEncode(field.Name)}</strong> <span class=\"muted\">({WebUtility.HtmlEncode(field.Type.ToString())})</span></li>");
                }
                sb.AppendLine("</ul>");
            }
        }
        sb.AppendLine("</section>");

        sb.AppendLine("</body>");
        sb.AppendLine("</html>");
        return sb.ToString();
    }

    private static void WriteNodeRows(StringBuilder sb, IReadOnlyList<DesignNode> nodes, string parentPath)
    {
        for (var index = 0; index < nodes.Count; index++)
        {
            var node = nodes[index];
            var path = $"{parentPath}/{index + 1}";
            var properties = node.Parameters.Count == 0
                ? "-"
                : string.Join("<br />", node.Parameters.Select(static pair =>
                {
                    var literal = pair.Value?.Literal?.ToJsonString() ?? "";
                    var expr = pair.Value?.Expression;
                    var value = !string.IsNullOrWhiteSpace(expr) ? expr : literal;
                    return $"<code>{WebUtility.HtmlEncode(pair.Key)}</code>: {WebUtility.HtmlEncode(value)}";
                }));
            var bindings = node.Parameters
                .Where(static pair => !string.IsNullOrWhiteSpace(pair.Value?.Expression))
                .Select(static pair => $"{pair.Key} = {pair.Value!.Expression}")
                .ToArray();

            sb.AppendLine("<tr>");
            sb.AppendLine($"<td>{WebUtility.HtmlEncode(path)}</td>");
            sb.AppendLine($"<td>{WebUtility.HtmlEncode(node.ComponentType)}</td>");
            sb.AppendLine($"<td>{properties}</td>");
            sb.AppendLine($"<td>{(bindings.Length == 0 ? "-" : WebUtility.HtmlEncode(string.Join("; ", bindings)))}</td>");
            sb.AppendLine("</tr>");

            foreach (var slot in node.Children)
            {
                WriteNodeRows(sb, slot.Value, $"{path}/{slot.Key}");
            }
        }
    }

    private void UpdateStartScreenState()
    {
        _showStartScreen = CanShowStartScreen;
    }

    private async Task EvaluateOnboardingAsync()
    {
        JsonObject? payload = null;
        try
        {
            payload = await JS.InvokeAsync<JsonObject?>("designerInterop.getJson", LocalStorageOnboardedKey);
        }
        catch
        {
            payload = null;
        }

        var onboarded = payload?.TryGetPropertyValue("onboarded", out var onboardedNode) == true
            && onboardedNode?.GetValue<bool>() == true;
        _showOnboardingOverlay = !onboarded;
    }
}
