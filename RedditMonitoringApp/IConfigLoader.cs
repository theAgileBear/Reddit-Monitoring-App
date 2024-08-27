using Newtonsoft.Json.Linq;

/// <summary>
/// Interface for loading configuration values from a configuration source.
/// Provides a method to retrieve configuration values based on a specified key.
/// </summary>
public interface IConfigLoader
{
    /// <summary>
    /// Gets the configuration value for the specified key.
    /// </summary>
    /// <param name="key">The key of the configuration value to retrieve.</param>
    /// <returns>The configuration value as a string.</returns>
    string GetConfigValue(string key);
}
