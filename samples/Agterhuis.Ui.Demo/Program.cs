using Agterhuis.Ui.Demo.Components;
using Agterhuis.Ui.Extensions;
using Agterhuis.Ui.Options;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseStaticWebAssets();

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAgterhuisUi();
builder.Services.AddScoped<Agterhuis.Ui.Demo.Services.ShowcaseDataService>();
builder.Services.AddScoped<Agterhuis.Ui.Demo.Services.BlogShowcaseService>();
builder.Services.AddSingleton<Agterhuis.Ui.Demo.Services.DemoSourceProvider>();

var app = builder.Build();

var uiOptions = app.Services.GetRequiredService<IOptions<AgtUiOptions>>().Value;
var defaultCulture = new CultureInfo(uiOptions.DefaultCulture);
CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;

var requestLocalizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(defaultCulture),
    SupportedCultures = [defaultCulture],
    SupportedUICultures = [defaultCulture]
};

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseRequestLocalization(requestLocalizationOptions);
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
