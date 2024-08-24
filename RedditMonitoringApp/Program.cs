using RedditMonitoringApp;
using System;
using System.IO;
using System.Threading.Tasks;

class Program
{
    private const string SUBREDDIT = "funny";
    private const int REQUEST_LIMIT = 1000000;
    private const string CONFIG_FILENAME = "config.json";
    private static readonly string _configFilePath = 
        Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName, CONFIG_FILENAME);

    static async Task Main(string[] args)
    {
        ConfigLoader config = new ConfigLoader(_configFilePath);
        ApiClient apiClient = new ApiClient(REQUEST_LIMIT, config);
        RedditMonitoringService app = new RedditMonitoringService(apiClient);

        config.GetConfigValue("ClientId");

        await app.RunMonitoringLoopAsync(SUBREDDIT);
        Console.WriteLine("Completed reddit monitoring task, ending application!");
    }


}