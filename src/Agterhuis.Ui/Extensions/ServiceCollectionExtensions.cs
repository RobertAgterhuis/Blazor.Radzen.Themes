using Agterhuis.Ui.Options;
using Agterhuis.Ui.Services;
using Agterhuis.Ui.Theming;
using Microsoft.Extensions.DependencyInjection;
using Radzen;

namespace Agterhuis.Ui.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAgterhuisUi(this IServiceCollection services, Action<AgtUiOptions>? configure = null)
    {
        services.AddRadzenComponents();
        services.AddScoped<AgtDensityState>();
        services.AddScoped<AgtThemeState>();
        services.AddScoped<IAgtCommandRegistry, AgtCommandRegistry>();
        services.AddScoped<IAgtConfirmDialog, AgtConfirmDialog>();
        services.AddScoped<IAgtNotificationService, AgtNotificationService>();

        if (configure is null)
        {
            services.AddOptions<AgtUiOptions>();
        }
        else
        {
            services.Configure(configure);
        }

        return services;
    }
}
