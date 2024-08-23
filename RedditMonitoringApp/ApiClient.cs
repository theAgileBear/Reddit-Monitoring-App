using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using JObject = Newtonsoft.Json.Linq.JObject;

namespace RedditMonitoringApp
{
    class ApiClient
    {
        private const string CLIENT_ID = "m9GwE9xSSjcSCcByzRm7TQ";
        private const string REDIRECT_URI = "http://localhost:5000/callback";
        private const string SCOPE_STRING = "read";
        private const string USER_AGENT = "Monitoring-App By ScheduleSalt6538";
        private static readonly string UniqueString = GenerateSimpleState();
        private string _accessToken = null;

        private readonly HttpClient _client;

        public ApiClient()
        {
            if (_accessToken == null)
            {
                GetAccessToken();
            }
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);
            _client.DefaultRequestHeaders.Add("Authorization", $"bearer {_accessToken}");
        }

        public async Task<JObject> GetPosts(string subreddit)
        {
            //API requests with a bearer token should be made to https://oauth.reddit.com, NOT www.reddit.com.
            var url = $"https://oauth.reddit.com/r/{subreddit}/new.json?limit=25";
            var response = await _client.GetStringAsync(url);
            return JObject.Parse(response);
        }

        private void GetAccessToken()
        {
            //Authorize your reddit application
            var authUrl = $"https://www.reddit.com/api/v1/authorize?client_id={CLIENT_ID}&response_type=token&state={UniqueString}&redirect_uri={REDIRECT_URI}&duration=temporary&scope={SCOPE_STRING}";
            LaunchUrl(authUrl);

            // Get access token
            Console.WriteLine("Copy entire URL with Access Token here: ");
            string inputUrl = Console.ReadLine();
            string accessToken = ExtractAccessTokenFromUrl(inputUrl);

            if (!string.IsNullOrEmpty(accessToken))
            {
                Console.WriteLine("Access Token retreived successfully");
                // Use the access token to make API requests
                _accessToken = accessToken;
            }
            else
            {
                Console.WriteLine("No access token found in the URL.");
            }
        }
        /// <summary>
        /// Generates a random string based on GUID to ensure uniqueness
        /// </summary>
        /// <returns>String</returns>
        private static string GenerateSimpleState()
        {
            // Generates a random string based on a GUID to ensure
            return Guid.NewGuid().ToString("N");
        }
        /// <summary>
        /// Launches the <param name="url"></param> in the users default browser
        /// </summary>
        /// <param name="url"></param>
        private static void LaunchUrl(string url)
        {
            try
            {
                // Open the URL in the default web browser
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while trying to open the URL: " + ex.Message);
            }
        }
        /// <summary>
        /// Extracts the access token the given URL. Returns null if no string.
        /// </summary>
        /// <param name="url"></param>
        /// <returns>string</returns>
        private static string ExtractAccessTokenFromUrl(string url)
        {
            try
            {
                var uri = new Uri(url);
                var fragment = uri.Fragment; // Get the fragment part of the URL

                if (!string.IsNullOrEmpty(fragment))
                {
                    var fragments = System.Web.HttpUtility.ParseQueryString(fragment.TrimStart('#'));
                    return fragments.Get("access_token");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while extracting the access token: " + ex.Message);
            }

            return null;
        }
    }
}
