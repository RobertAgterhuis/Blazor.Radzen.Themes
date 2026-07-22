using Agterhuis.Ui.Designer.Model;

namespace Agterhuis.Ui.Designer.Services;

public sealed class DesignDataContext
{
    public DesignDataContext(DesignDataModel dataModel)
    {
        DataModel = dataModel ?? throw new ArgumentNullException(nameof(dataModel));
    }

    public DesignDataModel DataModel { get; }

    public bool IsDesignMode => true;

    public IReadOnlyList<DesignSeedRow> GetPreviewRows(string entityName, int maxRows = 10)
    {
        var previewRows = DesignDataModelSeeder.GeneratePreview(DataModel, entityName);
        return previewRows.Take(Math.Max(1, maxRows)).ToArray();
    }

    public object? GetSampleValue(string entityName, string fieldName)
    {
        var firstRow = GetPreviewRows(entityName, 1).FirstOrDefault();
        return firstRow is null ? null : firstRow.Values.GetValueOrDefault(fieldName);
    }
}