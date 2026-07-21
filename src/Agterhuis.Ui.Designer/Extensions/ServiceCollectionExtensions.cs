using Agterhuis.Ui.Extensions;
using Agterhuis.Ui.Designer.Registry;
using Microsoft.Extensions.DependencyInjection;

namespace Agterhuis.Ui.Designer.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDesigner(this IServiceCollection services)
    {
        services.AddAgterhuisUi();
        services.AddSingleton(DesignerComponentRegistry.Instance);
        return services;
    }
}