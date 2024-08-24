using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditMonitoringApp
{
    /// <summary>
    /// Represents a Reddit post, encapsulating key information such as the post ID, author, title, upvotes, and creation timestamp.
    /// </summary>
    class Post
    {
        /// <summary>
        /// Gets or sets the unique identifier of the Reddit post.
        /// </summary>
        public string Id { get; set; }
        public string Author { get; set; }
        public string Title { get; set; }
        public int Upvotes { get; set; }
        public DateTime CreatedAt { get; set; }

        public Post(JObject rawPost)
        {
            var postData = rawPost["data"];
            Id = postData["id"].ToString();
            Author = postData["author"].ToString();
            Title = postData["title"].ToString();
            Upvotes = (int)postData["ups"];
            CreatedAt = DateTimeOffset.FromUnixTimeSeconds(postData["created_utc"].ToObject<long>()).UtcDateTime;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current post based on the post's unique ID.
        /// </summary>
        /// <param name="obj">The object to compare with the current post.</param>
        /// <returns>true if the specified object is equal to the current post; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (obj is Post other)
            {
                return this.Id == other.Id;
            }
            return false;
        }

        /// <summary>
        /// Serves as the default hash function, generating a hash code based on the post's unique ID.
        /// </summary>
        /// <returns>A hash code for the current post.</returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

    }

}
