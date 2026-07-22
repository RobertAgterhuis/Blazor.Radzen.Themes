namespace Agterhuis.Ui.Designer.Registry;

internal static class DesignerComponentDisplayMap
{
    private static readonly IReadOnlyDictionary<string, DesignerDisplayInfo> Entries =
        new Dictionary<string, DesignerDisplayInfo>(StringComparer.Ordinal)
        {
            ["AgtTextField"] = new("Tekstveld", "Invoer", "text_fields", "Een tekstveld voor vrije invoer."),
            ["AgtNumericField"] = new("Getallenveld", "Invoer", "pin", "Een veld voor numerieke invoer."),
            ["AgtDatePicker"] = new("Datumkiezer", "Invoer", "calendar_today", "Een veld om een datum te kiezen."),
            ["AgtDropdown"] = new("Keuzelijst", "Invoer", "arrow_drop_down_circle", "Een lijst om een keuze te maken."),
            ["AgtCheckbox"] = new("Aanvinkveld", "Invoer", "check_box", "Een veld voor een ja/nee keuze."),
            ["AgtSwitch"] = new("Schakelaar", "Invoer", "toggle_on", "Een compacte aan/uit schakelaar."),
            ["AgtAutoComplete"] = new("Zoek & selecteer", "Invoer", "manage_search", "Zoek en selecteer uit suggesties."),
            ["AgtFileUpload"] = new("Bestand uploaden", "Invoer", "upload_file", "Voeg een bestand toe aan het ontwerp."),
            ["AgtFormActions"] = new("Formulierknoppen", "Invoer", "done_all", "Actieknoppen zoals opslaan en annuleren."),
            ["AgtCard"] = new("Kaart", "Opmaak", "crop_portrait", "Een afgebakend inhoudsblok."),
            ["AgtBreadcrumb"] = new("Kruimelpad", "Opmaak", "arrow_right", "Navigatiepad naar de huidige pagina."),
            ["AgtDrawer"] = new("Zijpaneel", "Opmaak", "menu_open", "Een in- of uitschuivend paneel."),
            ["AgtNavLink"] = new("Navigatielink", "Navigatie", "link", "Navigeer naar een andere pagina in de app."),
            ["AgtDensityToggle"] = DesignerDisplayInfo.HiddenEntry,
            ["AgtCommandPalette"] = DesignerDisplayInfo.HiddenEntry,
            ["AgtAlert"] = new("Melding", "Meldingen", "info", "Toon een belangrijke melding."),
            ["AgtBadge"] = new("Badge", "Meldingen", "fiber_manual_record", "Een korte statusaanduiding."),
            ["AgtEmptyState"] = new("Lege staat", "Meldingen", "inbox", "Toon hulp bij lege inhoud."),
            ["AgtLoadingPanel"] = new("Laadpaneel", "Meldingen", "hourglass_empty", "Toon dat gegevens worden geladen."),
            ["AgtDelta"] = new("Verschil-indicator", "Meldingen", "trending_up", "Toon stijgingen of dalingen."),
            ["AgtDataGrid"] = new("Tabel", "Gegevens", "table_chart", "Toon data in een overzichtelijke tabel."),
            ["RadzenDataGrid"] = new("Tabel", "Gegevens", "table_chart", "Toon data in een overzichtelijke tabel.")
        };

    public static DesignerDisplayInfo? Resolve(string componentType)
        => Entries.TryGetValue(componentType, out var info) ? info : null;

    public static bool IsHiddenFromPalette(string componentType)
        => Entries.TryGetValue(componentType, out var info) && info.Hidden;

    internal sealed record DesignerDisplayInfo(string DisplayName, string Category, string Icon, string Description, bool Hidden = false)
    {
        public static DesignerDisplayInfo HiddenEntry { get; } = new(string.Empty, string.Empty, string.Empty, string.Empty, true);
    }
}
