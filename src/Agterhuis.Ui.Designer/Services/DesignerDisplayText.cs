namespace Agterhuis.Ui.Designer.Services;

internal static class DesignerDisplayText
{
    private static readonly IReadOnlyDictionary<string, string> SlotDisplayNames = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["ChildContent"] = "Inhoud",
        ["HeaderActions"] = "Acties",
        ["Logo"] = "Logo",
        ["Sidebar"] = "Zijmenu",
        ["Header"] = "Koptekst",
        ["Footer"] = "Voettekst",
        ["Columns"] = "Kolommen",
        ["Template"] = "Sjabloon",
        ["EmptyTemplate"] = "Lege weergave",
        ["HeaderTemplate"] = "Kop-sjabloon",
        ["FooterTemplate"] = "Voet-sjabloon",
        ["SummaryTemplate"] = "Samenvatting",
        ["Start"] = "Start-inhoud",
        ["End"] = "Eind-inhoud",
        ["PageTitle"] = "Paginatitel",
        ["Body"] = "Hoofdinhoud",
        ["Tabs"] = "Tabbladen",
        ["Items"] = "Items",
        ["Content"] = "Inhoud",
        ["Actions"] = "Acties",
        ["Icon"] = "Icoon",
        ["Prefix"] = "Prefix",
        ["Suffix"] = "Suffix",
        ["ValueTemplate"] = "Waarde-sjabloon",
        ["GroupHeaderTemplate"] = "Groepskop-sjabloon",
        ["EditTemplate"] = "Bewerk-sjabloon",
        ["FilterTemplate"] = "Filter-sjabloon",
        ["TitleTemplate"] = "Titel-sjabloon",
        ["DetailRowTemplate"] = "Detail-rij sjabloon"
    };

    public static string GetSlotDisplayName(string slotName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slotName);
        return SlotDisplayNames.TryGetValue(slotName, out var displayName) ? displayName : slotName;
    }
}