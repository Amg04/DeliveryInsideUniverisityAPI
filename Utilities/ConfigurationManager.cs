// Singleton pattern 
public sealed class ConfigurationManager
{
    private static readonly ConfigurationManager instance;

    private Dictionary<string, string> settings;

    static ConfigurationManager()
    {
        instance = new ConfigurationManager();
    }

    private ConfigurationManager()
    {
        settings = new Dictionary<string, string>
        {
            { "AppName", "DeliveryApp" },
            { "Version", "v1" }
        };
    }
    public static ConfigurationManager Instance => instance;

    public string GetSetting(string key)
    {
        return settings.ContainsKey(key) ? settings[key] : null;
    }
}
