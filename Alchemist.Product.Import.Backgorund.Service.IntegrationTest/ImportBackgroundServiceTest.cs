using Alchemist.Product.DataItem.Interfaces;
using Alchemist.Product.Import.Backgorund.Service.IntegrationTest.Infrastructure;
using Alchemist.Product.Import.Background;
using Alchemist.Product.ImportItem.Interfaces;
using Alchemist.Test.Server.Fixtures;
using Alchemist.Test.SignalRWebAppFactory;
using Message.Interfaces;
using Microsoft.Extensions.DependencyInjection;
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

public class ImportBackgroundServiceTest : LogContextTestFixture<ImportBackgroundServiceWebAppFactory, ImportBackgroundServiceProgram>
{ 
    private readonly AsyncAutoResetEvent _asyncAutoResetEvent = new();

    public ImportBackgroundServiceTest(ImportBackgroundServiceWebAppFactory webAppFactory, ITestOutputHelper outputHelper) : base(webAppFactory, outputHelper)
    {       
        WebAppFactory.ShopApiFixtureLoggingContext.LoggedMessage += Log;
        WebAppFactory.SettingsApiFixtureLoggingContext.LoggedMessage += Log;
    }

    [Fact]
    public async Task HelloResponseWhenStartingSuccessAsync()
    {
        var httpClient = WebAppFactory.CreateClient();
        var response = await httpClient.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var hello = await response.Content.ReadAsStringAsync();
        Assert.Equal("Hello ImportBackgroundService!", hello);
    }

    [Fact]
    public async Task WaitForImportMessageAsync()
    {
        var receiver = WebAppFactory.CreateImportItemReceiver();
        await receiver.Start();
        receiver.On("product", OnHandleProductMessage, typeof(Mock<IProductData>));
        receiver.On("category", OnHandleCategoryMessage, typeof(Mock<ICategoryData>));

        var httpClient = WebAppFactory.CreateClient();

        var response = await httpClient.GetAsync("/");
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
            OutputErrors();
            OutputWarnings();

            throw;            
        }
    }

    private void OnHandleCategoryMessage(object t)
    {
        _asyncAutoResetEvent.Set();
    }

    private void OnHandleProductMessage(object t)
    {
        _asyncAutoResetEvent.Set();
    }

    [Fact]
    public async Task NewShopSettingsHandlingWhenNewShopSettingsSavedAsync()
    {
        var messageReceiver = WebAppFactory.Services.GetRequiredKeyedService<IMessageReceiver>(ShopImportWorkerKeys.EventMessageReceiverKey);
        messageReceiver.On<ShopSettings>(Messages.Common.Messages.ReceiveShopSettingsCreated, OnShopSettingsCreatedAsync);

        var httpClient = WebAppFactory.CreateClient();

        var shop = await CreateNewShopAsync();

        var shopSettings = await CreateNewShopSettingsAsync(shop.Id);


        await Task.Delay(1000);

        var token = new CancellationToken();
        var task = _asyncAutoResetEvent.WaitAsync(token);

        try
        {
            await task.WaitAsync(TimeSpan.FromMilliseconds(30000), token);

            Assert.Contains(LogMessages, m =>m.Message == $"Handling of settings {shopSettings.Name} for shop id={shopSettings.ShopId} started.");

            OutputHelper.WriteLine("Event set");
        }
        catch
        {
            OutputErrors();
            OutputWarnings();

            throw;
        }
    }

    private async Task<Shop> CreateNewShopAsync()
    {
        var shop = new Shop { Name = "Test", Url = $"https://{Guid.NewGuid().ToString()}" };
        var response = await WebAppFactory.ShopApiClient.PutAsJsonAsync($"api/Shop", shop);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Shop>();
    }

    private async Task<ShopSettings> CreateNewShopSettingsAsync(int shopId)
    {
        var shopSettings = TestRepository.CreateProductShopSettings(shopId);
        var services = TestRepository.CreateShopSettingsServicesTestData(shopSettings);
        var settingsData = new ArrayList() { shopSettings, services.ToArray() };
        var settingsPutResponse = await WebAppFactory.ShopSettingsApiClient.PostAsJsonAsync("api/Settings/save", settingsData);
        settingsPutResponse.EnsureSuccessStatusCode();

        var result = await settingsPutResponse.Content.ReadFromJsonAsync<ArrayList>();
        var settings = JsonSerializer.Deserialize<ShopSettings>(result[0].ToString(),
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        return settings;
    }

    private async Task OnShopSettingsCreatedAsync(ShopSettings settings)
    {
        _asyncAutoResetEvent.Set();
        await Task.FromResult(true);
    }

    [Fact]

    public async Task ServiceCreatedCommandSendSuccessAsync()
    {
        var messageReceiver = SignalRHelper.CreateSignalRTestReceiver(WebAppFactory.Services, WebAppFactory.SignalRTestServer, "events");
        var serviceCreatedAutoResetEvent = new AsyncAutoResetEvent();
        var guids = new List<Guid>();
        var semaphoreSlim = new SemaphoreSlim(1, 1);
        Func<Guid, Task> onServiceCreated = async (guid) =>
        {
            await semaphoreSlim.WaitAsync();
            guids.Add(guid);
            OutputHelper.WriteLine(guid.ToString());
            semaphoreSlim.Release();

            serviceCreatedAutoResetEvent.Set();
        };
        messageReceiver.On(Messages.Common.Messages.ReceiveServiceCreated, onServiceCreated);
        await messageReceiver.Start();

        try
        {
            var httpClient = WebAppFactory.CreateClient();
            OutputHelper.WriteLine("Service started.");

            await httpClient.GetAsync("/");

            await Task.Delay(3000);

            if (guids.Count == 0)
            {
                await serviceCreatedAutoResetEvent.WaitAsync();
                Assert.NotEmpty(guids);
            }
        }
        catch
        {
            OutputErrors();
            OutputWarnings();

            throw;
        }
        finally
        {
            await messageReceiver.Stop();
        }
    }

    [Fact]

    public async Task ServiceCancelledWhenStopCommandSendAsync()
    {        
        var serviceGuids = new List<Guid>();
        
        var firstServiceCreatedAutoResetEvent = new AsyncAutoResetEvent(false);
        var semaphoreSlim = new SemaphoreSlim(1,1);
        async Task onServiceCreatedAsync(Guid guid)
        {
            await semaphoreSlim.WaitAsync();
            serviceGuids.Add(guid);
            OutputHelper.WriteLine($"Service {guid} created");
            semaphoreSlim.Release();           
            
            firstServiceCreatedAutoResetEvent.Set();
            
        }

        var testMessageReceiver = SignalRHelper.CreateSignalRTestReceiver(WebAppFactory.Services, WebAppFactory.SignalRTestServer, "events");
        testMessageReceiver.On(Messages.Common.Messages.ReceiveServiceCreated, (Func<Guid, Task>)onServiceCreatedAsync);        
        await testMessageReceiver.Start();

        var testMessageSender = SignalRHelper.CreateSignalRTestSender(WebAppFactory.Services, WebAppFactory.SignalRTestServer, "events");
        
         await testMessageSender.Start();
        
        try
        {
            var httpClient = WebAppFactory.CreateClient();
            OutputHelper.WriteLine("Service started.");            

            await httpClient.GetAsync("/");

            if (serviceGuids.Count == 0)
            {
                await firstServiceCreatedAutoResetEvent.WaitAsync();

                Assert.NotEmpty(serviceGuids);
            }

            var guid = serviceGuids.First();
            await testMessageSender.Send(guid, Messages.Common.Messages.SendServiceStop);            

            await Task.Delay(2000);

            Assert.Contains(LogMessages, l => l.LogLevel == Microsoft.Extensions.Logging.LogLevel.Information
            && l.Message?.Contains($"Stopping service with guid {guid} started.") == true);
            
        }
        catch
        {
            OutputErrors();
            OutputWarnings();

            throw;
        }
        finally
        {
            await testMessageReceiver.Stop();
            await testMessageSender.Stop();
        }
    }
}