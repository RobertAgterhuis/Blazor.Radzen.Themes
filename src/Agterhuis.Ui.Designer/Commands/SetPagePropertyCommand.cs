using Agterhuis.Ui.Designer.Model;

namespace Agterhuis.Ui.Designer.Commands;

public sealed class SetPagePropertyCommand : IDesignDocumentCommand
{
    public SetPagePropertyCommand(int pageIndex, string propertyName, string? value)
    {
        if (pageIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageIndex));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        PageIndex = pageIndex;
        PropertyName = propertyName;
        Value = value;
    }

    public int PageIndex { get; }

    public string PropertyName { get; }

    public string? Value { get; }

    public string Name => "Set page property";

    public bool Apply(DesignDocument document)
    {
        if (PageIndex >= document.Pages.Count)
        {
            return false;
        }

        var page = document.Pages[PageIndex];
        var normalized = Value?.Trim();

        if (string.Equals(PropertyName, nameof(DesignPage.Route), StringComparison.Ordinal))
        {
            page.Route = string.IsNullOrWhiteSpace(normalized) ? "/" : normalized;
            return true;
        }

        if (string.Equals(PropertyName, nameof(DesignPage.Title), StringComparison.Ordinal))
        {
            page.Title = normalized ?? string.Empty;
            return true;
        }

        return false;
    }
}