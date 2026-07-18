using Agterhuis.Ui.Extensions;
using Agterhuis.Ui.Services;
using Bunit;

namespace Agterhuis.Ui.Tests;

public sealed class AgtNotificationServiceTests
{
    [Fact]
    public void RegistersNotificationService()
    {
        using var ctx = new BunitContext();

        ctx.Services.AddAgterhuisUi();

        var registration = ctx.Services.GetService<IAgtNotificationService>();

        Assert.NotNull(registration);
    }

    [Fact]
    public void ConfirmDialogContractExposesDeleteConvenience()
    {
        var methods = typeof(IAgtConfirmDialog).GetMethods();
        var hasMethod = methods.Any(m => m.Name == nameof(IAgtConfirmDialog.ConfirmDeleteAsync));

        Assert.True(hasMethod);
    }
}
