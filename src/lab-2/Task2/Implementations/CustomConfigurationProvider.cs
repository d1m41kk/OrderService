using Microsoft.Extensions.Configuration;
using Task1.Models;

namespace Task2.Implementations;

public class CustomConfigurationProvider : ConfigurationProvider
{
    public bool DoReload(QueryConfigurationsResponse configurations)
    {
        var newConfigs = configurations?.Items?.ToDictionary(kv => kv.Key, string? (kv) => kv.Value);
        if (newConfigs != null && AreDictionariesEquals(Data, newConfigs))
        {
            return false;
        }

        Data = configurations?.Items?.ToDictionary(kv => kv.Key, string? (kv) => kv.Value)
               ?? new Dictionary<string, string?>();

        OnReload();
        return true;
    }

    public override void Load()
    {
        Data = new Dictionary<string, string?>();
    }

    private static bool AreDictionariesEquals(IDictionary<string, string?> first, Dictionary<string, string?> second)
    {
        return first.Count == second.Count && first.All(valuePair => valuePair.Value == second[valuePair.Key]);
    }
}