using System;
using System.Collections.Generic;
using System.Linq;
using JObject = Newtonsoft.Json.Linq.JObject;

namespace RedditMonitoringApp
{
    /// <summary>
    /// Provides functionality to track and display statistical information about from Reddit
    /// </summary>
    class StatisticsService
    {
        private Dictionary<string, int> _topPosts = new Dictionary<string, int>();
        private Dictionary<string, int> _topAuthors = new Dictionary<string, int>();
        private HashSet<string> _processedPostIds = new HashSet<string>();
        private const int SHOW_LIMIT = 10;

        /// <summary>
        /// Updates the statistics for the top posts and most active authors based on the provided Reddit posts data.
        /// This method processes the incoming posts, updating internal dictionaries that track the 
        /// number of upvotes for each post and the number of posts by each author. 
        /// /// </summary>
        /// <param name="posts">A JSON object containing Reddit posts data.</param>
        public void UpdateStatistics(JObject posts)
        {
            var postsList = posts["data"]["children"].ToObject<List<JObject>>();
            foreach (var post in postsList)
            {
                var postData = post["data"];
                var author = postData["author"].ToString();
                var postTitle = postData["title"].ToString();
                var upvotes = (int)postData["ups"];


                //Update top posts
                _topPosts[postTitle] = upvotes;

                //Update authors with the most posts without recounting already processed posts
                if (!_processedPostIds.Contains(postTitle))
                {
                    _processedPostIds.Add(postTitle);
                    if (!_topAuthors.ContainsKey(author))
                    {
                        _topAuthors[author] = 0;
                    }
                    _topAuthors[author]++;
                }
 
            }
        }
        /// <summary>
        /// Writes the top posts based on upvotes to the console.
        /// Displays a list of posts sorted by the number of upvotes in descending order,
        /// showing only the top entries up to the defined show limit.
        /// </summary>
        public void ConsoleWriteTopPosts()
        {
            var topPostsSorted = _topPosts.OrderByDescending(x => x.Value).ToList();
            Console.WriteLine("These are the top posts based on upvotes");
            for (int i = 0; i < SHOW_LIMIT; i++)
            {
                var post = topPostsSorted[i];
                Console.WriteLine($"Upvotes: {post.Value}, Post: {post.Key}");
            }
        }
        /// <summary>
        /// Writes the most active users based on the number of posts to the console.
        /// Displays a list of authors sorted by the number of posts they have made, 
        /// showing only the top entries up to the defined show limit.
        /// </summary>
        public void WriteMostActiveUsers()
        {
            var topAuthorsSorted = _topAuthors.OrderByDescending(x => x.Value).ToList();
            Console.WriteLine("These are the most active users based on posts");
            for (int i = 0; i < SHOW_LIMIT; i++)
            {
                var author = topAuthorsSorted[i];
                Console.WriteLine($"Author: {author.Key}, Posts: {author.Value}");
            }
        }
        
    }
}
