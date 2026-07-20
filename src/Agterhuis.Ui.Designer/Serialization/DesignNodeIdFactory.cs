using System.Security.Cryptography;
using System.Text;

namespace Agterhuis.Ui.Designer.Serialization;

internal static class DesignNodeIdFactory
{
    public static string Create(string route, string ancestry, string componentType)
    {
        var normalizedRoute = string.IsNullOrWhiteSpace(route) ? "/" : route.Trim();
        var normalizedComponent = string.IsNullOrWhiteSpace(componentType) ? "node" : componentType.Trim();
        var payload = $"{normalizedRoute}|{ancestry}|{normalizedComponent}";
        var hash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(payload)));
        return $"node-{hash[..12]}";
    }
}