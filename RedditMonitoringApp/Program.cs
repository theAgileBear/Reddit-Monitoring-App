using RedditMonitoringApp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JObject = Newtonsoft.Json.Linq.JObject;

class Program
{
    private const string SUBREDDIT = "funny";
    private const int REQUEST_LIMIT = 1000000;

    static async Task Main(string[] args)
    {
        ApiClient apiClient = new ApiClient(REQUEST_LIMIT);
        RedditMonitoringService app = new RedditMonitoringService(apiClient);

        await app.RunMonitoringLoopAsync(SUBREDDIT);

       

    }


}