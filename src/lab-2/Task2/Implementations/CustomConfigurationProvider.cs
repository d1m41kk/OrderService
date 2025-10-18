using Microsoft.Extensions.Configuration;
using Task1.Models;

namespace Task2.Implementations;

public class CustomConfigurationProvider : ConfigurationProvider
{
    public bool DoReload(IEnumerable<ConfigurationItemDto> items)
    {
        var newConfigs = items.ToDictionary(kv => kv.Key, string? (kv) => kv.Value);
        if (AreDictionariesEquals(Data, newConfigs))
        {
            return false;
        }

        Data = newConfigs;
        OnReload();
        return true;
    }

    private static bool AreDictionariesEquals(IDictionary<string, string?> first, Dictionary<string, string?> second)
    {
        return first.Count == second.Count && first.All(valuePair => valuePair.Value == second[valuePair.Key]);
    }
}