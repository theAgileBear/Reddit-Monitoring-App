using System.Threading.Tasks;

namespace RedditMonitoringApp
{
    /// <summary>
    /// Defines a contract for a service that monitors a specific subreddit on Reddit.
    /// </summary>
    interface IRedditMonitoringService
    {
        /// <summary>
        /// This method runs an infinite loop that continuously monitors a specific subreddit.
        /// </summary>
        /// <param name="subreddit"></param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task RunMonitoringLoopAsync(string subreddit);
    }
}
