using Agterhuis.Ui.Components.Feedback;
using Agterhuis.Ui.Extensions;
using Agterhuis.Ui.Services;
using Bunit;

namespace Agterhuis.Ui.Tests;

public sealed class AgtConfirmDialogTests
{
    [Fact]
    public void RegistersConfirmDialogService()
    {
        using var ctx = new BunitContext();

        ctx.Services.AddAgterhuisUi();

        var registration = ctx.Services.GetService<IAgtConfirmDialog>();

        Assert.NotNull(registration);
    }

    [Fact]
    public void ConfirmOptionsDefaultsAreValid()
    {
        var options = new AgtConfirmOptions();

        Assert.Null(options.OkText);
        Assert.Null(options.CancelText);
        Assert.Equal(AgtIntent.Primary, options.Intent);
    }
}
