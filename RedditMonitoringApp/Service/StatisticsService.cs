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
        private Dictionary<string, Post> _processedPosts = new Dictionary<string, Post>(); // Maps Post ID to Post object
        private Dictionary<string, int> _topPosts = new Dictionary<string, int>();            // Maps Post Title to Upvotes
        private Dictionary<string, int> _topAuthors = new Dictionary<string, int>();          // Maps Author to Post Count
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
            foreach (var rawPost in postsList)
            {
                Post post = new Post(rawPost);

                // Update existing if post already exists in memory else create a new one
                if (_processedPosts.TryGetValue(post.Id, out Post existingPost))
                {
                    existingPost.Upvotes = post.Upvotes;
                    _topPosts[existingPost.Title] = existingPost.Upvotes;
                }
                else
                {
                    _processedPosts[post.Id] = post;
                    _topPosts[post.Title] = post.Upvotes;
                    if (_topAuthors.ContainsKey(post.Author))
                    {
                        _topAuthors[post.Author]++;
                    }
                    else
                    {
                        _topAuthors[post.Author] = 1;
                    }
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
