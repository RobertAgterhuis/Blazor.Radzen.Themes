using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Agterhuis.Ui.Designer.Model;
using Agterhuis.Ui.Designer.Persistence;
using Agterhuis.Ui.Designer.Validation;
using Microsoft.AspNetCore.Components;
using Agterhuis.Ui.Demo.Services;
using Microsoft.JSInterop;

namespace Agterhuis.Ui.Tests;

public sealed class DesignerRouteRedirectTests
{
    [Fact]
    public void AllTemplatesRemainValidDesignDocuments()
    {
        foreach (var definition in DesignDocumentTemplates.DefinitionsList)
        {
            var document = DesignDocumentTemplates.Create(definition.Kind, definition.DisplayName + " demo");
            var errors = Agterhuis.Ui.Designer.Validation.DesignDocumentValidator.Validate(document);

            Assert.Empty(errors);
        }
    }

    [Fact]
    public void DesignerRoute_RedirectsDirectlyToEditor()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        var jsRuntime = new DesignerJsRuntimeStub();
        ctx.Services.AddSingleton<IJSRuntime>(jsRuntime);
        ctx.Services.AddSingleton(new LocalDesignStore(jsRuntime));

        var navigation = new TestNavigationManager("https://localhost/", "https://localhost/designer");
        ctx.Services.AddSingleton<NavigationManager>(navigation);

        var cut = ctx.Render<Agterhuis.Ui.Demo.Components.Pages.DesignerStart>();

        Assert.EndsWith("/designer/edit", navigation.Uri, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(string.Empty, cut.Markup.Trim());
    }

    private sealed class TestNavigationManager : NavigationManager
    {
        public TestNavigationManager(string baseUri, string uri)
        {
            Initialize(baseUri, uri);
        }

        protected override void NavigateToCore(string uri, bool forceLoad)
        {
            Uri = ToAbsoluteUri(uri).ToString();
            NotifyLocationChanged(false);
        }
    }
}