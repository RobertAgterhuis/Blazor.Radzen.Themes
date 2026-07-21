using Bunit;
using Agterhuis.Ui.Designer.Components;
using Agterhuis.Ui.Designer.Model;
using Agterhuis.Ui.Designer.Persistence;
using Agterhuis.Ui.Services;
using Agterhuis.Ui.Components.Feedback;
using Microsoft.Extensions.DependencyInjection;

namespace Agterhuis.Ui.Tests;

public sealed class DesignerShellPersistenceUiTests
{
    [Fact]
    public void VersionHistoryButton_IsRendered()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.Services.AddSingleton<IAgtConfirmDialog>(new ConfirmDialogStub());
        ctx.Services.AddSingleton<IAgtCommandRegistry>(new AgtCommandRegistry());

        var store = new StaticDesignStore();
        var cut = ctx.Render<DesignerShell>(parameters => parameters
            .Add(component => component.Store, store));

        cut.FindAll(".designer-menu-toggle")
            .First(button => button.TextContent.Contains("Bestand", StringComparison.Ordinal))
            .Click();

        Assert.Contains("Versiegeschiedenis", cut.Markup, StringComparison.Ordinal);
    }

    private sealed class StaticDesignStore : IDesignStore
    {
        private readonly DesignDocumentEnvelope _envelope = new("Demo", 1, "etag-1", DateTimeOffset.UtcNow,
            new DesignDocument
            {
                Name = "Demo",
                Pages =
                [
                    new DesignPage { Route = "/", Title = "Demo", Nodes = [] }
                ]
            });

        public Task<IReadOnlyList<DesignListItem>> GetRecentAsync()
            => Task.FromResult<IReadOnlyList<DesignListItem>>([new DesignListItem("Demo", DateTimeOffset.UtcNow, 1)]);

        public Task<DesignDocumentEnvelope?> LoadAsync(string name, int? version = null)
            => Task.FromResult<DesignDocumentEnvelope?>(_envelope);

        public Task<DesignDocumentEnvelope> SaveAsync(string name, DesignDocument document, string? expectedETag)
            => Task.FromResult(_envelope);

        public Task RemoveAsync(string name) => Task.CompletedTask;

        public Task<IReadOnlyList<DesignVersionInfo>> GetVersionsAsync(string name)
            => Task.FromResult<IReadOnlyList<DesignVersionInfo>>([new DesignVersionInfo(1, DateTimeOffset.UtcNow, 1024)]);

        public Task<DesignDocumentEnvelope?> RestoreVersionAsync(string name, int version)
            => Task.FromResult<DesignDocumentEnvelope?>(_envelope);
    }

    private sealed class ConfirmDialogStub : IAgtConfirmDialog
    {
        public Task<bool> ConfirmAsync(string message, string title = "Bevestiging", AgtConfirmOptions? options = null)
            => Task.FromResult(true);

        public Task<bool> ConfirmDeleteAsync(string itemName)
            => Task.FromResult(true);
    }
}
