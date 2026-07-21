using Agterhuis.Ui.Designer.Components;
using Agterhuis.Ui.Designer.Model;
using Agterhuis.Ui.Designer.Persistence;
using Agterhuis.Ui.Services;
using Agterhuis.Ui.Components.Feedback;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Agterhuis.Ui.Tests;

public sealed class DesignerShellIssueBehaviorTests
{
    [Fact]
    public void Shell_DisablesExport_WhenValidationHasErrors()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        ctx.Services.AddSingleton<IAgtConfirmDialog>(new ConfirmDialogStub());
        ctx.Services.AddSingleton<IAgtCommandRegistry>(new CommandRegistryStub());

        var invalidDocument = new DesignDocument
        {
            Name = "Broken",
            Pages =
            [
                new DesignPage
                {
                    Route = "/",
                    Title = "",
                    Nodes = []
                }
            ]
        };

        var store = new InMemoryDesignStore(new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Broken"] = System.Text.Json.JsonSerializer.Serialize(invalidDocument)
        });

        var navigation = ctx.Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo(navigation.GetUriWithQueryParameter("name", "Broken"));

        var cut = ctx.Render<DesignerShell>(parameters => parameters
            .Add(component => component.Store, store));

        cut.FindAll(".designer-menu-toggle")
            .First(button => button.TextContent.Contains("Bestand", StringComparison.Ordinal))
            .Click();

        var exportButton = cut.FindAll(".designer-menu__item")
            .First(button => button.TextContent.Contains("Exporteren", StringComparison.Ordinal));

        Assert.True(exportButton.HasAttribute("disabled"));
    }

    [Fact]
    public void Shell_ShowsIssuesPanel()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        ctx.Services.AddSingleton<IAgtConfirmDialog>(new ConfirmDialogStub());
        ctx.Services.AddSingleton<IAgtCommandRegistry>(new CommandRegistryStub());

        var store = new InMemoryDesignStore();
        var cut = ctx.Render<DesignerShell>(parameters => parameters
            .Add(component => component.Store, store));

        cut.Find(".designer-panel--tree .designer-panel__toggle").Click();

        Assert.Contains("Issues", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("fouten", cut.Markup, StringComparison.Ordinal);
    }

    private sealed class InMemoryDesignStore : IDesignStore
    {
        private readonly Dictionary<string, DesignDocumentEnvelope> _documents;

        public InMemoryDesignStore(Dictionary<string, string>? documents = null)
        {
            _documents = new Dictionary<string, DesignDocumentEnvelope>(StringComparer.Ordinal);
            if (documents is null)
            {
                return;
            }

            foreach (var pair in documents)
            {
                var document = Agterhuis.Ui.Designer.Serialization.DesignDocumentSerializer.Deserialize(pair.Value);
                _documents[pair.Key] = new DesignDocumentEnvelope(pair.Key, 1, "test-v1", DateTimeOffset.UtcNow, document);
            }
        }

        public Task<IReadOnlyList<DesignListItem>> GetRecentAsync()
            => Task.FromResult<IReadOnlyList<DesignListItem>>(
                _documents.Values
                    .Select(static envelope => new DesignListItem(envelope.Name, envelope.LastModified, envelope.Version))
                    .ToArray());

        public Task<DesignDocumentEnvelope?> LoadAsync(string name, int? version = null)
        {
            _documents.TryGetValue(name, out var value);
            return Task.FromResult(value);
        }

        public Task<DesignDocumentEnvelope> SaveAsync(string name, DesignDocument document, string? expectedETag)
        {
            var nextVersion = _documents.TryGetValue(name, out var existing) ? existing.Version + 1 : 1;
            var envelope = new DesignDocumentEnvelope(name, nextVersion, $"test-v{nextVersion}", DateTimeOffset.UtcNow, document);
            _documents[name] = envelope;
            return Task.FromResult(envelope);
        }

        public Task RemoveAsync(string name)
        {
            _documents.Remove(name);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<DesignVersionInfo>> GetVersionsAsync(string name)
        {
            if (!_documents.TryGetValue(name, out var envelope))
            {
                return Task.FromResult<IReadOnlyList<DesignVersionInfo>>([]);
            }

            return Task.FromResult<IReadOnlyList<DesignVersionInfo>>([
                new DesignVersionInfo(envelope.Version, envelope.LastModified, 1024)
            ]);
        }

        public Task<DesignDocumentEnvelope?> RestoreVersionAsync(string name, int version)
        {
            _documents.TryGetValue(name, out var envelope);
            return Task.FromResult(envelope);
        }
    }

    private sealed class ConfirmDialogStub : IAgtConfirmDialog
    {
        public Task<bool> ConfirmAsync(string message, string title = "Bevestiging", AgtConfirmOptions? options = null)
            => Task.FromResult(true);

        public Task<bool> ConfirmDeleteAsync(string itemName)
            => Task.FromResult(true);
    }

    private sealed class CommandRegistryStub : IAgtCommandRegistry
    {
        public event Action? Changed;

        public IReadOnlyList<AgtCommandItem> Commands { get; private set; } = [];

        public IReadOnlyList<AgtCommandItem> RecentCommands => [];

        public void SetCommands(string scope, IEnumerable<AgtCommandItem> commands)
        {
            Commands = commands.ToArray();
            Changed?.Invoke();
        }

        public void RemoveScope(string scope)
        {
            Changed?.Invoke();
        }

        public Task ExecuteAsync(AgtCommandItem command)
        {
            return Task.CompletedTask;
        }
    }
}
