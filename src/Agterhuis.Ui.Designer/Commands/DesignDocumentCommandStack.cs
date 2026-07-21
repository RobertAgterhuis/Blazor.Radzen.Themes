using Agterhuis.Ui.Designer.Model;
using Agterhuis.Ui.Designer.Serialization;

namespace Agterhuis.Ui.Designer.Commands;

public sealed class DesignDocumentCommandStack
{
    private readonly List<CommandHistoryEntry> _undo = [];
    private readonly List<CommandHistoryEntry> _redo = [];

    private string _savedSnapshot;

    public event EventHandler<DesignDocumentChangedEventArgs>? DocumentChanged;

    public DesignDocumentCommandStack(DesignDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        Document = DesignDocumentMigrator.Migrate(document);
        _savedSnapshot = DesignDocumentSerializer.Serialize(Document);
    }

    public DesignDocument Document { get; private set; }

    public bool CanUndo => _undo.Count > 0;

    public bool CanRedo => _redo.Count > 0;

    public bool IsDirty => !string.Equals(_savedSnapshot, DesignDocumentSerializer.Serialize(Document), StringComparison.Ordinal);

    public string? LastCommandName => _undo.Count > 0 ? _undo[^1].CommandName : null;

    public bool Execute(IDesignDocumentCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var before = DesignDocumentSerializer.Serialize(Document);
        var working = DesignDocumentSerializer.Deserialize(before);

        if (!command.Apply(working))
        {
            return false;
        }

        var after = DesignDocumentSerializer.Serialize(working);
        if (string.Equals(before, after, StringComparison.Ordinal))
        {
            return false;
        }

        Document = working;
        _undo.Add(new CommandHistoryEntry(command.Name, before, after));
        _redo.Clear();
        OnDocumentChanged(new DesignDocumentChangedEventArgs(command.Name));
        return true;
    }

    public bool Undo()
    {
        if (!CanUndo)
        {
            return false;
        }

        var entry = _undo[^1];
        _undo.RemoveAt(_undo.Count - 1);

        Document = DesignDocumentSerializer.Deserialize(entry.BeforeSnapshot);
        _redo.Add(entry);
        OnDocumentChanged(new DesignDocumentChangedEventArgs("Undo"));
        return true;
    }

    public bool Redo()
    {
        if (!CanRedo)
        {
            return false;
        }

        var entry = _redo[^1];
        _redo.RemoveAt(_redo.Count - 1);

        Document = DesignDocumentSerializer.Deserialize(entry.AfterSnapshot);
        _undo.Add(entry);
        OnDocumentChanged(new DesignDocumentChangedEventArgs("Redo"));
        return true;
    }

    public void MarkSaved()
    {
        _savedSnapshot = DesignDocumentSerializer.Serialize(Document);
    }

    public void ReplaceDocument(DesignDocument document, bool markSaved)
    {
        ArgumentNullException.ThrowIfNull(document);

        Document = DesignDocumentMigrator.Migrate(document);
        _undo.Clear();
        _redo.Clear();

        if (markSaved)
        {
            _savedSnapshot = DesignDocumentSerializer.Serialize(Document);
        }

        OnDocumentChanged(new DesignDocumentChangedEventArgs("ReplaceDocument"));
    }

    private void OnDocumentChanged(DesignDocumentChangedEventArgs args)
    {
        DocumentChanged?.Invoke(this, args);
    }

    private sealed record CommandHistoryEntry(string CommandName, string BeforeSnapshot, string AfterSnapshot);
}

public sealed record DesignDocumentChangedEventArgs(string Reason);
