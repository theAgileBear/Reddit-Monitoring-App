using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RedditMonitoringApp
{
    /// <summary>
    /// Implements the functionality to monitor a specific subreddit on Reddit continuously.
    /// This class interacts with the Reddit API through the IApiClient interface to fetch posts 
    /// and updates statistical data using the StatisticsService. It is designed to handle the 
    /// Reddit API's rate limiting by adjusting the interval between successive API requests 
    /// to prevent exceeding the allowed number of requests within a specified time frame.
    /// </summary>
    class RedditMonitoringService : IRedditMonitoringService
    {
        private readonly IApiClient _apiClient;
        private StatisticsService _statsService = new StatisticsService();


        /// <summary>
        /// Initializes a new instance of the <see cref="RedditMonitoringService"/> class.
        /// Takes an IApiClient implementation as a dependency to handle API requests.
        /// </summary>
        /// <param name="apiClient">The client used to interact with the Reddit API.</param>

        public RedditMonitoringService(IApiClient apiClient)
        {
            _apiClient = apiClient;
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
        public async Task RunMonitoringLoopAsync(string subreddit)
        {
            while (true)
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
                    await Task.Delay(rateLimitReset * 1000);
                }
                else
                {
                    //create bufferedInterval and convert it to milliseconds
                    int bufferedInterval = (rateLimitReset / rateLimitRemaining + 1) * 1000;
                    Console.WriteLine($"Buffered interval for next request: {bufferedInterval/1000} seconds");
                    await Task.Delay(bufferedInterval);
                }
            }
        }
    }
}
