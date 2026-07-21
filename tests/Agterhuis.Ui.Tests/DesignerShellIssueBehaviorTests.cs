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

        Assert.Contains("Los eerst alle fouten op", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("disabled", cut.Markup, StringComparison.OrdinalIgnoreCase);
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

        Assert.Contains("Issues", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("fouten", cut.Markup, StringComparison.Ordinal);
    }

    private sealed class InMemoryDesignStore : IDesignStore
    {
        private readonly Dictionary<string, string> _documents;

        public InMemoryDesignStore(Dictionary<string, string>? documents = null)
        {
            _documents = documents ?? new Dictionary<string, string>(StringComparer.Ordinal);
        }

        public Task<IReadOnlyList<string>> GetRecentNamesAsync() => Task.FromResult<IReadOnlyList<string>>(_documents.Keys.ToArray());

        public Task<string?> LoadAsync(string name)
        {
            _documents.TryGetValue(name, out var value);
            return Task.FromResult<string?>(value);
        }

        public Task SaveAsync(string name, DesignDocument document)
        {
            _documents[name] = System.Text.Json.JsonSerializer.Serialize(document);
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string name)
        {
            _documents.Remove(name);
            return Task.CompletedTask;
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
