namespace Agterhuis.Ui.Designer.Persistence;

using Agterhuis.Ui.Designer.Model;

public sealed record DesignDocumentEnvelope(
    string Name,
    int Version,
    string ETag,
    DateTimeOffset LastModified,
    DesignDocument Document);

public sealed record DesignListItem(
    string Name,
    DateTimeOffset LastModified,
    int CurrentVersion);

public sealed record DesignVersionInfo(
    int Version,
    DateTimeOffset Created,
    long SizeBytes);

public enum DesignerPersistenceMode
{
    Auto,
    Local,
    Remote
}

public sealed class DesignConflictException : Exception
{
    public DesignConflictException(string message, DesignDocumentEnvelope? serverEnvelope = null, Exception? innerException = null)
        : base(message, innerException)
    {
        ServerEnvelope = serverEnvelope;
    }

    public DesignDocumentEnvelope? ServerEnvelope { get; }
}
