using System.Text.RegularExpressions;

namespace Agterhuis.Ui.Demo.Services;

public sealed class BlogShowcaseService(IWebHostEnvironment environment)
{
    private static readonly Regex PromptNumberRegex = new("^(?<number>\\d+)-", RegexOptions.Compiled);
    private static readonly Regex PromptHeadingRegex = new("^#\\s+(?<title>.+)$", RegexOptions.Multiline | RegexOptions.Compiled);

    private IReadOnlyList<BlogPromptItem>? _cachedPrompts;

    public IReadOnlyList<BlogProjectCard> Projects { get; } =
    [
        new("volt-journey", "Volt Journal Shell", "Mobiele redactionele shell met view-transition details en strakke leesmodus.", "Platform UI", 2026, 7, "Editorial"),
        new("ops-radar", "Ops Radar Console", "Realtime operationeel overzicht met calm-route heuristieken voor datadichte teams.", "DevOps", 2026, 6, "Observability"),
        new("agent-binder", "Agent Binder", "Playbook rond custom AI agents, van promptstrategie tot evaluatie in productie.", "AI Agents", 2025, 8, "Automation"),
        new("workorder-loop", "Workorder Loop", "Workflowonderzoek naar planning, notificaties en field-service handoffs.", "Business Apps", 2025, 5, "Operations"),
        new("token-governance", "Token Governance", "Token-audit rails die design drift vroeg signaleren in component libraries.", "Design Systems", 2024, 9, "Quality"),
        new("prompt-factory", "Prompt Factory", "Curatie van prompts als herbruikbare capability-catalogus per domein.", "Prompting", 2024, 6, "Knowledge")
    ];

    public IReadOnlyList<BlogAgentCard> Agents { get; } =
    [
        new("GraphDroid", "M365 rapportage-agent", "graph monthly-report --scope finance --period this-quarter", "Rapport samengesteld: 14 KPI afwijkingen, 3 actiepunten geprioriteerd.", "M365"),
        new("PipelineMedic", "CI/CD incident triage", "scan pipeline --last-failure --include-tests", "Root cause: flaky integration test in payments suite. Aanbevolen fixpad toegevoegd.", "DevOps"),
        new("PromptSherpa", "Prompt quality reviewer", "review prompt --style editorial --risk medium", "Promptscore 8.6/10. Twee ambiguiteiten verwijderd en evaluatiecriteria aangescherpt.", "AI"),
        new("InfraLighthouse", "Azure governance copiloot", "audit subscriptions --focus networking --depth deep", "12 afwijkingen gevonden, 4 met hoge impact. Remediation backlog gegenereerd.", "Azure")
    ];

    public IReadOnlyList<BlogSkillGroup> SkillGroups { get; } =
    [
        new("Azure", [new("Infrastructure Planning", 88), new("Diagnostics", 82), new("Security & Compliance", 79), new("Cost Optimization", 74)]),
        new("M365", [new("Graph Automation", 84), new("Tenant Reporting", 78), new("Teams Integraties", 71)]),
        new("AI / Agents", [new("Prompt Engineering", 91), new("Evaluation Design", 77), new("Tracing & Telemetry", 73)]),
        new("DevOps", [new("Pipeline Triage", 86), new("Release Coordination", 75), new("Quality Gates", 82)]),
        new("Blazor", [new("RCL Architecture", 89), new("Accessibility", 83), new("Theming", 94)])
    ];

    public IReadOnlyList<BlogArticle> Articles { get; } =
    [
        new(
            "van-wrapper-naar-workflow",
            "Van wrapper naar workflow",
            "Hoe een componentbibliotheek volwassen wordt zodra de focus verschuift van controls naar complete journeys.",
            new[]
            {
                new BlogArticleSection("intro", "Waarom journeys eerst", "Een losse component voelt af als hij renderbaar is. Een workflow voelt pas af als keuzes, feedback en foutpaden coherent zijn over meerdere schermen."),
                new BlogArticleSection("tokens", "Tokens als contract", "Tokens zijn geen design-restje, maar het contract tussen merk, toegankelijkheid en implementatie. Zonder die laag ontstaat drift."),
                new BlogArticleSection("calm", "Calm routes", "Datadichte routes krijgen minimale beweging. Rust in de interface verhoogt besluitkwaliteit en verlaagt cognitieve ruis."),
                new BlogArticleSection("ops", "Operationaliseren", "Koppel designbeslissingen aan tests: token audits, contrast sweeps, smoke tests en routechecks in CI.")
            },
                "Accent is een signaal, geen achtergrondvulling.",
                "if (route.IsDataDense)\n{\n    motion = MotionMode.Calm;\n}")
        ,
        new(
            "prompten-als-product",
            "Prompten als product",
            "Prompts zijn productassets: versioneerbaar, testbaar en herbruikbaar in teams.",
            new[]
            {
                new BlogArticleSection("catalogus", "Catalogusdenken", "Bewaar prompts in een doorzoekbare bibliotheek met onderwerp-tags, nummering en context."),
                new BlogArticleSection("kwaliteit", "Kwaliteit boven volume", "Een kleine set sterke prompts met heldere intent wint van honderden varianten zonder eigenaarschap."),
                new BlogArticleSection("metrieken", "Metrieken", "Meet bruikbaarheid met latency, follow-up rate en task completion in plaats van alleen tokengebruik."),
                new BlogArticleSection("governance", "Governance", "Bepaal wie prompts mag wijzigen, hoe reviews lopen en welke evaluatiecriteria verplicht zijn.")
            },
            "Een prompt zonder evaluatie is een aanname.",
                "prompt.score = evaluator.Run(prompt, dataset);\nif (prompt.score < 0.8) reject(prompt);")
        ,
        new(
            "agenten-met-een-ruggengraat",
            "Agenten met een ruggengraat",
            "Custom agents slagen alleen met duidelijke boundaries, fallback-gedrag en observability.",
            new[]
            {
                new BlogArticleSection("scope", "Scope begrenzen", "Definieer expliciet wat de agent wel en niet doet. Dat voorkomt dat hij alles probeert te beantwoorden."),
                new BlogArticleSection("fallback", "Fallbacks", "Bij lage confidence moet de agent escaleren, niet gokken. Toon de reden en het volgende beste pad."),
                new BlogArticleSection("traces", "Trace everything", "Zonder traces kun je regressies niet begrijpen. Log intent, gebruikte tools en beslispaden."),
                new BlogArticleSection("itereren", "Iteratief verbeteren", "Gebruik evaluaties en productfeedback om prompts en tooling in kleine stappen aan te scherpen.")
            },
            "Betrouwbaarheid ontstaat uit grenzen, niet uit magie.",
                "if (confidence < threshold)\n{\n    return EscalateToHuman(context);\n}")
    ];

    public BlogProjectCard? GetProject(string slug)
    {
        return Projects.FirstOrDefault(project => string.Equals(project.Slug, slug, StringComparison.OrdinalIgnoreCase));
    }

    public BlogArticle? GetArticle(string slug)
    {
        return Articles.FirstOrDefault(article => string.Equals(article.Slug, slug, StringComparison.OrdinalIgnoreCase));
    }

    public IReadOnlyList<BlogPromptItem> GetPrompts()
    {
        if (_cachedPrompts is not null)
        {
            return _cachedPrompts;
        }

        var promptFiles = ResolvePromptFiles();
        var items = new List<BlogPromptItem>();

        foreach (var filePath in promptFiles)
        {
            var fileName = Path.GetFileName(filePath);
            var content = File.ReadAllText(filePath);
            var number = ParsePromptNumber(fileName);
            var title = ParsePromptTitle(fileName, content);
            var summary = ParseSummary(content);
            var tags = BuildTags(fileName, title);

            items.Add(new BlogPromptItem(number, fileName, title, summary, string.Join(", ", tags), content));
        }

        _cachedPrompts = items
            .OrderBy(item => item.Number)
            .ToList();

        return _cachedPrompts;
    }

    private IEnumerable<string> ResolvePromptFiles()
    {
        var candidateDirectories = new[]
        {
            Path.Combine(environment.ContentRootPath, "prompts"),
            Path.Combine(environment.ContentRootPath, "blog-prompts"),
            Path.Combine(AppContext.BaseDirectory, "blog-prompts"),
            Path.GetFullPath(Path.Combine(environment.ContentRootPath, "..", "..", "prompts"))
        };

        foreach (var directory in candidateDirectories)
        {
            if (!Directory.Exists(directory))
            {
                continue;
            }

            var files = Directory.EnumerateFiles(directory, "*.md", SearchOption.TopDirectoryOnly).ToList();
            if (files.Count > 0)
            {
                return files;
            }
        }

        return [];
    }

    private static int ParsePromptNumber(string fileName)
    {
        var match = PromptNumberRegex.Match(fileName);
        if (match.Success && int.TryParse(match.Groups["number"].Value, out var parsed))
        {
            return parsed;
        }

        return 0;
    }

    private static string ParsePromptTitle(string fileName, string content)
    {
        var headingMatch = PromptHeadingRegex.Match(content);
        if (headingMatch.Success)
        {
            return headingMatch.Groups["title"].Value.Trim();
        }

        return Path.GetFileNameWithoutExtension(fileName).Replace('-', ' ');
    }

    private static string ParseSummary(string content)
    {
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim())
            .Where(line => !line.StartsWith("#", StringComparison.Ordinal) && !line.StartsWith("---", StringComparison.Ordinal))
            .ToList();

        return lines.FirstOrDefault() ?? "Prompt without summary.";
    }

    private static IReadOnlyList<string> BuildTags(string fileName, string title)
    {
        var bucket = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var seed = $"{fileName} {title}".ToLowerInvariant();

        if (seed.Contains("theme") || seed.Contains("showcase") || seed.Contains("layout")) bucket.Add("UI");
        if (seed.Contains("azure")) bucket.Add("Azure");
        if (seed.Contains("radzen")) bucket.Add("Radzen");
        if (seed.Contains("prompt")) bucket.Add("Prompt");
        if (seed.Contains("a11y") || seed.Contains("wcag") || seed.Contains("contrast")) bucket.Add("A11Y");
        if (seed.Contains("agent") || seed.Contains("foundry") || seed.Contains("ai")) bucket.Add("AI");
        if (seed.Contains("test") || seed.Contains("audit")) bucket.Add("Quality");

        if (bucket.Count == 0)
        {
            bucket.Add("General");
        }

        return bucket.OrderBy(tag => tag).ToList();
    }
}

public sealed record BlogProjectCard(string Slug, string Title, string Excerpt, string Category, int Year, int ReadMinutes, string Tone);

public sealed record BlogAgentCard(string Name, string Subtitle, string PromptLine, string ResponseLine, string Group);

public sealed record BlogSkillGroup(string Group, IReadOnlyList<BlogSkillItem> Skills);

public sealed record BlogSkillItem(string Label, int Score);

public sealed record BlogPromptItem(int Number, string FileName, string Title, string Summary, string TagList, string Content);

public sealed record BlogArticle(string Slug, string Title, string Intro, IReadOnlyList<BlogArticleSection> Sections, string PullQuote, string CodeSnippet)
{
    public int EstimatedReadMinutes
    {
        get
        {
            var words = Intro.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length
                + Sections.Sum(section => section.Body.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length)
                + PullQuote.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

            return Math.Max(3, (int)Math.Ceiling(words / 180d));
        }
    }
}

public sealed record BlogArticleSection(string Id, string Title, string Body);
