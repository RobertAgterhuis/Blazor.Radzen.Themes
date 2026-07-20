using Agterhuis.Ui.Extensions;
using Agterhuis.Ui.Demo.Components.Pages.Guidance;
using Agterhuis.Ui.Demo.Services;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Radzen;

namespace Agterhuis.Ui.Tests;

public sealed class GuidancePagesTests
{
    [Fact]
    public void PatronenPage_RendersPatternOverviewAndExamples()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddAgterhuisUi();
        ctx.Services.AddRadzenComponents();
        ctx.Services.AddSingleton<DemoSourceProvider>();
        ctx.Services.AddScoped<ShowcaseDataService>();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = ctx.Render<Patronen>();

        Assert.Contains("Patronenbibliotheek", cut.Markup);
        Assert.Contains("Formulierpagina", cut.Markup);
        Assert.Contains("Bevestiging", cut.Markup);
        Assert.Contains("Dashboard", cut.Markup);
    }

    [Fact]
    public void SchrijfwijzerPage_RendersCoreToneRules()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddAgterhuisUi();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = ctx.Render<Schrijfwijzer>();

        Assert.Contains("Content-richtlijnen", cut.Markup);
        Assert.Contains("Opslaan", cut.Markup);
        Assert.Contains("Niet doen", cut.Markup);
    }

    [Fact]
    public void OntwerpstelselPage_RendersToolingOverview()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddAgterhuisUi();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = ctx.Render<Ontwerpstelsel>();

        Assert.Contains("Ontwerpstelsel", cut.Markup);
        Assert.Contains("Starter template", cut.Markup);
        Assert.Contains("Token-export", cut.Markup);
        Assert.Contains("Visuele regressie", cut.Markup);
    }

    [Fact]
    public void StarterTemplatePage_RendersTemplateDetails()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddAgterhuisUi();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = ctx.Render<StarterTemplate>();

        Assert.Contains("Starter template", cut.Markup);
        Assert.Contains("dotnet new agterhuis-app", cut.Markup);
        Assert.Contains("Package-referentie", cut.Markup);
    }

    [Fact]
    public void TokenExportPage_RendersExportDetails()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddAgterhuisUi();
        ctx.Services.AddRadzenComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = ctx.Render<global::Agterhuis.Ui.Demo.Components.Pages.Guidance.TokenExport>();

        Assert.Contains("Token-export", cut.Markup);
        Assert.Contains("npm run token:export", cut.Markup);
        Assert.Contains("eng/token-export/output", cut.Markup);
    }
}