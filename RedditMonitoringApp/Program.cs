using System;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
namespace RedditMonitoringApp
{
    public class Program
    {
        private const string SUBREDDIT = "funny";
        private const int REQUEST_LIMIT = 100;  // Adjust to a reasonable limit
        private const string CONFIG_FILENAME = "config.json";
        private static readonly string _projectFolderPath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
        private static readonly string _configFilePath =
            Path.Combine(_projectFolderPath, CONFIG_FILENAME);

        public static async Task Main(string[] args)
        {
            try
            {
                ConfigLoader config = new ConfigLoader(_configFilePath);
                ApiClient apiClient = new ApiClient(REQUEST_LIMIT, config);
                RedditMonitoringService app = new RedditMonitoringService(apiClient);

                var cancellationSource = new CancellationTokenSource();

                _ = Task.Run(() =>
                  {
                      Console.WriteLine("Press 'c' to cancel...");
                      while (Console.ReadKey(true).Key != ConsoleKey.C) ;
                      cancellationSource.Cancel();
                  });

                await app.RunMonitoringLoopAsync(SUBREDDIT, cancellationSource.Token);

                Console.WriteLine("Completed reddit monitoring task, ending application!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}