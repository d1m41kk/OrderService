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
        if (first.Count != second.Count)
        {
            return false;
        }

        foreach (KeyValuePair<string, string?> kvp in first)
        {
            if (!second.TryGetValue(kvp.Key, out string? secondValue))
            {
                return false;
            }

            if (kvp.Value != secondValue)
            {
                return false;
            }
        }

        return true;
    }
}