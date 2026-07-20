using Agterhuis.Ui.Extensions;
using Agterhuis.Ui.Options;
using Agterhuis.Ui.Templates.Components;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseStaticWebAssets();

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAgterhuisUi(options =>
{
    options.DefaultTheme = "THEME_FAMILY_TOKEN-THEME_VARIANT_TOKEN";
    options.EnableAmbientEffects = true;
});

var app = builder.Build();

var uiOptions = app.Services.GetRequiredService<IOptions<AgtUiOptions>>().Value;
var defaultCulture = new CultureInfo(uiOptions.DefaultCulture);
CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(defaultCulture),
    SupportedCultures = [defaultCulture],
    SupportedUICultures = [defaultCulture]
};

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRequestLocalization(localizationOptions);
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();