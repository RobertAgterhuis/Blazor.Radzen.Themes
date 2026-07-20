using Agterhuis.Ui.Designer.Model;

namespace Agterhuis.Ui.Designer.Commands;

public interface IDesignDocumentCommand
{
    string Name { get; }

    bool Apply(DesignDocument document);
}
