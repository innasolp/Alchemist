using Alchemist.Common;
using Alchemist.Product.DataItem.Interfaces;
using Alchemist.Product.Import.Background;
using Alchemist.Product.ImportItem.Interfaces;
using Alchemist.Test.Server.Fixtures;
using Message.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;
using Moq;
using System.Collections;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit.Abstractions;
using Shop = Alchemist.Product.Entities.Shop;
using ShopSettings = Alchemist.Product.Entities.ShopSettings;

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
        WebAppFactory.ShopApiFixtureLoggingContext.LoggedMessage += Log;
        WebAppFactory.SettingsApiFixtureLoggingContext.LoggedMessage += Log;

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
            OutputHelper.WriteLine("Errors:");
            foreach (var errorMessage in _messages.Where(m => m.LogLevel == LogLevel.Error))
            {
                OutputHelper.WriteLine($"{errorMessage.Message} : {errorMessage.Exception?.Message ?? ""}");
                if(errorMessage.Exception != null)
                    OutputHelper.WriteLine(errorMessage.Exception.StackTrace);
            }

            OutputHelper.WriteLine("\r\n Warnings:");
            foreach (var errorMessage in _messages.Where(m => m.LogLevel == LogLevel.Warning))
            {
                OutputHelper.WriteLine($"{errorMessage.Message} : {errorMessage.Exception?.Message ?? ""}");
                if(errorMessage.Exception != null) 
                    OutputHelper.WriteLine(errorMessage.Exception.StackTrace);
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

    [Fact]
    public async Task NewShopSettingsHandlingWhenNewShopSettingsSaved()
    {
        var messageReceiver = WebAppFactory.Services.GetRequiredKeyedService<IMessageReceiver>(ShopImportWorkerKeys.DataMessageReceiverKey);
        messageReceiver.On<ShopSettings>(Messages.ReceiveShopSettingsCreated, OnShopSettingsCreatedAsync);

        var shop = await CreateNewShop();

        var shopSettings = await CreateNewShopSettings(shop.Id);


        await Task.Delay(1000);

        var token = new CancellationToken();
        var task = _asyncAutoResetEvent.WaitAsync(token);

        try
        {
            await task.WaitAsync(TimeSpan.FromMilliseconds(30000), token);

            Assert.True(_messages.Any(m=>m.Message == $"Handling of settings {shopSettings.Name} for shop id={shopSettings.ShopId} started."));

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

    private async Task<Shop> CreateNewShop()
    {
        var shop = new Shop { Name = "Test", Url = $"https://{Guid.NewGuid().ToString()}" };
        var response = await WebAppFactory.ShopApiClient.PutAsJsonAsync($"api/Shop", shop);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Shop>();
    }

    private async Task<ShopSettings> CreateNewShopSettings(int shopId)
    {
        var shopSettings = TestRepository.CreateProductShopSettings(shopId);
        var services = TestRepository.CreateShopSettingsServicesTestData(shopSettings);
        var settingsData = new ArrayList() { shopSettings, services.ToArray() };
        var settingsPutResponse = await WebAppFactory.ShopSettingsApiClient.PostAsJsonAsync("api/Settings/save", settingsData);
        settingsPutResponse.EnsureSuccessStatusCode();

        var result = await settingsPutResponse.Content.ReadFromJsonAsync<ArrayList>();
        return JsonSerializer.Deserialize<ShopSettings>(result[0].ToString(),
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    private async Task OnShopSettingsCreatedAsync(ShopSettings settings)
    {
        _asyncAutoResetEvent.Set();
        await Task.FromResult(true);
    }
}