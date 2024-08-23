using System;
using System.Collections.Generic;
using JObject = Newtonsoft.Json.Linq.JObject;

namespace RedditMonitoringApp
{
    class StatisticsService
    {
        private Dictionary<string, int> _topPosts = new Dictionary<string, int>();
        private Dictionary<string, int> _userPostsCount = new Dictionary<string, int>();
        private HashSet<string> _processedPostIds = new HashSet<string>();


        public void UpdateStatistics(List<JObject> posts)
        {
            foreach (var post in posts)
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
                    if (!_userPostsCount.ContainsKey(author))
                    {
                        _userPostsCount[author] = 0;
                    }
                    _userPostsCount[author]++;
                }
 
            }
        }
        
        public void ConsoleWriteTopPosts()
        {
            Console.WriteLine("These are the top posts");
            foreach (var post in _topPosts)
            {
                Console.WriteLine($"Upvotes: {post.Value}, Post: {post.Key}");
            }
        }

        public void WriteMostActiveUsers()
        {
            Console.WriteLine("These are the most active users");
            foreach (var author in _userPostsCount)
            {
                Console.WriteLine($"Author: {author.Key}, Posts: {author.Value}");
            }
        }
        
    }
}
