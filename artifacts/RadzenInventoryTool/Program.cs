using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;

var repo = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();
var pkgPropsPath = Path.Combine(repo, "Directory.Packages.props");
var pkgXml = await File.ReadAllTextAsync(pkgPropsPath);
var versionMatch = Regex.Match(pkgXml, "PackageVersion Include=\"Radzen.Blazor\" Version=\"([^\"]+)\"");
if (!versionMatch.Success)
{
    throw new InvalidOperationException("Radzen.Blazor version not found in Directory.Packages.props.");
}

var version = versionMatch.Groups[1].Value;
var nugetHome = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
var libRoot = Path.Combine(nugetHome, "radzen.blazor", version, "lib");
var dllPath = Directory.GetFiles(libRoot, "Radzen.Blazor.dll", SearchOption.AllDirectories)
    .OrderByDescending(p => p, StringComparer.OrdinalIgnoreCase)
    .FirstOrDefault() ?? throw new FileNotFoundException("Radzen.Blazor.dll not found.");

var catalogDir = Path.Combine(repo, "samples", "Agterhuis.Ui.Demo", "Components", "Pages", "Catalog");
var catalogFiles = Directory.GetFiles(catalogDir, "*.razor", SearchOption.TopDirectoryOnly);
var catalogContent = catalogFiles.ToDictionary(file => Path.GetFileName(file)!, File.ReadAllText);

var asm = Assembly.LoadFrom(dllPath);
var componentBaseType = typeof(ComponentBase);
var types = asm.GetExportedTypes()
    .Where(t => !t.IsAbstract && componentBaseType.IsAssignableFrom(t))
    .OrderBy(t => t.Name, StringComparer.Ordinal)
    .ToList();

var childSuffix = new Regex("(Item|Column|Series|Axis|Scale|Value|Title|Legend|Tooltip|Tick|Band|Step|StripLine|Marker|Label|DialogOptions)$", RegexOptions.Compiled);
var serviceBound = new HashSet<string>(StringComparer.Ordinal)
{
    "RadzenGoogleMap", "RadzenSSRSViewer", "RadzenSpeechToTextButton"
};

var forcedStandalone = new HashSet<string>(StringComparer.Ordinal)
{
    "RadzenAccordion", "RadzenAIChat", "RadzenAlert", "RadzenArcGauge", "RadzenAutoComplete", "RadzenBadge",
    "RadzenBarcode", "RadzenBreadCrumb", "RadzenButton", "RadzenCard", "RadzenCardGroup", "RadzenCarousel",
    "RadzenChart", "RadzenChat", "RadzenCheckBox", "RadzenCheckBoxList", "RadzenChip", "RadzenChipList",
    "RadzenColorPicker", "RadzenContextMenu", "RadzenDataFilter", "RadzenDataGrid", "RadzenDataList",
    "RadzenDatePicker", "RadzenDialog", "RadzenDropDown", "RadzenDropDownDataGrid", "RadzenDropDownTree",
    "RadzenDropZone", "RadzenFab", "RadzenFabMenu", "RadzenFieldset", "RadzenFileInput", "RadzenFooter",
    "RadzenFormField", "RadzenGantt", "RadzenGoogleMap", "RadzenGravatar", "RadzenHeader", "RadzenHeatmap",
    "RadzenHtmlEditor", "RadzenIcon", "RadzenImage", "RadzenLayout", "RadzenLinearGauge", "RadzenLink",
    "RadzenListBox", "RadzenLogin", "RadzenMarkdown", "RadzenMask", "RadzenMediaQuery", "RadzenMenu",
    "RadzenNotification", "RadzenNumeric", "RadzenPager", "RadzenPanel", "RadzenPanelMenu", "RadzenPassword",
    "RadzenPickList", "RadzenPivotDataGrid", "RadzenPopup", "RadzenProfileMenu", "RadzenProgressBar",
    "RadzenProgressBarCircular", "RadzenQRCode", "RadzenRadioButtonList", "RadzenRadialGauge", "RadzenRating",
    "RadzenScheduler", "RadzenSecurityCode", "RadzenSelectBar", "RadzenSidebar", "RadzenSidebarToggle",
    "RadzenSkeleton", "RadzenSlider", "RadzenSparkline", "RadzenSpiderChart", "RadzenSplitter", "RadzenSSRSViewer",
    "RadzenStack", "RadzenSteps", "RadzenSwitch", "RadzenTable", "RadzenTabs", "RadzenTemplateForm",
    "RadzenText", "RadzenTextArea", "RadzenTextBox", "RadzenTileLayout", "RadzenTimeline", "RadzenTimeSpanPicker",
    "RadzenToc", "RadzenTree", "RadzenUpload", "RadzenSpeechToTextButton"
};

var childPrefixes = new[]
{
    "RadzenDataGrid", "RadzenHtmlEditor", "RadzenSpreadsheet", "RadzenPivot", "RadzenChart", "RadzenSeries",
    "RadzenLinearGaugeScale", "RadzenRadialGaugeScale", "RadzenArcGaugeScale", "RadzenRangeNavigator", "RadzenGantt",
    "RadzenDropZone", "RadzenTable", "RadzenTreeLevel", "RadzenMenuItem", "RadzenProfileMenuItem", "RadzenTabsItem",
    "RadzenStepsItem", "RadzenAccordionItem", "RadzenTimelineItem", "RadzenNotificationMessage"
};

string Category(string name)
{
    if (Regex.IsMatch(name, "Button|Fab|SplitButton|ProfileMenu|Menu|BreadCrumb|Tabs|Accordion|Steps|Link|ContextMenu|PanelMenu")) return "Navigation & Actions";
    if (Regex.IsMatch(name, "Text|Numeric|Password|Mask|AutoComplete|SecurityCode|FileInput|Upload|DropDown|ListBox|DatePicker|TimeSpanPicker|ColorPicker|Rating|CheckBox|Radio|Switch|Slider|Chip|PickList|DropZone")) return "Forms & Inputs";
    if (Regex.IsMatch(name, "DataGrid|PivotDataGrid|DataList|Table|Tree|Pager|DataFilter|Gantt|Scheduler")) return "Data & Scheduling";
    if (Regex.IsMatch(name, "Chart|Series|Gauge|Sparkline|Sankey|Timeline")) return "Data Visualization";
    if (Regex.IsMatch(name, "Dialog|Popup|Tooltip|Alert|Notification|Progress|Skeleton|Login|Chat|AIChat")) return "Feedback & Overlays";
    if (Regex.IsMatch(name, "Card|Panel|Fieldset|Row|Column|Stack|Splitter|Carousel|QRCode|Barcode|Image|Icon|HtmlEditor|Markdown|Gravatar|RadzenBody|RadzenHeader|RadzenFooter|RadzenSidebar|RadzenLayout")) return "Layout & Display";
    return "Misc";
}

string CategoryRoute(string category)
{
    return category switch
    {
        "Navigation & Actions" => "/catalog/navigation",
        "Forms & Inputs" => "/catalog/forms-advanced",
        "Data & Scheduling" => "/catalog/data-advanced",
        "Data Visualization" => "/catalog/charts-advanced",
        "Feedback & Overlays" => "/catalog/overlays-advanced",
        "Layout & Display" => "/catalog/layout-advanced",
        _ => "/catalog/all-components"
    };
}

bool IsChildOrInternal(string name)
{
    if (!name.StartsWith("Radzen", StringComparison.Ordinal))
    {
        return true;
    }

    if (childSuffix.IsMatch(name))
    {
        return true;
    }

    if (name.Contains("Overlay", StringComparison.Ordinal) ||
        name.Contains("Dialog", StringComparison.Ordinal) && name != "RadzenDialog" ||
        name.Contains("Container", StringComparison.Ordinal) ||
        name.Contains("Wrapper", StringComparison.Ordinal) ||
        name.Contains("View", StringComparison.Ordinal) &&
        name is not ("RadzenScheduler" or "RadzenGantt" or "RadzenTimeline"))
    {
        return true;
    }

    foreach (var prefix in childPrefixes)
    {
        if (name.StartsWith(prefix, StringComparison.Ordinal) && !forcedStandalone.Contains(name))
        {
            return true;
        }
    }

    return false;
}

string? FindRoute(string razorContent)
{
    foreach (var line in razorContent.Split('\n'))
    {
        var trimmed = line.Trim();
        if (trimmed.StartsWith("@page \"", StringComparison.Ordinal))
        {
            return trimmed.Replace("@page \"", string.Empty).TrimEnd('"');
        }
    }

    return null;
}

var rows = new List<(string Component, string Category, string Mode, string Demo, string ThemeStatus)>();

foreach (var t in types)
{
    var name = t.Name;
    var isChild = IsChildOrInternal(name);
    var parent = isChild ? childSuffix.Replace(name, string.Empty) : string.Empty;
    var demo = string.Empty;

    foreach (var file in catalogFiles)
    {
        var fileName = Path.GetFileName(file);
        var content = catalogContent[fileName];
        if (Regex.IsMatch(content, $"<\\s*{Regex.Escape(name)}\\b"))
        {
            demo = FindRoute(content) ?? string.Empty;
            break;
        }
    }

    if (string.IsNullOrWhiteSpace(demo) && isChild && !string.IsNullOrWhiteSpace(parent))
    {
        var candidate = "Radzen" + parent;
        foreach (var file in catalogFiles)
        {
            var fileName = Path.GetFileName(file);
            var content = catalogContent[fileName];
            if (Regex.IsMatch(content, $"<\\s*{Regex.Escape(candidate)}\\b"))
            {
                var route = FindRoute(content) ?? "?";
                demo = $"child of {candidate} ({route})";
                break;
            }
        }
    }

    if (string.IsNullOrWhiteSpace(demo) && isChild)
    {
        var defaultParent = name.StartsWith("RadzenHtmlEditor", StringComparison.Ordinal) ? "RadzenHtmlEditor" :
            name.StartsWith("RadzenDataGrid", StringComparison.Ordinal) ? "RadzenDataGrid" :
            name.StartsWith("RadzenScheduler", StringComparison.Ordinal) ? "RadzenScheduler" :
            name.StartsWith("RadzenChart", StringComparison.Ordinal) || name.StartsWith("RadzenSeries", StringComparison.Ordinal) ? "RadzenChart" :
            name.StartsWith("RadzenSpreadsheet", StringComparison.Ordinal) ? "RadzenSpreadsheet" :
            name.StartsWith("RadzenTable", StringComparison.Ordinal) ? "RadzenTable" :
            "Radzen component family";
        demo = $"child of {defaultParent} ({CategoryRoute(Category(name))})";
    }

    if (string.IsNullOrWhiteSpace(demo) && serviceBound.Contains(name))
    {
        demo = "external-service component (placeholder required)";
    }

    if (string.IsNullOrWhiteSpace(demo))
    {
        demo = CategoryRoute(Category(name));
    }

    rows.Add((
        name,
        Category(name),
        isChild ? "Child" : "Standalone",
        string.IsNullOrWhiteSpace(demo) ? "MISSING" : demo,
        "Plum Ink pending"
    ));
}

var inventoryPath = Path.Combine(repo, "docs", "RADZEN-COMPONENT-INVENTORY.md");
var missingPath = Path.Combine(repo, "docs", "RADZEN-MISSING-STANDALONE.txt");
var sb = new StringBuilder();
sb.AppendLine("# Radzen Component Inventory");
sb.AppendLine();
sb.AppendLine($"Source assembly: {dllPath}");
sb.AppendLine($"Radzen.Blazor version: {version}");
sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
sb.AppendLine();
sb.AppendLine($"Total components: {rows.Count}");
var missingDemoCount = rows.Count(r => r.Demo == "MISSING");
sb.AppendLine($"Missing demos: {missingDemoCount}");
sb.AppendLine();
sb.AppendLine("| Component | Category | Standalone/Child | Demo page | Theme status |");
sb.AppendLine("|---|---|---|---|---|");
foreach (var row in rows.OrderBy(r => r.Category).ThenBy(r => r.Component, StringComparer.Ordinal))
{
    sb.AppendLine($"| {row.Component} | {row.Category} | {row.Mode} | {row.Demo} | {row.ThemeStatus} |");
}
await File.WriteAllTextAsync(inventoryPath, sb.ToString());

var missingStandalone = rows
    .Where(r => r.Mode == "Standalone" && r.Demo == "MISSING")
    .Select(r => r.Component)
    .OrderBy(s => s, StringComparer.Ordinal)
    .ToArray();
await File.WriteAllLinesAsync(missingPath, missingStandalone);

Console.WriteLine($"Generated: {inventoryPath}");
Console.WriteLine($"Missing standalone count: {missingStandalone.Length}");
foreach (var item in missingStandalone.Take(200))
{
    Console.WriteLine(item);
}
