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
        ["SummaryTemplate"] = "Samenvatting"
    };

    public static string GetSlotDisplayName(string slotName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slotName);
        return SlotDisplayNames.TryGetValue(slotName, out var displayName) ? displayName : slotName;
    }
}