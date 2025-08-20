using Alchemist.Product.DataItem.Interfaces;
using Alchemist.Product.ImportItem.Interfaces;
using Alchemist.Test.Server.Fixtures;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;
using Moq;
using System.Net;
using Xunit.Abstractions;

namespace Alchemist.Product.Import.Backgorund.Service.IntegrationTest;

public class ImportBackgroundServiceTest : TestFixture<ImportBackgroundServiceWebAppFactory, ImportBackgroundServiceProgram>
{
    record TestLogMessage(LogLevel LogLevel, string CategoryName, EventId EventId, string Message, Exception? Exception);

    private readonly HttpClient _httpClient;

    private readonly AsyncAutoResetEvent _asyncAutoResetEvent = new();
    
    private readonly List<TestLogMessage> _messages = [];

    public ImportBackgroundServiceTest(ImportBackgroundServiceWebAppFactory webAppFactory, ITestOutputHelper outputHelper) : base(webAppFactory, outputHelper)
    {
        WebAppFactory.FixtureLoggingContext.LoggedMessage += Log;

        _httpClient = WebAppFactory.CreateClient();
    }

    private void Log(LogLevel logLevel, string categoryName, EventId eventId, string message, Exception? exception)
    {
        _messages.Add(new TestLogMessage(logLevel, categoryName, eventId, message, exception));
    }

    [Fact]
    public async Task HelloResponseWhenStartingSuccess()
    {
        var response = await _httpClient.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var hello = await response.Content.ReadAsStringAsync();
        Assert.Equal("Hello ImportBackgroundService!", hello);
    }

    [Fact]
    public async Task WaitForImportMessage()
    {
        var receiver = WebAppFactory.CreateImportItemReceiver();
        await receiver.Start();
        receiver.On("product", OnHandleProductMessageAsync, typeof(Mock<IProductData>));
        receiver.On("category", OnHandleCategoryMessageAsync, typeof(Mock<ICategoryData>));

        var response = await _httpClient.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var token = new CancellationToken();
        var task = _asyncAutoResetEvent.WaitAsync(token);

        try
        {
            await task.WaitAsync(TimeSpan.FromMilliseconds(30000), token);

            OutputHelper.WriteLine("Event set");
        }
        catch
        {
            foreach (var errorMessage in _messages.Where(m => m.LogLevel == LogLevel.Error))
            {
                OutputHelper.WriteLine($"{errorMessage.Message} : {errorMessage.Exception?.Message ?? ""}");                
            }

            throw;            
        }
    }

    private async Task OnHandleCategoryMessageAsync(object t)
    {
        _asyncAutoResetEvent.Set();
    }

    private async Task OnHandleProductMessageAsync(object t)
    {
        _asyncAutoResetEvent.Set();
    }
}