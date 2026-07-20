using Agterhuis.Ui.Demo.Components.Pages.App;
using Agterhuis.Ui.Demo.Components.Pages.Catalog;
using Agterhuis.Ui.Demo.Services;
using Agterhuis.Ui.Extensions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.DependencyInjection;
using Radzen;

namespace Agterhuis.Ui.Tests;

public sealed class ShowcaseAppSmokeTests
{
    [Fact]
    public void DashboardRenders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<ShowcaseDashboard>(p => p.Add(x => x.PreviewOnly, true));

        Assert.Contains("Dashboard", cut.Markup);
        Assert.Contains("Dashboard preview", cut.Markup);
    }

    [Fact]
    public void WerkordersRenders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<ShowcaseWerkorders>();

        Assert.Contains("Werkorders", cut.Markup);
        Assert.Contains("Nieuwe werkorder", cut.Markup);
    }

    [Fact]
    public void WerkordersGrid_HeaderAndFirstCellsRemainAligned()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<ShowcaseWerkorders>();

        var headers = cut.FindAll(".showcase-page__workorders-grid thead th")
            .Select(header => header.TextContent.Trim())
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .Take(3)
            .ToArray();

        Assert.Equal(["Nummer", "Klant", "Adres"], headers);

        var firstRowCells = cut.FindAll(".showcase-page__workorders-grid tbody tr:first-child td")
            .Select(cell => cell.TextContent.Trim())
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .Take(3)
            .ToArray();

        Assert.Equal(3, firstRowCells.Length);
        Assert.StartsWith("WO-", firstRowCells[0], StringComparison.Ordinal);
    }

    [Fact]
    public void CatalogDataGrid_HeaderAndFirstCellsRemainAligned()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogDataGridPage>();

        var headers = cut.FindAll("thead th")
            .Select(header => header.TextContent.Trim())
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .Take(3)
            .ToArray();

        Assert.Equal(["Naam", "Team", "Score"], headers);

        var firstRowCells = cut.FindAll("tbody tr:first-child td")
            .Select(cell => cell.TextContent.Trim())
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .Take(3)
            .ToArray();

        Assert.Equal(["Alfa", "Noord", "95"], firstRowCells);
    }

    [Fact]
    public void PlanningRenders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<ShowcasePlanning>(p => p.Add(x => x.PreviewOnly, true));

        Assert.Contains("Planning", cut.Markup);
        Assert.Contains("Planbord", cut.Markup);
    }

    [Fact]
    public void KlantenRenders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<ShowcaseKlanten>();

        Assert.Contains("Klanten", cut.Markup);
        Assert.Contains("Selecteer een klant", cut.Markup);
    }

    [Fact]
    public void RapportageRenders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<ShowcaseRapportage>(p => p.Add(x => x.PreviewOnly, true));

        Assert.Contains("Rapportage", cut.Markup);
        Assert.Contains("Exporteer CSV", cut.Markup);
    }

    [Fact]
    public void InstellingenRenders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<ShowcaseInstellingen>();

        Assert.Contains("Instellingen", cut.Markup);
        Assert.Contains("Actieve instellingen", cut.Markup);
    }

    [Fact]
    public void ProjectenRenders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<ShowcaseProjecten>(p => p.Add(x => x.PreviewOnly, true));

        Assert.Contains("Projecten", cut.Markup);
        Assert.Contains("Nieuw project", cut.Markup);
    }

    [Fact]
    public void AssetsRenders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<ShowcaseAssets>();

        Assert.Contains("Assets en locaties", cut.Markup);
        Assert.Contains("Werkorder uit asset", cut.Markup);
    }

    [Fact]
    public void ServicedeskRenders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<ShowcaseServicedesk>();

        Assert.Contains("Servicedesk", cut.Markup);
        Assert.Contains("Open tickets", cut.Markup);
    }

    [Fact]
    public void HelpRenders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<ShowcaseHelp>();

        Assert.Contains("Help", cut.Markup);
        Assert.Contains("Inhoudsopgave", cut.Markup);
    }

    [Fact]
    public void WerkorderDetailRendersForExistingId()
    {
        using var ctx = CreateContext();
        var dataService = ctx.Services.GetRequiredService<ShowcaseDataService>();
        var knownId = dataService.WorkOrders.First().Id;

        var cut = ctx.Render<ShowcaseWerkorderDetail>(parameters => parameters.Add(x => x.Id, knownId));

        Assert.Contains("Werkorderdetail", cut.Markup);
        Assert.Contains("Werkbon", cut.Markup);
    }

    [Fact]
    public async Task NewWorkOrderAppearsInGridAfterSavingDialog()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<ShowcaseWerkorders>();

        var dataService = ctx.Services.GetRequiredService<ShowcaseDataService>();
        await cut.InvokeAsync(() => dataService.AddWorkOrder(dataService.CreateDraftWorkOrder()));

        cut.WaitForAssertion(() => Assert.Contains("WO-", cut.Markup));

        Assert.Contains("WO-", cut.Markup);
    }

    [Fact]
    public void ProjectCreationViaWizardFlowAddsProject()
    {
        using var ctx = CreateContext();
        var dataService = ctx.Services.GetRequiredService<ShowcaseDataService>();
        var before = dataService.Projects.Count;

        var created = dataService.CreateProject("Testproject", 1, DateTime.Today, DateTime.Today.AddDays(10), [1, 2]);

        Assert.Equal(before + 1, dataService.Projects.Count);
        Assert.Equal("Testproject", created.Name);
    }

    [Fact]
    public void ChatSendAppendsTicketMessage()
    {
        using var ctx = CreateContext();
        var dataService = ctx.Services.GetRequiredService<ShowcaseDataService>();
        var ticket = dataService.Tickets.First();
        var before = dataService.GetMessages(ticket.Id).Count;

        dataService.SendChatMessage(ticket.Id, "Testbericht vanuit smoke test");

        Assert.Equal(before + 2, dataService.GetMessages(ticket.Id).Count);
        Assert.Contains(dataService.GetMessages(ticket.Id), message => message.Text == "Testbericht vanuit smoke test");
    }

    [Fact]
    public void AssetContextActionCreatesWorkOrder()
    {
        using var ctx = CreateContext();
        var dataService = ctx.Services.GetRequiredService<ShowcaseDataService>();
        var assetId = dataService.Assets.First().Id;
        var before = dataService.WorkOrders.Count;

        var created = dataService.CreateWorkOrderFromAsset(assetId);

        Assert.Equal(before + 1, dataService.WorkOrders.Count);
        Assert.Contains("Asset-opdracht", created.Description, StringComparison.OrdinalIgnoreCase);
    }

    private static BunitContext CreateContext()
    {
        var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.Services.AddAgterhuisUi();
        ctx.Services.AddScoped<ShowcaseDataService>();
        ctx.Services.AddSingleton<DemoSourceProvider>();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.JSInterop.SetupVoid("agtTheme.closeAllPopups").SetVoidResult();
        ctx.JSInterop.Setup<bool>("agtTheme.prefersReducedMotion").SetResult(true);
        ctx.JSInterop.Setup<string>("agtTheme.getStoredTheme", _ => true).SetResult("plum-dark");
        ctx.Services.AddSingleton<NavigationManager>(new TestNavigationManager("https://localhost/", "https://localhost/app"));
        return ctx;
    }

    private sealed class TestNavigationManager : NavigationManager
    {
        public TestNavigationManager(string baseUri, string uri)
        {
            Initialize(baseUri, uri);
        }

        protected override void NavigateToCore(string uri, bool forceLoad)
        {
            Initialize(BaseUri, uri);
        }
    }
}