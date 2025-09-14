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
        var importReceiver = WebAppFactory.CreateImportItemReceiver();
        AsyncAutoResetEvent asyncAutoResetEvent = new();
        void onHandleProductMessage(object obj) => asyncAutoResetEvent.Set();
        void onHandleCategoryMessage(object obj) => asyncAutoResetEvent.Set();

        await importReceiver.Start();
        importReceiver.On("product", onHandleProductMessage, typeof(Mock<IProductData>));
        importReceiver.On("category", onHandleCategoryMessage, typeof(Mock<ICategoryData>));

        var httpClient = WebAppFactory.CreateClient();

        var response = await httpClient.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var token = new CancellationToken();
        var task = asyncAutoResetEvent.WaitAsync(token);

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
        finally
        {
            await importReceiver.Stop();
        }
    }


    [Fact]
    public async Task NewShopSettingsHandlingWhenNewShopSettingsSavedAsync()
    {
        AsyncAutoResetEvent asyncAutoResetEvent = new();
        Action<ShopSettings> onShopSettingsCreated = (settings) => asyncAutoResetEvent.Set();

        var messageReceiver = WebAppFactory.Services.GetRequiredKeyedService<IMessageReceiver>(ShopImportWorkerKeys.EventMessageReceiverKey);
        messageReceiver.On<ShopSettings>(Messages.Common.Messages.ReceiveShopSettingsCreated, onShopSettingsCreated);

        var httpClient = WebAppFactory.CreateClient();

        var shop = await CreateNewShopAsync();

        var shopSettings = await CreateNewShopSettingsAsync(shop.Id);


        await Task.Delay(1000);

        var token = new CancellationToken();
        var task = asyncAutoResetEvent.WaitAsync(token);

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

    [Fact]

    public async Task ServiceCreatedCommandSendSuccessAsync()
    {
        var messageReceiver = SignalRHelper.CreateSignalRTestReceiver(WebAppFactory.Services, WebAppFactory.SignalRTestServer, "events");
        var serviceCreatedAutoResetEvent = new AsyncAutoResetEvent();
        var guids = new List<Guid>();
        var semaphoreSlim = new SemaphoreSlim(1, 1);

        Func<object[], Task> serviceCreatedAsync = async (parameters) =>
        {
            await OnServiceCreatedAsync(parameters, semaphoreSlim, guids);
            serviceCreatedAutoResetEvent.Set();
        };
        messageReceiver.On(Messages.Common.Messages.ReceiveServiceCreated, serviceCreatedAsync);
        await messageReceiver.Start();

        try
        {
            var httpClient = WebAppFactory.CreateClient();
            OutputHelper.WriteLine("Service started.");

            await httpClient.GetAsync("/");

            await Task.Delay(3000);

            if (guids.Count == 0)
            {
                var waitServiceCreationTask = serviceCreatedAutoResetEvent.WaitAsync();
                await waitServiceCreationTask.WaitAsync(TimeSpan.FromMilliseconds(3000));
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
    private async Task OnServiceCreatedAsync(object[] parameters, SemaphoreSlim semaphoreSlim, List<Guid> guids)
    {
        await semaphoreSlim.WaitAsync();
        if (parameters.Length > 0 && Guid.TryParse(parameters[0].ToString(), out var guid))
        {
            guids.Add(guid);
            OutputHelper.WriteLine(guid.ToString());
        }
        semaphoreSlim.Release();
    }


    [Fact]

    public async Task ServiceCancelledWhenStopCommandSendAsync()
    {        
        var serviceGuids = new List<Guid>();        
        var firstServiceCreatedAutoResetEvent = new AsyncAutoResetEvent(false);
        var semaphoreSlim = new SemaphoreSlim(1,1);
        Func<object[], Task> serviceCreatedAsync = async (parameters) =>
        {
            await OnServiceCreatedAsync(parameters, semaphoreSlim, serviceGuids);
            firstServiceCreatedAutoResetEvent.Set();
        };

        var testMessageReceiver = SignalRHelper.CreateSignalRTestReceiver(WebAppFactory.Services, WebAppFactory.SignalRTestServer, "events");
        testMessageReceiver.On(Messages.Common.Messages.ReceiveServiceCreated, serviceCreatedAsync);        
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
                var waitServiceCreationTask = firstServiceCreatedAutoResetEvent.WaitAsync();
                await waitServiceCreationTask.WaitAsync(TimeSpan.FromMilliseconds(30000));

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