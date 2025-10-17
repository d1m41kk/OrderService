using Microsoft.Extensions.Configuration;
using Task1.Models;

namespace Task2.Implementations;

public class CustomConfigurationProvider : ConfigurationProvider
{
    private Dictionary<string, string>? _lastConfigurations;

    public bool DoReload(QueryConfigurationsResponse? configurations)
    {
        var configurationsSorted = configurations?.Items
            .OrderBy(x => x.Key + x.Value).ToDictionary(x => x.Key, x => x.Value);
        if (_lastConfigurations != null && configurationsSorted != null && Equals(_lastConfigurations, configurationsSorted))
        {
            return false;
        }

        _lastConfigurations = configurationsSorted;
        if (_lastConfigurations != null)
        {
            Data = _lastConfigurations.ToDictionary(kv => kv.Key, string? (kv) => kv.Value);
        }

        OnReload();
        return true;
    }

    public override void Load()
    {
        _lastConfigurations = new Dictionary<string, string>();
    }

    private static bool Equals(Dictionary<string, string> first, Dictionary<string, string> second)
    {
        return first.Count == second.Count && first.All(valuePair => valuePair.Value == second[valuePair.Key]);
    }
}