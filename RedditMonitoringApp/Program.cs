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
            StartUpMessage();
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

        private static void StartUpMessage()
        {
            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine("|                                                           |");
            Console.WriteLine("|               Welcome to the Reddit Monitoring App        |");
            Console.WriteLine("|                                                           |");
            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine("\n\nHello!");
            Console.WriteLine("\nThis application tracks the latest posts from your selected subreddit in real-time.");
            Console.WriteLine("It efficiently manages Reddit's API rate limits while displaying the top posts and most active users.");
            Console.WriteLine("\n\nMake sure your 'config.json' is properly configured with your Reddit API credentials.");
            Console.WriteLine("\n\nInstructions: \n1. Please accept reddits API request for permission in your default browser (Press Allow)");
            Console.WriteLine("2. Then copy the localhost URL at the top of the browser.");
            Console.WriteLine("3. Right click then left click on the console window to paste.");
            Console.WriteLine("4. Press enter and Reddit Monitoring will begin!");
        }
    }
}