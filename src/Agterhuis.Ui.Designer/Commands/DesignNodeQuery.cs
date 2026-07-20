using Agterhuis.Ui.Designer.Model;

namespace Agterhuis.Ui.Designer.Commands;

internal static class DesignNodeQuery
{
    internal sealed record NodeMatch(DesignNode Node, DesignNode? Parent, string SlotName, List<DesignNode> Container, int Index);

    public static bool TryFindNode(DesignPage page, string nodeId, out NodeMatch? match)
    {
        ArgumentNullException.ThrowIfNull(page);
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);

        for (var index = 0; index < page.Nodes.Count; index++)
        {
            if (TryFindNodeRecursive(page.Nodes[index], null, DesignNodeLocation.RootSlotName, page.Nodes, index, nodeId, out match))
            {
                return true;
            }
        }

        match = null;
        return false;
    }

    public static bool TryResolveContainer(DesignPage page, DesignNodeLocation location, out List<DesignNode>? container)
    {
        ArgumentNullException.ThrowIfNull(page);
        ArgumentNullException.ThrowIfNull(location);

        if (location.ParentNodeId is null)
        {
            if (!string.Equals(location.SlotName, DesignNodeLocation.RootSlotName, StringComparison.Ordinal))
            {
                container = null;
                return false;
            }

            container = page.Nodes;
            return true;
        }

        if (!TryFindNode(page, location.ParentNodeId, out var parentMatch) || parentMatch is null)
        {
            container = null;
            return false;
        }

        if (!parentMatch.Node.Children.TryGetValue(location.SlotName, out var children) || children is null)
        {
            children = [];
            parentMatch.Node.Children[location.SlotName] = children;
        }

        container = children;
        return true;
    }

    public static bool IsDescendant(DesignNode potentialDescendant, string ancestorNodeId)
    {
        ArgumentNullException.ThrowIfNull(potentialDescendant);
        ArgumentException.ThrowIfNullOrWhiteSpace(ancestorNodeId);

        foreach (var childList in potentialDescendant.Children.Values)
        {
            foreach (var child in childList)
            {
                if (string.Equals(child.Id, ancestorNodeId, StringComparison.Ordinal))
                {
                    return true;
                }

                if (IsDescendant(child, ancestorNodeId))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool TryFindNodeRecursive(
        DesignNode node,
        DesignNode? parent,
        string slotName,
        List<DesignNode> container,
        int index,
        string nodeId,
        out NodeMatch? match)
    {
        if (string.Equals(node.Id, nodeId, StringComparison.Ordinal))
        {
            match = new NodeMatch(node, parent, slotName, container, index);
            return true;
        }

        foreach (var slot in node.Children.OrderBy(static pair => pair.Key, StringComparer.Ordinal))
        {
            var children = slot.Value ?? [];
            node.Children[slot.Key] = children;

            for (var childIndex = 0; childIndex < children.Count; childIndex++)
            {
                if (TryFindNodeRecursive(children[childIndex], node, slot.Key, children, childIndex, nodeId, out match))
                {
                    return true;
                }
            }
        }

        match = null;
        return false;
    }
}
