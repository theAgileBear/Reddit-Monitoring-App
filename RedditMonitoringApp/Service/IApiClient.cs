using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace RedditMonitoringApp
{
    /// <summary>
    /// Interface representing a client for interacting with the Reddit API.
    /// </summary>
    public interface IApiClient
    {
        /// <summary>
        /// Retrieves the remaining number of requests allowed before the rate limit is reached.
        /// </summary>
        /// <returns>The remaining number of API requests available before rate limit reset.</returns>
        int GetRateLimitRemaining();

        /// <summary>
        /// Retrieves the time (in seconds) until the rate limit resets.
        /// </summary>
        /// <returns>The number of seconds until the rate limit resets.</returns>
        int GetRateLimitReset();

        /// <summary>
        /// Asynchronously retrieves the latest posts from a specified subreddit.
        /// </summary>
        /// <param name="subreddit">The name of the subreddit from which to retrieve posts.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a 
        /// JObject representing the JSON response from the Reddit API. 
        /// Returns null and throws exception if status code is not success.</returns>
        /// <exception cref="HttpRequestException">
        /// Thrown when the HTTP request fails or the Reddit API responds with a non-success status code.
        /// </exception>
        Task<JObject> GetPostsAsync(string subreddit);
    }
}
