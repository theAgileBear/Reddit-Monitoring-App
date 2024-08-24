using Newtonsoft.Json.Linq;
using System.IO;

public class ConfigLoader
{
    private readonly JObject _config;

    public ConfigLoader(string filePath)
    {
        var json = File.ReadAllText(filePath);
        _config = JObject.Parse(json);
    }

    public string GetConfigValue(string key)
    {
        return _config.SelectToken(key)?.ToString();
    }
}
