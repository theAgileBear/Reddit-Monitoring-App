using System.Threading;
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
    interface IRedditMonitoringService
    {
        /// <summary>
        /// This method runs an infinite loop that continuously monitors a specific subreddit.
        /// </summary>
        /// <param name="subreddit">string of the subreddit</param>
        /// /// <param name="cancellationToken">Cancel parameter to cancel the infinite loop from the outside of the class</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task RunMonitoringLoopAsync(string subreddit, CancellationToken cancellationToken);
    }
}
