using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Agterhuis.Ui.Designer.Model;
using Agterhuis.Ui.Designer.Persistence;
using Agterhuis.Ui.Designer.Validation;
using Microsoft.AspNetCore.Components;
using Agterhuis.Ui.Demo.Services;
using Microsoft.JSInterop;

namespace Agterhuis.Ui.Tests;

public sealed class DesignerStartScreenTests
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
    public void StartScreen_TemplateButtonNavigatesToEditorWithQuery()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        var jsRuntime = new DesignerJsRuntimeStub();
        ctx.Services.AddSingleton<IJSRuntime>(jsRuntime);
        ctx.Services.AddSingleton(new LocalDesignStore(jsRuntime));

        var navigation = new TestNavigationManager("https://localhost/", "https://localhost/designer");
        ctx.Services.AddSingleton<NavigationManager>(navigation);

        var cut = ctx.Render<Agterhuis.Ui.Demo.Components.Pages.DesignerStart>();

        cut.Find(".designer-startscreen__patterns button").Click();

        Assert.Contains("/designer/edit?template=", navigation.Uri);
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