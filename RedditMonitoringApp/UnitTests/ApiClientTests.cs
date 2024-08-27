using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using System;
using System.Net.Http;
using System.Threading.Tasks;

using System.Threading;

namespace RedditMonitoringApp.UnitTests
{
    [TestClass]
    public class ApiClientTests
    {
        private Mock<HttpClient> _mockHttpClient;

        [TestInitialize]
        public void Setup()
        {
            _mockHttpClient = new Mock<HttpClient>();
        }

        [TestMethod]
        public void Constructor_ShouldInitializeConfigValuesCorrectly_NoErrorThrown()
        {
            //Arrange
            var mockConfigLoader = CreateMoqConfigLoader("mockedClientId", 
                "http://mocked.redirect.uri", "MockedUserAgent", "mockedAccessToken");

            //Act
            var apiClient = new ApiClient(_mockHttpClient.Object, mockConfigLoader.Object);

            //Assert
            // No exception means the configuration was loaded correctly
            Assert.IsNotNull(apiClient);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException),
            "Critical configuration values are missing. The application will now close.")]
        public void Constructor_WhenConfigValueClientIdIsMissing_ShouldThrowException()
        {
            // Arrange
            var mockConfigLoader = CreateMoqConfigLoader(null,
                 "http://mocked.redirect.uri", "MockedUserAgent", "mockedAccessToken");

            // Act
            var ApiClient = new ApiClient(_mockHttpClient.Object, mockConfigLoader.Object);

            //Assert done above with ExpectedException
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException),
            "Critical configuration values are missing. The application will now close.")]
        public void Constructor_WhenConfigValueRedirectUriIsMissing_ShouldThrowException()
        {
            // Arrange
            var mockConfigLoader = CreateMoqConfigLoader("mockedClientId",
                 null, "MockedUserAgent", "mockedAccessToken");

            // Act
            var ApiClient = new ApiClient(_mockHttpClient.Object, mockConfigLoader.Object);

            //Assert done above with ExpectedException
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException),
            "Critical configuration values are missing. The application will now close.")]
        public void Constructor_WhenConfigValueUserAgentIsMissing_ShouldThrowException()
        {
            // Arrange
            var mockConfigLoader = CreateMoqConfigLoader("mockedClientId",
                 "http://mocked.redirect.uri", null, "mockedAccessToken");

            // Act
            var ApiClient = new ApiClient(_mockHttpClient.Object, mockConfigLoader.Object);

            //Assert done above with ExpectedException
        }

        [TestMethod]
        public async Task GetPostsAsync_OnSuccess_ReturnsPosts()
        {
            // Arrange: Simulate a successful API response
            var fakeResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("{ 'data': { 'children': [] } }")
            };
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(fakeResponse);

            var mockConfigLoader = CreateDefaultMoqConfigLoader();
            var httpClient = new HttpClient(handlerMock.Object);
            var apiClient = new ApiClient(httpClient, mockConfigLoader.Object);

            // Act
            var result = await apiClient.GetPostsAsync("someSubreddit");

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpRequestException),
            "Client error: 400. Check the request parameters.")]
        public async Task GetPostsAsync_On400ClientError_ReturnsHttpRequestException()
        {
            // Arrange
            var fakeResponse = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest)
            {
                Content = new StringContent("Client error occurred")
            };
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(fakeResponse);

            var mockConfigLoader = CreateDefaultMoqConfigLoader();
            var httpClient = new HttpClient(handlerMock.Object);
            var apiClient = new ApiClient(httpClient, mockConfigLoader.Object);

            //Act
            var result = await apiClient.GetPostsAsync("someSubreddit");

            //Assert expected exception above
        }
        [TestMethod]
        [ExpectedException(typeof(HttpRequestException),
            "Server error: 500. Check the request parameters.")]
        public async Task GetPostsAsync_On500ServerError_ReturnsHttpRequestException()
        {
            // Arrange
            var fakeResponse = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("Client error occurred")
            };
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(fakeResponse);

            var mockConfigLoader = CreateDefaultMoqConfigLoader();
            var httpClient = new HttpClient(handlerMock.Object);
            var apiClient = new ApiClient(httpClient, mockConfigLoader.Object);

            //Act
            var result = await apiClient.GetPostsAsync("someSubreddit");

            //Assert expected exception above
        }

        private Mock<IConfigLoader> CreateMoqConfigLoader(string clientId, string redirectUri, string userAgent, string accessToken)
        {
            var moqConfigLoader = new Mock<IConfigLoader>();
            moqConfigLoader.Setup(cl => cl.GetConfigValue("ClientId")).Returns(clientId);
            moqConfigLoader.Setup(cl => cl.GetConfigValue("RedirectUri")).Returns(redirectUri);
            moqConfigLoader.Setup(cl => cl.GetConfigValue("UserAgent")).Returns(userAgent);
            moqConfigLoader.Setup(cl => cl.GetConfigValue("AccessToken")).Returns(accessToken);

            return moqConfigLoader;
        }
        private Mock<IConfigLoader> CreateDefaultMoqConfigLoader()
        {
            var moqConfigLoader = new Mock<IConfigLoader>();
            moqConfigLoader.Setup(cl => cl.GetConfigValue("ClientId")).Returns("Mocked Client Id");
            moqConfigLoader.Setup(cl => cl.GetConfigValue("RedirectUri")).Returns("http://mocked.redirect.uri");
            moqConfigLoader.Setup(cl => cl.GetConfigValue("UserAgent")).Returns("Mocked User Agent");
            moqConfigLoader.Setup(cl => cl.GetConfigValue("AccessToken")).Returns("Mocked Access Token");

            return moqConfigLoader;
        }
    }
}
