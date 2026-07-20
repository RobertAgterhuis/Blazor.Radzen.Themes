using Agterhuis.Ui.Designer.Model;

namespace Agterhuis.Ui.Tests;

public sealed class DesignDataModelSeederTests
{
    [Fact]
    public void CreateDefault_SeedsTheExpectedDomainEntities()
    {
        var model = DesignDataModelSeeder.CreateDefault();

        Assert.Equal(42, model.Seed);
        Assert.Equal(25, model.RowCount);
        Assert.Equal(["Schadedossier", "Klant", "Voertuig", "Werkorder", "Factuur", "Voorraad"], model.Entities.Select(entity => entity.Name).ToArray());
        Assert.All(model.Entities, entity => Assert.NotEmpty(entity.Fields));
    }

    [Fact]
    public void GeneratePreview_ReturnsDeterministicRows()
    {
        var model = DesignDataModelSeeder.CreateDefault();

        var first = DesignDataModelSeeder.GeneratePreview(model, "Schadedossier");
        var second = DesignDataModelSeeder.GeneratePreview(model, "Schadedossier");

        Assert.Equal(5, first.Count);
        Assert.Equal(
            first.Select(row => string.Join("|", row.Values.OrderBy(item => item.Key, StringComparer.Ordinal).Select(item => $"{item.Key}={item.Value}"))),
            second.Select(row => string.Join("|", row.Values.OrderBy(item => item.Key, StringComparer.Ordinal).Select(item => $"{item.Key}={item.Value}"))));
        Assert.Contains(first[0].Values.Keys, key => string.Equals(key, "Dossiernummer", StringComparison.Ordinal));
    }
}