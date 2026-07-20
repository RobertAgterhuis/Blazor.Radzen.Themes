using Agterhuis.Ui.Demo.Components.Pages.Blog;
using Agterhuis.Ui.Demo.Services;
using Agterhuis.Ui.Extensions;
using Agterhuis.Ui.Services;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.DependencyInjection;
using Radzen;

namespace Agterhuis.Ui.Tests;

public sealed class BlogShowcaseSmokeTests
{
    [Fact]
    public void BlogHomeRenders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<BlogHome>();

        Assert.Contains("Signal Journal", cut.Markup);
        Assert.Contains("Featured projecten", cut.Markup);
    }

    [Fact]
    public void BlogProjectenRenders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<BlogProjecten>();

        Assert.Contains("Projectreel", cut.Markup);
    }

    [Fact]
    public void BlogAgentsRenders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<BlogAgents>();

        Assert.Contains("Agent cards", cut.Markup);
        Assert.Contains("GraphDroid", cut.Markup);
    }

    [Fact]
    public void BlogPromptsRenders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<BlogPrompts>();

        Assert.Contains("Dogfood prompt library", cut.Markup);
    }

    [Fact]
    public void BlogSkillsRenders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<BlogSkills>();

        Assert.Contains("Skill matrix", cut.Markup);
    }

    [Fact]
    public void BlogArticleRenders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<BlogArtikel>(parameters => parameters.Add(p => p.Slug, "van-wrapper-naar-workflow"));

        Assert.Contains("Geschatte leestijd", cut.Markup);
    }

    [Fact]
    public void BlogProjectDetailRenders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<BlogProjectDetail>(parameters => parameters.Add(p => p.Slug, "volt-journey"));

        Assert.Contains("Volt Journal Shell", cut.Markup);
    }

    [Fact]
    public void BlogOverRenders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<BlogOver>();

        Assert.Contains("Compact contactpunt", cut.Markup);
    }

    [Fact]
    public void PromptLibraryLoadsRepositoryPromptFiles()
    {
        using var ctx = CreateContext();
        var service = ctx.Services.GetRequiredService<BlogShowcaseService>();
        var prompts = service.GetPrompts();

        Assert.NotEmpty(prompts);
        Assert.Contains(prompts, prompt => prompt.FileName.EndsWith(".md", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(prompts, prompt => prompt.FileName.StartsWith("42-", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task PromptFilterReducesVisibleCards()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<BlogPrompts>();

        var before = cut.FindAll(".blog-prompt-card").Count;
        cut.Find("#blog-prompt-search").Input("volt");

        await cut.InvokeAsync(() => Task.Delay(260));

        cut.WaitForAssertion(() =>
        {
            var after = cut.FindAll(".blog-prompt-card").Count;
            Assert.True(after > 0);
            Assert.True(after <= before);
        });
    }

    [Fact]
    public async Task CopyPromptShowsToastViaNotificationService()
    {
        using var ctx = CreateContext();
        var notifier = new RecordingNotifier();
        ctx.Services.AddSingleton<IAgtNotificationService>(notifier);
        ctx.JSInterop.Setup<bool>("blogShowcase.copyText", _ => true).SetResult(true);

        var cut = ctx.Render<BlogPrompts>();
        var copyButton = cut.FindAll("button").First(button => button.TextContent.Contains("Kopieer prompt", StringComparison.OrdinalIgnoreCase));
        copyButton.Click();

        await cut.InvokeAsync(() => Task.CompletedTask);

        Assert.True(notifier.SuccessCalls >= 1);
    }

    private static BunitContext CreateContext()
    {
        var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.Services.AddAgterhuisUi();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var root = GetRepositoryRoot();
        ctx.Services.AddSingleton<IWebHostEnvironment>(new TestWebHostEnvironment(root));
        ctx.Services.AddScoped<BlogShowcaseService>();

        return ctx;
    }

    private static string GetRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Agterhuis.Ui.sln")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Repository root not found.");
    }

    private sealed class TestWebHostEnvironment(string contentRootPath) : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "Agterhuis.Ui.Demo";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = contentRootPath;
        public string EnvironmentName { get; set; } = "Development";
        public string ContentRootPath { get; set; } = contentRootPath;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    private sealed class RecordingNotifier : IAgtNotificationService
    {
        public int SuccessCalls { get; private set; }

        public void Success(string title, string? detail = null, double? duration = null)
        {
            SuccessCalls++;
        }

        public void Warning(string title, string? detail = null, double? duration = null)
        {
        }

        public void Danger(string title, string? detail = null, double? duration = null)
        {
        }

        public void Info(string title, string? detail = null, double? duration = null)
        {
        }
    }
}
