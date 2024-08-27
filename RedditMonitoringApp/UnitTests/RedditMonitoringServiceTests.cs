using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RedditMonitoringApp.UnitTests
{
    [TestClass]
    public class RedditMonitoringServiceTests
    {
        [TestMethod]
        public async Task RunMonitoringLoopAsync_GivenGoodPostData_ShouldFetchPostsAndUpdateStatistics()
        {
            // Arrange
            var mockApiClient = new Mock<IApiClient>();
            var mockStatsService = new Mock<IStatisticsService>();
            var service = new RedditMonitoringService(mockApiClient.Object, mockStatsService.Object);


            var fakePostData = CreateFakePostData();

            mockApiClient.Setup(client => client.GetPostsAsync(It.IsAny<string>()))
                          .ReturnsAsync(fakePostData);
            mockApiClient.Setup(client => client.GetRateLimitRemaining())
                          .Returns(10);
            mockApiClient.Setup(client => client.GetRateLimitReset())
                          .Returns(60);

            var cancellationTokenSource = new CancellationTokenSource();



            // Act
            var monitoringTask = service.RunMonitoringLoopAsync("someSubreddit", cancellationTokenSource.Token);
            await Task.Delay(1000);
            cancellationTokenSource.Cancel();
            await monitoringTask;


            // Assert
            mockApiClient.Verify(client => client.GetPostsAsync("someSubreddit"), Times.Once);
            mockStatsService.Verify(stats => stats.UpdateStatistics(fakePostData), Times.Once);
            mockStatsService.Verify(stats => stats.ConsoleWriteTopPosts(), Times.Once);
            mockStatsService.Verify(stats => stats.WriteMostActiveUsers(), Times.Once);
        }
        [TestMethod]
        public async Task RunMonitoringLoopAsync_WhenApiClientThrowsException_ShouldHandleGracefully()
        {
            // Arrange
            var mockApiClient = new Mock<IApiClient>();
            var service = new RedditMonitoringService(mockApiClient.Object);

            mockApiClient.Setup(client => client.GetPostsAsync(It.IsAny<string>()))
                          .ThrowsAsync(new HttpRequestException("API request failed"));

            var cancellationTokenSource = new CancellationTokenSource();


            // Act
            var monitoringTask = service.RunMonitoringLoopAsync("someSubreddit", cancellationTokenSource.Token);

            // Assert
            await Assert.ThrowsExceptionAsync<HttpRequestException>(() => monitoringTask);
        }
        [TestMethod]
        public async Task RunMonitoringLoopAsync_WhenApiClientReturnsNull_ShouldHandleGracefully()
        {
            // Arrange
            // Arrange
            var mockApiClient = new Mock<IApiClient>();
            var mockStatsService = new Mock<IStatisticsService>();
            var service = new RedditMonitoringService(mockApiClient.Object, mockStatsService.Object);

            mockApiClient.Setup(client => client.GetPostsAsync(It.IsAny<string>()))
                          .ReturnsAsync((JObject)null);

            var cancellationTokenSource = new CancellationTokenSource();


            // Act
            var monitoringTask = service.RunMonitoringLoopAsync("someSubreddit", cancellationTokenSource.Token);
            await Task.Delay(1000);
            cancellationTokenSource.Cancel();
            await monitoringTask;

            // Assert
            mockStatsService.Verify(stats => stats.UpdateStatistics(It.IsAny<JObject>()), Times.AtLeastOnce);
            mockStatsService.Verify(stats => stats.ConsoleWriteTopPosts(), Times.AtLeastOnce);
            mockStatsService.Verify(stats => stats.WriteMostActiveUsers(), Times.AtLeastOnce);
        }
        [TestMethod]
        public async Task RunMonitoringLoopAsync_CancellationRequested_ShouldStopLoopGracefully()
        {
            // Arrange
            var mockApiClient = new Mock<IApiClient>();
            var mockStatsService = new Mock<IStatisticsService>();
            var service = new RedditMonitoringService(mockApiClient.Object, mockStatsService.Object);

            mockApiClient.Setup(client => client.GetPostsAsync(It.IsAny<string>()))
                         .ReturnsAsync(new JObject());  // Return an empty JObject to simulate API response

            mockApiClient.Setup(client => client.GetRateLimitRemaining())
                         .Returns(10);

            mockApiClient.Setup(client => client.GetRateLimitReset())
                         .Returns(60);

            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            var monitoringTask = service.RunMonitoringLoopAsync("someSubreddit", cancellationTokenSource.Token);
            await Task.Delay(500);
            cancellationTokenSource.Cancel();
            await monitoringTask; 

            // Assert
            Assert.IsTrue(monitoringTask.IsCompleted);
            mockApiClient.Verify(client => client.GetPostsAsync("someSubreddit"), Times.AtLeastOnce);
            mockStatsService.Verify(stats => stats.UpdateStatistics(It.IsAny<JObject>()), Times.AtLeastOnce);
        }

        [TestMethod]
        public async Task RunMonitoringLoopAsync_WhenRateLimitExceeded_ShouldWaitForResetPeriod()
        {
            // Arrange
            var mockApiClient = new Mock<IApiClient>();
            var mockStatsService = new Mock<IStatisticsService>();
            var service = new RedditMonitoringService(mockApiClient.Object, mockStatsService.Object);

            var fakePostData = CreateFakePostData();

            mockApiClient.Setup(client => client.GetPostsAsync(It.IsAny<string>()))
                          .ReturnsAsync(fakePostData);
            mockApiClient.Setup(client => client.GetRateLimitRemaining())
                          .Returns(0);
            mockApiClient.Setup(client => client.GetRateLimitReset())
                          .Returns(10);

            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            var monitoringTask = service.RunMonitoringLoopAsync("someSubreddit", cancellationTokenSource.Token);
            await Task.Delay(500);
            cancellationTokenSource.Cancel();
            await monitoringTask;

            // Assert
            mockApiClient.Verify(client => client.GetPostsAsync("someSubreddit"), Times.Once);
            mockApiClient.Verify(client => client.GetRateLimitRemaining(), Times.Once);
            mockApiClient.Verify(client => client.GetRateLimitReset(), Times.Once);
        }
        [TestMethod]
        public async Task RunMonitoringLoopAsync_WithRateLimitRemaining_ShouldDelayByBufferedInterval()
        {
            // Arrange
            var mockApiClient = new Mock<IApiClient>();
            var mockStatsService = new Mock<IStatisticsService>();
            var service = new RedditMonitoringService(mockApiClient.Object, mockStatsService.Object);

            var idealRateLimitRemaining = 10;
            var idealRateLimitReset = 60;
            var idealBufferedInterval = (idealRateLimitReset / idealRateLimitRemaining + 1) * 1000;

            var fakePostData = CreateFakePostData();

            mockApiClient.Setup(client => client.GetPostsAsync(It.IsAny<string>()))
                          .ReturnsAsync(fakePostData);
            mockApiClient.Setup(client => client.GetRateLimitRemaining())
                          .Returns(idealRateLimitRemaining);
            mockApiClient.Setup(client => client.GetRateLimitReset())
                          .Returns(idealRateLimitReset);

            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            var monitoringTask = service.RunMonitoringLoopAsync("someSubreddit", cancellationTokenSource.Token);
            await Task.Delay(idealBufferedInterval);
            cancellationTokenSource.Cancel();
            await monitoringTask;

            // Assert
            mockApiClient.Verify(client => client.GetPostsAsync("someSubreddit"), Times.Exactly(2));
            mockApiClient.Verify(client => client.GetRateLimitRemaining(), Times.Exactly(2));
            mockApiClient.Verify(client => client.GetRateLimitReset(), Times.Exactly(2));
        }
        [TestMethod]
        public async Task RunMonitoringLoopAsync_WhenRateLimitDataIsMissing_ShouldHandleGracefully()
        {
            // Arrange
            var mockApiClient = new Mock<IApiClient>();
            var mockStatsService = new Mock<IStatisticsService>();
            var service = new RedditMonitoringService(mockApiClient.Object, mockStatsService.Object);

            var fakePostData = CreateFakePostData();

            mockApiClient.Setup(client => client.GetPostsAsync(It.IsAny<string>()))
                          .ReturnsAsync(fakePostData);
            mockApiClient.Setup(client => client.GetRateLimitRemaining())
                          .Returns(0);
            mockApiClient.Setup(client => client.GetRateLimitReset())
                          .Returns(0); 

            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            var monitoringTask = service.RunMonitoringLoopAsync("someSubreddit", cancellationTokenSource.Token);
            await Task.Delay(500);
            cancellationTokenSource.Cancel();
            await monitoringTask;

            // Assert
            mockApiClient.Verify(client => client.GetPostsAsync("someSubreddit"), Times.AtLeastOnce);
            mockApiClient.Verify(client => client.GetRateLimitRemaining(), Times.AtLeastOnce);
            mockApiClient.Verify(client => client.GetRateLimitReset(), Times.AtLeastOnce);        }


        public JObject CreateFakePostData()
        {
            var fakePostData = new JObject(
                new JProperty("data", new JObject(
                    new JProperty("children", new JArray(
                        new JObject(
                            new JProperty("data", new JObject(
                                new JProperty("id", "fakeId123"),
                                new JProperty("author", "fakeAuthor"),
                                new JProperty("title", "fakeTitle"),
                                new JProperty("ups", 123),
                                new JProperty("created_utc", DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                            ))
                        ),
                        new JObject(
                            new JProperty("data", new JObject(
                                new JProperty("id", "fakeId456"),
                                new JProperty("author", "anotherFakeAuthor"),
                                new JProperty("title", "anotherFakeTitle"),
                                new JProperty("ups", 456),
                                new JProperty("created_utc", DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                            ))
                        )
                    ))
                ))
            );

            return fakePostData;
        }
    }
}