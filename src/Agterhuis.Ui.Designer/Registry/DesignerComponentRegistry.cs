namespace Agterhuis.Ui.Designer.Registry;

public sealed partial class DesignerComponentRegistry
{
    private static readonly Lazy<DesignerComponentRegistry> LazyInstance = new(BuildDefault);

    private readonly IReadOnlyDictionary<string, DesignerComponentDescriptor> _components;

    public DesignerComponentRegistry(IEnumerable<DesignerComponentDescriptor> components)
    {
        ArgumentNullException.ThrowIfNull(components);

        _components = components
            .GroupBy(static component => component.ComponentType, StringComparer.Ordinal)
            .ToDictionary(static group => group.Key, static group => group.First(), StringComparer.Ordinal);
    }

    public static DesignerComponentRegistry Instance => LazyInstance.Value;

    public IReadOnlyCollection<DesignerComponentDescriptor> Components => _components.Values.ToArray();

    public IReadOnlyDictionary<string, int> CountsByCategory => _components.Values
        .GroupBy(static component => component.Category, StringComparer.Ordinal)
        .OrderBy(static group => group.Key, StringComparer.Ordinal)
        .ToDictionary(static group => group.Key, static group => group.Count(), StringComparer.Ordinal);

    public DesignerComponentDescriptor GetDescriptor(string componentType)
    {
        if (!_components.TryGetValue(componentType, out var descriptor))
        {
            throw new KeyNotFoundException($"Unknown component type '{componentType}'.");
        }

        return descriptor;
    }

    public bool TryGetDescriptor(string componentType, out DesignerComponentDescriptor descriptor)
        => _components.TryGetValue(componentType, out descriptor!);

    private static partial DesignerComponentRegistry BuildDefault();
}