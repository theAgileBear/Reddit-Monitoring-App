using System;
using System.Threading;
using System.Threading.Tasks;

namespace RedditMonitoringApp
{
    class RedditMonitoringService : IRedditMonitoringService
    {
        private readonly IApiClient _apiClient;
        private IStatisticsService _statsService = new StatisticsService();

        public RedditMonitoringService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public RedditMonitoringService(IApiClient apiClient, IStatisticsService statsService)
        {
            _apiClient = apiClient;
            _statsService = statsService;
        }

        /// <summary>
        /// This method runs an infinite loop that continuously monitors a specific subreddit.
        /// Manages the Reddit API rate limits by checking the remaining requests and the reset period.
        /// Introduces a delay between requests to avoid exceeding the rate limit, 
        /// using an optimized interval calculation based on the rate limit headers.
        /// If the rate limit is exhausted, 
        /// the method waits for the entire reset period before making further requests.
        /// </summary>
        /// <param name="subreddit">string of the subreddit</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task RunMonitoringLoopAsync(string subreddit, CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var posts = await _apiClient.GetPostsAsync(subreddit);

                    // Process posts
                    _statsService.UpdateStatistics(posts);

                    _statsService.ConsoleWriteTopPosts();
                    _statsService.WriteMostActiveUsers();

                    int rateLimitRemaining = _apiClient.GetRateLimitRemaining();
                    int rateLimitReset = _apiClient.GetRateLimitReset();

                    if (rateLimitRemaining == 0)
                    {
                        Console.WriteLine($"Rate limit exceeded, waiting {rateLimitReset} seconds...");
                        await Task.Delay(rateLimitReset * 1000 + 1, cancellationToken); //+1 is needed to ensure if rateResetLimit is 0 there is a slight delay
                    }
                    else
                    {
                        //create bufferedInterval and convert it to milliseconds
                        int bufferedInterval = (rateLimitReset / rateLimitRemaining + 1) * 1000;
                        Console.WriteLine($"Buffered interval for next request: {bufferedInterval / 1000} seconds");
                        await Task.Delay(bufferedInterval, cancellationToken);
                    }
                }
            }
            catch (TaskCanceledException e)
            {
                Console.WriteLine("Cancelled monitoring task" + e);
            }
        }
    }
}
