using Agterhuis.Ui.Demo.Components;
using Agterhuis.Ui.Extensions;
using Agterhuis.Ui.Designer.Extensions;
using Agterhuis.Ui.Options;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Options;
using System.Globalization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddDesigner();
builder.Services.AddScoped<Agterhuis.Ui.Demo.Services.ShowcaseDataService>();
builder.Services.AddScoped<Agterhuis.Ui.Demo.Services.BlogShowcaseService>();
builder.Services.AddScoped<Agterhuis.Ui.Demo.Services.LocalDesignStore>();
builder.Services.AddSingleton<Agterhuis.Ui.Demo.Services.DemoSourceProvider>();

await using var app = builder.Build();

var uiOptions = app.Services.GetRequiredService<IOptions<AgtUiOptions>>().Value;
var defaultCulture = new CultureInfo(uiOptions.DefaultCulture);
CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;

await app.RunAsync();
