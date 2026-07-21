namespace Agterhuis.Ui.Designer.Model;

public sealed class DesignEntityMetadata
{
    public string? Description { get; set; }

    public List<DesignEntityEndpointMetadata> Endpoints { get; set; } = [];
}

public sealed class DesignEntityEndpointMetadata
{
    public string Path { get; set; } = string.Empty;

    public string Method { get; set; } = string.Empty;
}
