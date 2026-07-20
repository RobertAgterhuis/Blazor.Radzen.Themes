using Agterhuis.Ui.Demo.Components.Pages.Catalog;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Radzen;

namespace Agterhuis.Ui.Tests;

public sealed class CatalogPageSmokeTests
{
    [Fact]
    public void CatalogIndex_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogIndex>();
        Assert.Contains("Radzen catalogus (Radzen)", cut.Markup);
    }

    [Fact]
    public void ButtonsCatalog_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<ButtonsCatalog>();
        Assert.Contains("Buttons", cut.Markup);
    }

    [Fact]
    public void TextInputsCatalog_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<TextInputsCatalog>();
        Assert.Contains("Text Inputs", cut.Markup);
    }

    [Fact]
    public void SelectionInputsCatalog_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<SelectionInputsCatalog>();
        Assert.Contains("Selection Inputs", cut.Markup);
    }

    [Fact]
    public void PickersCatalog_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<PickersCatalog>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Pickers", cut.Markup);
    }

    [Fact]
    public void FormsCatalog_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<FormsCatalog>();
        Assert.Contains("Forms", cut.Markup);
    }

    [Fact]
    public void ValidatorsCatalog_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<ValidatorsCatalog>();
        Assert.Contains("Validators", cut.Markup);
    }

    [Fact]
    public void DataCatalog_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<DataCatalog>();
        Assert.Contains("Data", cut.Markup);
    }

    [Fact]
    public void SchedulingCatalog_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<SchedulingCatalog>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Scheduling", cut.Markup);
    }

    [Fact]
    public void NavigationCatalog_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<NavigationCatalog>();
        Assert.Contains("Navigation", cut.Markup);
    }

    [Fact]
    public void OverlaysCatalog_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<OverlaysCatalog>();
        Assert.Contains("Overlays", cut.Markup);
    }

    [Fact]
    public void LayoutCatalog_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<LayoutCatalog>();
        Assert.Contains("Layout", cut.Markup);
    }

    [Fact]
    public void FeedbackCatalog_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<FeedbackCatalog>();
        Assert.Contains("Feedback", cut.Markup);
    }

    [Fact]
    public void ChartsCatalog_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<ChartsCatalog>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Charts", cut.Markup);
    }

    [Fact]
    public void GaugesCatalog_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<GaugesCatalog>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Gauges", cut.Markup);
    }

    [Fact]
    public void DisplayCatalog_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<DisplayCatalog>();
        Assert.Contains("Display", cut.Markup);
    }

    [Fact]
    public void EmbedCatalog_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<EmbedCatalog>();
        Assert.Contains("Embed", cut.Markup);
    }

    [Fact]
    public void FormsAdvancedCatalog_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<FormsAdvancedCatalog>();
        Assert.Contains("Forms Advanced", cut.Markup);
    }

    [Fact]
    public void DataAdvancedCatalog_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<DataAdvancedCatalog>();
        Assert.Contains("Data Advanced", cut.Markup);
    }

    [Fact]
    public void OverlaysAdvancedCatalog_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<OverlaysAdvancedCatalog>();
        Assert.Contains("Overlays Advanced", cut.Markup);
    }

    [Fact]
    public void LayoutAdvancedCatalog_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<LayoutAdvancedCatalog>();
        Assert.Contains("Layout Advanced", cut.Markup);
    }

    [Fact]
    public void ChartsAdvancedCatalog_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<ChartsAdvancedCatalog>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Charts Advanced", cut.Markup);
    }

    [Fact]
    public void AllComponentsMatrixCatalog_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<AllComponentsCatalog>();
        Assert.Contains("RadzenAxisCrosshair", cut.Markup);
        Assert.Contains("RadzenComponents", cut.Markup);
        Assert.Contains("RadzenTicks", cut.Markup);
    }

    [Fact]
    public void StackedAreaSeriesComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogStackedAreaSeriesPage>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Chart preview", cut.Markup);
    }

    [Fact]
    public void StackedBarSeriesComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogStackedBarSeriesPage>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Chart preview", cut.Markup);
    }

    [Fact]
    public void StackedColumnSeriesComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogStackedColumnSeriesPage>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Chart preview", cut.Markup);
    }

    [Fact]
    public void StackedLineSeriesComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogStackedLineSeriesPage>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Chart preview", cut.Markup);
    }

    [Fact]
    public void TimelineComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogTimelinePage>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Timeline", cut.Markup);
    }

    [Fact]
    public void WaterfallSeriesComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogWaterfallSeriesPage>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Chart preview", cut.Markup);
    }

    [Fact]
    public void SignaturePadComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogSignaturePadPage>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Signature pad preview", cut.Markup);
    }

    [Fact]
    public void DataGridComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogDataGridPage>();
        Assert.Contains("DataGrid", cut.Markup);
    }

    [Fact]
    public void SchedulerComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogSchedulerPage>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Scheduler", cut.Markup);
    }

    [Fact]
    public void TextBoxComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogTextBoxPage>();
        Assert.Contains("TextBox", cut.Markup);
    }

    [Fact]
    public void AutoCompleteComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogAutoCompletePage>();
        Assert.Contains("AutoComplete", cut.Markup);
    }

    [Fact]
    public void DropDownComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogDropDownPage>();
        Assert.Contains("DropDown", cut.Markup);
    }

    [Fact]
    public void CheckBoxComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogCheckBoxPage>();
        Assert.Contains("CheckBox", cut.Markup);
    }

    [Fact]
    public void PasswordComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogPasswordPage>();
        Assert.Contains("Password", cut.Markup);
    }

    [Fact]
    public void TextAreaComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogTextAreaPage>();
        Assert.Contains("TextArea", cut.Markup);
    }

    [Fact]
    public void DatePickerComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogDatePickerPage>();
        Assert.Contains("DatePicker", cut.Markup);
    }

    [Fact]
    public void NumericComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogNumericPage>();
        Assert.Contains("Numeric", cut.Markup);
    }

    [Fact]
    public void SwitchComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogSwitchPage>();
        Assert.Contains("Switch", cut.Markup);
    }

    [Fact]
    public void FileInputComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogFileInputPage>();
        Assert.Contains("FileInput", cut.Markup);
    }

    [Fact]
    public void ColorPickerComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogColorPickerPage>();
        Assert.Contains("ColorPicker", cut.Markup);
    }

    [Fact]
    public void ListBoxComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogListBoxPage>();
        Assert.Contains("ListBox", cut.Markup);
    }

    [Fact]
    public void RatingComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogRatingPage>();
        Assert.Contains("Rating", cut.Markup);
    }

    [Fact]
    public void SliderComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogSliderPage>();
        Assert.Contains("Slider", cut.Markup);
    }

    [Fact]
    public void TimeSpanPickerComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogTimeSpanPickerPage>();
        Assert.Contains("TimeSpanPicker", cut.Markup);
    }

    [Fact]
    public void UploadComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogUploadPage>();
        Assert.Contains("Upload", cut.Markup);
    }

    [Fact]
    public void PickListComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogPickListPage>();
        Assert.Contains("PickList", cut.Markup);
    }

    [Fact]
    public void CheckBoxListComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogCheckBoxListPage>();
        Assert.Contains("CheckBoxList", cut.Markup);
    }

    [Fact]
    public void SecurityCodeComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogSecurityCodePage>();
        Assert.Contains("SecurityCode", cut.Markup);
    }

    [Fact]
    public void ChipComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogChipPage>();
        Assert.Contains("Chip", cut.Markup);
    }

    [Fact]
    public void ChipListComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogChipListPage>();
        Assert.Contains("ChipList", cut.Markup);
    }

    [Fact]
    public void MaskComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogMaskPage>();
        Assert.Contains("Mask", cut.Markup);
    }

    [Fact]
    public void NumericRangeValidatorComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogNumericRangeValidatorPage>();
        Assert.Contains("NumericRangeValidator", cut.Markup);
    }

    [Fact]
    public void TextComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogTextPage>();
        Assert.Contains("Text", cut.Markup);
    }

    [Fact]
    public void BarcodeComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogBarcodePage>();
        Assert.Contains("Barcode", cut.Markup);
    }

    [Fact]
    public void QRCodeComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogQRCodePage>();
        Assert.Contains("QRCode", cut.Markup);
    }

    [Fact]
    public void IconComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogIconPage>();
        Assert.Contains("Icon", cut.Markup);
    }

    [Fact]
    public void GravatarComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogGravatarPage>();
        Assert.Contains("Gravatar", cut.Markup);
    }

    [Fact]
    public void CardGroupComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogCardGroupPage>();
        Assert.Contains("CardGroup", cut.Markup);
    }

    [Fact]
    public void BodyChildRoute_RedirectsToLayoutComponent()
    {
        using var ctx = CreateContext();
        var nav = new TestNavigationManager("https://localhost/", "https://localhost/catalog/body");
        ctx.Services.AddSingleton<NavigationManager>(nav);
        _ = ctx.Render<CatalogBodyPage>();
        Assert.EndsWith("/catalog/layout-component#body", nav.Uri, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CardComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogCardPage>();
        Assert.Contains("Card", cut.Markup);
    }

    [Fact]
    public void FieldsetComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogFieldsetPage>();
        Assert.Contains("Fieldset", cut.Markup);
    }

    [Fact]
    public void FooterChildRoute_RedirectsToLayoutComponent()
    {
        using var ctx = CreateContext();
        var nav = new TestNavigationManager("https://localhost/", "https://localhost/catalog/footer");
        ctx.Services.AddSingleton<NavigationManager>(nav);
        _ = ctx.Render<CatalogFooterPage>();
        Assert.EndsWith("/catalog/layout-component#footer", nav.Uri, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void HeaderChildRoute_RedirectsToLayoutComponent()
    {
        using var ctx = CreateContext();
        var nav = new TestNavigationManager("https://localhost/", "https://localhost/catalog/header");
        ctx.Services.AddSingleton<NavigationManager>(nav);
        _ = ctx.Render<CatalogHeaderPage>();
        Assert.EndsWith("/catalog/layout-component#header", nav.Uri, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void LayoutComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogLayoutComponentPage>();
        Assert.Contains("Layout", cut.Markup);
    }

    [Fact]
    public void CompareValidatorComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogCompareValidatorPage>();
        Assert.Contains("CompareValidator", cut.Markup);
    }

    [Fact]
    public void CustomValidatorComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogCustomValidatorPage>();
        Assert.Contains("CustomValidator", cut.Markup);
    }

    [Fact]
    public void DataAnnotationValidatorComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogDataAnnotationValidatorPage>();
        Assert.Contains("DataAnnotationValidator", cut.Markup);
    }

    [Fact]
    public void EmailValidatorComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogEmailValidatorPage>();
        Assert.Contains("EmailValidator", cut.Markup);
    }

    [Fact]
    public void ColumnOptionsChildRoute_RedirectsToDataGrid()
    {
        using var ctx = CreateContext();
        var nav = new TestNavigationManager("https://localhost/", "https://localhost/catalog/column-options");
        ctx.Services.AddSingleton<NavigationManager>(nav);
        _ = ctx.Render<CatalogColumnOptionsPage>();
        Assert.EndsWith("/catalog/data-grid#column-options", nav.Uri, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GridRowChildRoute_RedirectsToDataGrid()
    {
        using var ctx = CreateContext();
        var nav = new TestNavigationManager("https://localhost/", "https://localhost/catalog/grid-row");
        ctx.Services.AddSingleton<NavigationManager>(nav);
        _ = ctx.Render<CatalogGridRowPage>();
        Assert.EndsWith("/catalog/data-grid#grid-row", nav.Uri, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ImageComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogImagePage>();
        Assert.Contains("Image", cut.Markup);
    }

    [Fact]
    public void MarkdownComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogMarkdownPage>();
        Assert.Contains("Markdown", cut.Markup);
    }

    [Fact]
    public void SidebarComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogSidebarPage>();
        Assert.Contains("Sidebar", cut.Markup);
    }

    [Fact]
    public void SidebarToggleComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogSidebarTogglePage>();
        Assert.Contains("SidebarToggle", cut.Markup);
    }

    [Fact]
    public void SplitterComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogSplitterPage>();
        Assert.Contains("Splitter", cut.Markup);
    }

    [Fact]
    public void SplitterPaneChildRoute_RedirectsToSplitter()
    {
        using var ctx = CreateContext();
        var nav = new TestNavigationManager("https://localhost/", "https://localhost/catalog/splitter-pane");
        ctx.Services.AddSingleton<NavigationManager>(nav);
        _ = ctx.Render<CatalogSplitterPanePage>();
        Assert.EndsWith("/catalog/splitter#splitter-pane", nav.Uri, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormFieldComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogFormFieldPage>();
        Assert.Contains("FormField", cut.Markup);
    }

    [Fact]
    public void LengthValidatorComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogLengthValidatorPage>();
        Assert.Contains("LengthValidator", cut.Markup);
    }

    [Fact]
    public void BreadCrumbComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogBreadCrumbPage>();
        Assert.Contains("BreadCrumb", cut.Markup);
    }

    [Fact]
    public void ButtonComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogButtonPage>();
        Assert.Contains("Button", cut.Markup);
    }

    [Fact]
    public void SplitButtonComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogSplitButtonPage>();
        Assert.Contains("SplitButton", cut.Markup);
    }

    [Fact]
    public void RadioButtonListComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogRadioButtonListPage>();
        Assert.Contains("RadioButtonList", cut.Markup);
    }

    [Fact]
    public void ProfileMenuComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogProfileMenuPage>();
        Assert.Contains("ProfileMenu", cut.Markup);
    }

    [Fact]
    public void RadioButtonListItemChildRoute_RedirectsToRadioButtonList()
    {
        using var ctx = CreateContext();
        var nav = new TestNavigationManager("https://localhost/", "https://localhost/catalog/radio-button-list-item");
        ctx.Services.AddSingleton<NavigationManager>(nav);
        _ = ctx.Render<CatalogRadioButtonListItemPage>();
        Assert.EndsWith("/catalog/radio-button-list#radio-button-list-item", nav.Uri, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ContextMenuComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogContextMenuPage>();
        Assert.Contains("ContextMenu", cut.Markup);
    }

    [Fact]
    public void LinkComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogLinkPage>();
        Assert.Contains("Link", cut.Markup);
    }

    [Fact]
    public void FabComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogFabPage>();
        Assert.Contains("Fab", cut.Markup);
    }

    [Fact]
    public void FabMenuComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogFabMenuPage>();
        Assert.Contains("FabMenu", cut.Markup);
    }

    [Fact]
    public void ToggleButtonComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogToggleButtonPage>();
        Assert.Contains("ToggleButton", cut.Markup);
    }

    [Fact]
    public void SpeechToTextButtonComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogSpeechToTextButtonPage>();
        Assert.Contains("SpeechToTextButton", cut.Markup);
    }

    [Fact]
    public void AccordionComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogAccordionPage>();
        Assert.Contains("Accordion", cut.Markup);
    }

    [Fact]
    public void PanelMenuComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogPanelMenuPage>();
        Assert.Contains("PanelMenu", cut.Markup);
    }

    [Fact]
    public void TabsComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogTabsPage>();
        Assert.Contains("Tabs", cut.Markup);
    }

    [Fact]
    public void StepsComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogStepsPage>();
        Assert.Contains("Steps", cut.Markup);
    }

    [Fact]
    public void RowComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogRowPage>();
        Assert.Contains("Row", cut.Markup);
    }

    [Fact]
    public void PanelComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogPanelPage>();
        Assert.Contains("Panel", cut.Markup);
    }

    [Fact]
    public void CarouselComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogCarouselPage>();
        Assert.Contains("Carousel", cut.Markup);
    }

    [Fact]
    public void AlertComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogAlertPage>();
        Assert.Contains("Alert", cut.Markup);
    }

    [Fact]
    public void ProgressBarComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogProgressBarPage>();
        Assert.Contains("ProgressBar", cut.Markup);
    }

    [Fact]
    public void ProgressBarCircularComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogProgressBarCircularPage>();
        Assert.Contains("ProgressBarCircular", cut.Markup);
    }

    [Fact]
    public void SkeletonComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogSkeletonPage>();
        Assert.Contains("Skeleton", cut.Markup);
    }

    [Fact]
    public void DialogComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogDialogPage>();
        Assert.Contains("Dialog", cut.Markup);
    }

    [Fact]
    public void NotificationComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogNotificationPage>();
        Assert.Contains("Notification", cut.Markup);
    }

    [Fact]
    public void PopupComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogPopupPage>();
        Assert.Contains("Popup", cut.Markup);
    }

    [Fact]
    public void LoginComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogLoginPage>();
        Assert.Contains("Login", cut.Markup);
    }

    [Fact]
    public void ChatComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogChatPage>();
        Assert.Contains("Chat", cut.Markup);
    }

    [Fact]
    public void AIChatComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogAIChatPage>();
        Assert.Contains("AIChat", cut.Markup);
    }

    [Fact]
    public void MenuComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogMenuPage>();
        Assert.Contains("Menu", cut.Markup);
    }

    [Fact]
    public void RequiredValidatorComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogRequiredValidatorPage>();
        Assert.Contains("RequiredValidator", cut.Markup);
    }

    [Fact]
    public void HtmlEditorComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogHtmlEditorPage>();
        Assert.Contains("HtmlEditor", cut.Markup);
    }

    [Fact]
    public void StackComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogStackPage>();
        Assert.Contains("Stack", cut.Markup);
    }

    [Fact]
    public void PagerComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogPagerPage>();
        Assert.Contains("Pager", cut.Markup);
    }

    [Fact]
    public void TreeComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogTreePage>();
        Assert.Contains("Tree", cut.Markup);
    }

    [Fact]
    public void DataListComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogDataListPage>();
        Assert.Contains("DataList", cut.Markup);
    }

    [Fact]
    public void DropDownDataGridComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogDropDownDataGridPage>();
        Assert.Contains("DropDownDataGrid", cut.Markup);
    }

    [Fact]
    public void RegexValidatorComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogRegexValidatorPage>();
        Assert.Contains("RegexValidator", cut.Markup);
    }

    [Fact]
    public void GoogleMapComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogGoogleMapPage>();
        Assert.Contains("GoogleMap", cut.Markup);
    }

    [Fact]
    public void BadgeComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogBadgePage>();
        Assert.Contains("Badge", cut.Markup);
    }

    [Fact]
    public void TemplateFormComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogTemplateFormPage>();
        Assert.Contains("TemplateForm", cut.Markup);
    }

    [Fact]
    public void SelectBarComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogSelectBarPage>();
        Assert.Contains("SelectBar", cut.Markup);
    }

    [Fact]
    public void MediaQueryComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogMediaQueryPage>();
        Assert.Contains("MediaQuery", cut.Markup);
    }

    [Fact]
    public void AppearanceToggleComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogAppearanceTogglePage>();
        Assert.Contains("AppearanceToggle", cut.Markup);
    }

    [Fact]
    public void TileLayoutComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogTileLayoutPage>();
        Assert.Contains("TileLayout", cut.Markup);
    }

    [Fact]
    public void TocComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogTocPage>();
        Assert.Contains("Toc", cut.Markup);
    }

    [Fact]
    public void DataFilterItemChildRoute_RedirectsToDataFilter()
    {
        using var ctx = CreateContext();
        var nav = new TestNavigationManager("https://localhost/", "https://localhost/catalog/data-filter-item");
        ctx.Services.AddSingleton<NavigationManager>(nav);
        _ = ctx.Render<CatalogDataFilterItemPage>();
        Assert.EndsWith("/catalog/data-filter#data-filter-item", nav.Uri, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DataFilterPropertyChildRoute_RedirectsToDataFilter()
    {
        using var ctx = CreateContext();
        var nav = new TestNavigationManager("https://localhost/", "https://localhost/catalog/data-filter-property");
        ctx.Services.AddSingleton<NavigationManager>(nav);
        _ = ctx.Render<CatalogDataFilterPropertyPage>();
        Assert.EndsWith("/catalog/data-filter#data-filter-property", nav.Uri, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DataFilterComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogDataFilterPage>();
        Assert.Contains("DataFilter", cut.Markup);
    }

    [Fact]
    public void DataListRowChildRoute_RedirectsToDataList()
    {
        using var ctx = CreateContext();
        var nav = new TestNavigationManager("https://localhost/", "https://localhost/catalog/data-list-row");
        ctx.Services.AddSingleton<NavigationManager>(nav);
        _ = ctx.Render<CatalogDataListRowPage>();
        Assert.EndsWith("/catalog/data-list#data-list-row", nav.Uri, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TableComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogTablePage>();
        Assert.Contains("Table", cut.Markup);
    }

    [Fact]
    public void TreemapComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogTreemapPage>();
        Assert.Contains("Treemap", cut.Markup);
    }

    [Fact]
    public void ArcGaugeComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogArcGaugePage>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("preview", cut.Markup, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AreaSeriesComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogAreaSeriesPage>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Chart preview", cut.Markup);
    }

    [Fact]
    public void BarSeriesComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogBarSeriesPage>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Chart preview", cut.Markup);
    }

    [Fact]
    public void BoxPlotSeriesComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogBoxPlotSeriesPage>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Chart preview", cut.Markup);
    }

    [Fact]
    public void BubbleSeriesComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogBubbleSeriesPage>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Chart preview", cut.Markup);
    }

    [Fact]
    public void BulletSeriesComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogBulletSeriesPage>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Chart preview", cut.Markup);
    }

    [Fact]
    public void CandlestickSeriesComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogCandlestickSeriesPage>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Chart preview", cut.Markup);
    }

    [Fact]
    public void ChartComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogChartPage>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Chart preview", cut.Markup);
    }

    [Fact]
    public void ColumnSeriesComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogColumnSeriesPage>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Chart preview", cut.Markup);
    }

    [Fact]
    public void ContourSeriesComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogContourSeriesPage>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Chart preview", cut.Markup);
    }

    [Fact]
    public void DonutSeriesComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogDonutSeriesPage>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Chart preview", cut.Markup);
    }

    [Fact]
    public void FullStackedAreaSeriesComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogFullStackedAreaSeriesPage>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Chart preview", cut.Markup);
    }

    [Fact]
    public void FullStackedBarSeriesComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogFullStackedBarSeriesPage>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Chart preview", cut.Markup);
    }

    [Fact]
    public void FullStackedColumnSeriesComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogFullStackedColumnSeriesPage>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Chart preview", cut.Markup);
    }

    [Fact]
    public void FullStackedLineSeriesComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogFullStackedLineSeriesPage>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Chart preview", cut.Markup);
    }

    [Fact]
    public void FunnelSeriesComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogFunnelSeriesPage>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Chart preview", cut.Markup);
    }

    [Fact]
    public void HeatmapSeriesComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogHeatmapSeriesPage>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Chart preview", cut.Markup);
    }

    [Fact]
    public void HighLowSeriesComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogHighLowSeriesPage>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Chart preview", cut.Markup);
    }

    [Fact]
    public void HorizontalWaterfallSeriesComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogHorizontalWaterfallSeriesPage>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Chart preview", cut.Markup);
    }

    [Fact]
    public void LineSeriesComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogLineSeriesPage>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Chart preview", cut.Markup);
    }

    [Fact]
    public void LinearGaugeComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogLinearGaugePage>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Gauge preview", cut.Markup);
    }

    [Fact]
    public void OhlcSeriesComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogOhlcSeriesPage>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Chart preview", cut.Markup);
    }

    [Fact]
    public void PieSeriesComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogPieSeriesPage>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Chart preview", cut.Markup);
    }

    [Fact]
    public void PyramidSeriesComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogPyramidSeriesPage>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Chart preview", cut.Markup);
    }

    [Fact]
    public void RadialGaugeComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogRadialGaugePage>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Gauge preview", cut.Markup);
    }

    [Fact]
    public void RangeAreaSeriesComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogRangeAreaSeriesPage>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Chart preview", cut.Markup);
    }

    [Fact]
    public void RangeBarSeriesComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogRangeBarSeriesPage>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Chart preview", cut.Markup);
    }

    [Fact]
    public void RangeColumnSeriesComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogRangeColumnSeriesPage>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Chart preview", cut.Markup);
    }

    [Fact]
    public void SankeyDiagramComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogSankeyDiagramPage>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Chart preview", cut.Markup);
    }

    [Fact]
    public void ScatterSeriesComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogScatterSeriesPage>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Chart preview", cut.Markup);
    }

    [Fact]
    public void SparklineComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogSparklinePage>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Chart preview", cut.Markup);
    }

    [Fact]
    public void SpiderChartComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogSpiderChartPage>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Chart preview", cut.Markup);
    }

    [Fact]
    public void SpiderColumnSeriesComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogSpiderColumnSeriesPage>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Chart preview", cut.Markup);
    }

    [Fact]
    public void SpiderSeriesComponentPage_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<CatalogSpiderSeriesPage>(p => p.Add(x => x.PreviewOnly, true));
        Assert.Contains("Chart preview", cut.Markup);
    }

    [Fact]
    public void AllComponentsCatalog_Renders()
    {
        using var ctx = CreateContext();
        var cut = ctx.Render<AllComponentsCatalog>();
        Assert.Contains("All Components", cut.Markup);
    }

    private static BunitContext CreateContext()
    {
        var ctx = new BunitContext();
        ctx.Services.AddRadzenComponents();
        ctx.Services.AddSingleton<Agterhuis.Ui.Demo.Services.DemoSourceProvider>();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
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
            Uri = ToAbsoluteUri(uri).ToString();
            NotifyLocationChanged(false);
        }
    }
}
