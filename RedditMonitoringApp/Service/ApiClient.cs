using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using JObject = Newtonsoft.Json.Linq.JObject;

namespace RedditMonitoringApp
{
    class ApiClient : IApiClient
    {
        private readonly string _clientId;
        private readonly string _redirectUri;
        private const string SCOPE_STRING = "read";
        private readonly string _userAgent;
        private static readonly string UniqueString = GenerateSimpleState();

        //default request limit to 100
        private int _requestLimit = 100;
        private int _rateLimitRemaining = 0;
        private int _rateLimitReset = 0;
        private string _accessToken = null;

        private readonly HttpClient _client;

        /// <summary>
        /// Initializes a new instance of the `ApiClient` class with a provided `HttpClient` instance.
        /// This constructor is useful for dependency injection.
        /// </summary>
        /// <param name="client">An instance of `HttpClient` used for making API requests.</param>
        public ApiClient(HttpClient client, ConfigLoader config)
        {
            _clientId = config.GetConfigValue("ClientId");
            _redirectUri = config.GetConfigValue("RedirectUri");
            _userAgent = config.GetConfigValue("UserAgent");
            _accessToken = string.IsNullOrEmpty(config.GetConfigValue("AccessToken")) ? null : config.GetConfigValue("AccessToken");

            ValidateConfigValues();
            _accessToken = GetAccessToken();
            _client = client;


        }

        /// <summary>
        /// Initializes a new instance of the `ApiClient` class with a specified request limit.
        /// This constructor creates a new `HttpClient` instance and sets up the necessary headers 
        /// for making requests to the Reddit API.
        /// </summary>
        /// <param name="requestLimit">The maximum number of posts to retrieve per request.</param>
        public ApiClient(int requestLimit, ConfigLoader config)
        {
            _requestLimit = requestLimit;
            _clientId = config.GetConfigValue("ClientId");
            _redirectUri = config.GetConfigValue("RedirectUri");
            _userAgent = config.GetConfigValue("UserAgent");
            _accessToken = string.IsNullOrEmpty(config.GetConfigValue("AccessToken")) ? null : config.GetConfigValue("AccessToken");

            ValidateConfigValues();



            _accessToken = GetAccessToken();
            _client = new HttpClient();

            _client.DefaultRequestHeaders.Add("User-Agent", _userAgent);
            _client.DefaultRequestHeaders.Add("Authorization", $"bearer {_accessToken}");
        }

        private void ValidateConfigValues()
        {

            // Check for null or empty values
            if (string.IsNullOrEmpty(_clientId) || string.IsNullOrEmpty(_redirectUri) || string.IsNullOrEmpty(_userAgent))
            {
                throw new InvalidOperationException("Critical configuration values are missing. The application will now close.");
            }
        }

        public async Task<JObject> GetPostsAsync(string subreddit)
        {
            try
            {
                //API requests with a bearer token should be made to https://oauth.reddit.com, NOT www.reddit.com.
                var url = $"https://oauth.reddit.com/r/{subreddit}/new.json?limit={_requestLimit}";
                var response = await _client.GetAsync(url);

                ShowRequestDataFromHeader(response);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JObject.Parse(content);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    HandleErrors(response, errorContent);

                }
            }
            catch (HttpRequestException ex)
            {
                // Handle network or other unexpected exceptions
                Console.WriteLine($"Request error: {ex.Message}");
                throw;
            }

            return null;
        }

        private void HandleErrors(HttpResponseMessage response, string errorContent)
        {
            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
            {
                Console.WriteLine($"Client error: {response.StatusCode} - {errorContent}");
                throw new HttpRequestException($"Client error: {response.StatusCode}. Check the request parameters.");
            }
            else if ((int)response.StatusCode >= 500)
            {
                Console.WriteLine($"Server error: {response.StatusCode} - {errorContent}");
                throw new HttpRequestException($"Server error: {response.StatusCode}. Try again later.");
                //TODO: create a retry mechanism for server error with exponential backoff
            }
        }

        public int GetRateLimitRemaining()
        {
            return _rateLimitRemaining;
        }

        public int GetRateLimitReset()
        {
            return _rateLimitReset;
        }

        private void ShowRequestDataFromHeader(HttpResponseMessage response)
        {

            Console.WriteLine($"Request Limit: {_requestLimit}");
            // Extract rate limit headers
            if (response.Headers.Contains("X-Ratelimit-Used"))
            {
                var rateLimitUsed = response.Headers.GetValues("X-Ratelimit-Used").FirstOrDefault();
                Console.WriteLine($"Rate Limit Used: {rateLimitUsed}");
            }

            if (response.Headers.Contains("X-Ratelimit-Remaining"))
            {
                var rateLimitRemaining = response.Headers.GetValues("X-Ratelimit-Remaining").FirstOrDefault();
                Console.WriteLine($"Rate Limit Remaining: {rateLimitRemaining}");
                _rateLimitRemaining = ConvertStringToInt(rateLimitRemaining);
            }

            if (response.Headers.Contains("X-Ratelimit-Reset"))
            {
                var rateLimitReset = response.Headers.GetValues("X-Ratelimit-Reset").FirstOrDefault();
                Console.WriteLine($"Rate Limit Reset In: {rateLimitReset} seconds");
                _rateLimitReset = ConvertStringToInt(rateLimitReset);
            }
        }

        private int ConvertStringToInt(string s)
        {
            int myInt;
            bool isConvertible = Int32.TryParse(s, out myInt);
            if (isConvertible) return myInt;
            else
            {
                decimal myDecimal;
                isConvertible = decimal.TryParse(s, out myDecimal);
                if (isConvertible) return Convert.ToInt32(myDecimal);
                else return 0;
            }
        }

        private string GetAccessToken()
        {
            if (_accessToken != null) return _accessToken;


            //Authorize your reddit application
            var authUrl = $"https://www.reddit.com/api/v1/authorize?client_id={_clientId}&response_type=token&state={UniqueString}&redirect_uri={_redirectUri}&duration=temporary&scope={SCOPE_STRING}";
            LaunchUrl(authUrl);

            // Get access token
            Console.WriteLine("Copy entire URL with Access Token here: ");
            string inputUrl = Console.ReadLine();
            string accessToken = ExtractAccessTokenFromUrl(inputUrl);

            if (!string.IsNullOrEmpty(accessToken))
            {
                Console.WriteLine("Access Token retreived successfully");
                // Use the access token to make API requests
                return accessToken;
            }
            else
            {
                Console.WriteLine("No access token found in the URL.");
                return null;
            }
        }

        /// <summary>
        /// Generates a random unqiue string
        /// </summary>
        /// <returns>GUID UTF-16 string</returns>
        private static string GenerateSimpleState()
        {
            return Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// Launches url in the users default browser
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
        /// <returns>Access token in string format. 
        /// Returns null if cannot extract access token from string format</returns>
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
