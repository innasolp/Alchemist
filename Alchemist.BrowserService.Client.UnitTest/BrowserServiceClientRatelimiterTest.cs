using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebLoader.Interfaces;
using Xunit.Abstractions;

namespace Alchemist.BrowserService.Client.UnitTest;

public class BrowserServiceClientRatelimiterTest(ITestOutputHelper testOutputHelper)
{
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;

    [Theory]
    [InlineData(500,300)]
    [InlineData(100,500)]
    public async Task TaskWhenAllLoadFromUrl_SuccessAsync(int windowMilliseconds, int delay)
    {
        var webLoaderMock = new Mock<IWebLoader>();

        var ids = Enumerable.Range(0, 10).ToArray();

        var urls = ids.Select(i => $"http://example/{i}").ToArray();

        var expectedStreams = new Dictionary <string,Stream>();
        foreach (var id in ids)
        {
            var expectedBytes = Encoding.UTF8.GetBytes($"hello world {id}");
            expectedStreams.Add(urls[id], new MemoryStream(expectedBytes));            
        }

        webLoaderMock
            .Setup(w => w.LoadFromUrl(It.IsAny<string>(), It.IsAny<RequestOptions>()))
            .Returns(async (string url, RequestOptions? loadOptions = null)=>
            {
                _testOutputHelper.WriteLine($"Load from  {url} started at {DateTime.Now}");
                await Task.Delay(delay);
                _testOutputHelper.WriteLine($"Load from  {url} completed at {DateTime.Now}");
                return expectedStreams.TryGetValue(url, out var result) 
                    ? result 
                    : throw new InvalidOperationException(url);
            });

        var client = await Helper.CreateAsync(webLoaderMock, 
            rateLimiterOptions:new Import.Settings.RateLimiterOptions { WindowMilliseconds = windowMilliseconds });

        async Task<Stream> loadCall(int id)
        {
            await client.Start();
            return await client.Load(urls[id], Array.Empty<object>(),
                    cancellationToken: CancellationToken.None);
        }

        var tasks = ids.Select(loadCall).ToArray();

        await client.Start();

        var stopWatch = new Stopwatch();
        stopWatch.Start();
        var streams = await Task.WhenAll(tasks);
        stopWatch.Stop();

        Assert.Equal(ids.Length, streams.Length);
        webLoaderMock.Verify(w => w.Start(), Times.Once);
        webLoaderMock.Verify(w => w.LoadFromUrl(It.IsIn(urls), It.IsAny<RequestOptions>()), Times.Exactly(ids.Length));
        Assert.True(stopWatch.Elapsed.TotalMilliseconds >= windowMilliseconds * (ids.Length - 1));
    }
}
