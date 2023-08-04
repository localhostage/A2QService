using Newtonsoft.Json;

namespace A2QService.Managers;

public class ConfigManager
{
    public A2QConfig Config { get; }
    public ConfigManager()
    {
        var configJson = File.ReadAllText("config.json");
        Config = JsonConvert.DeserializeObject<A2QConfig>(configJson);
    }
}