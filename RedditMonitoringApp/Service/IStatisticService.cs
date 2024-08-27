using Newtonsoft.Json.Linq;

/// <summary>
/// Provides functionality to track and display statistical information about from Reddit
/// </summary>
public interface IStatisticsService
{
    /// <summary>
    /// Updates the statistics for the top posts and most active authors based on the provided Reddit posts data.
    /// This method processes the incoming posts, updating internal dictionaries that track the 
    /// number of upvotes for each post and the number of posts by each author. 
    /// </summary>
    /// <param name="posts">A JSON object containing Reddit posts data.</param>
    void UpdateStatistics(JObject posts);

    /// <summary>
    /// Writes the top posts based on upvotes to the console.
    /// Displays a list of posts sorted by the number of upvotes in descending order,
    /// showing only the top entries up to the defined show limit.
    /// </summary>
    public void ConsoleWriteTopPosts();

    /// <summary>
    /// Writes the most active users based on the number of posts to the console.
    /// Displays a list of authors sorted by the number of posts they have made, 
    /// showing only the top entries up to the defined show limit.
    /// </summary>
    public void WriteMostActiveUsers();
}