using RedditMonitoringApp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JObject = Newtonsoft.Json.Linq.JObject;

class Program
{
    private const int DELAY_IN_MILLISECONDS = 10000;
    private const string SUBREDDIT = "funny";

    static async Task Main(string[] args)
    {

        var client = new ApiClient();
        var statsService = new StatisticsService();

        try
        {
            while (true)
            {
                var posts = await client.GetPosts(SUBREDDIT);
                var postsList = posts["data"]["children"].ToObject<List<JObject>>();

                statsService.UpdateStatistics(postsList);

                statsService.ConsoleWriteTopPosts();
                statsService.WriteMostActiveUsers();

                // Wait before the next poll
                await Task.Delay(DELAY_IN_MILLISECONDS);
            }
        } catch (Exception e)
        {
            Console.WriteLine($"Exception found: {e}");
        }
    }
   

}