using Alchemist.Product.BeautyAndHealth;
using Alchemist.Product.Import.Backgorund.Service.IntegrationTest.Infrastructure;
using Alchemist.Test.Server.Fixtures;
using Alchemist.Test.SettingsAPIFactory;
using Alchemist.Test.SignalRWebAppFactory;
using Microsoft.VisualStudio.Threading;
using System.Net;
using System.Net.Http.Json;
using Xunit.Abstractions;
namespace Alchemist.Product.Import.Backgorund.Service.IntegrationTest;

public class ImportBackgroundServiceTestFixtureWebAppFactory : ImportBackgroundServiceWebAppFactory
{
    public ImportBackgroundServiceTestFixtureWebAppFactory() : base(8138, 8139, "serviceTestDb", 8050, 8051, 8200, 8201, 8302, 8303)
    {
    }
}

public class ImportBackgroundServiceTest : LoggedContextTestFixture<ImportBackgroundServiceTestFixtureWebAppFactory, ImportBackgroundServiceProgram>
{   
    private record ShopSettingsWithServices(Infrastructure.ShopSettings ShopSettings, Infrastructure.ShopSettings[] Services);
    
    public ImportBackgroundServiceTest(ImportBackgroundServiceTestFixtureWebAppFactory webAppFactory, ITestOutputHelper outputHelper) 
        : base(webAppFactory, outputHelper)
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
        importReceiver.On("product", onHandleProductMessage, typeof(BeautyAndHealthProductData));
        importReceiver.On("category", onHandleCategoryMessage, typeof(CategoryData.CategoryData));        
        
        var httpClient = WebAppFactory.CreateClient();

        var tokenSource = new CancellationTokenSource();
        tokenSource.CancelAfter(120000);
        
        try
        {
            await asyncAutoResetEvent.WaitAsync(tokenSource.Token);

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
        var shopSettingsName = Guid.NewGuid().ToString();

        AsyncAutoResetEvent asyncAutoResetEvent = new();
        async Task onShopSettingsCreatedAsync(string msgId, Data.ShopSettings settings)
        {
            if(settings.Name == shopSettingsName)
                asyncAutoResetEvent.Set();

            await Task.FromResult(true);
        }

        await using var messageReceiver = SignalRHelper.CreateTestSignalRMessageHubAckReceiver(WebAppFactory.Services, WebAppFactory.SignalRTestServer, "events");
        await messageReceiver.Start();
        await messageReceiver.On<Data.ShopSettings>(Messages.Common.Messages.ShopSettingsCreated, onShopSettingsCreatedAsync);
        
        var httpClient = WebAppFactory.CreateClient();

        var shopTokenSource = new CancellationTokenSource();
        shopTokenSource.CancelAfter(10000);
        
        var shop = await CreateNewShopAsync(shopTokenSource.Token);

        var shopSettings = await CreateNewShopSettingsAsync(shop.Id, shopSettingsName, shopTokenSource.Token);        

        var token = new CancellationToken();
        var task = asyncAutoResetEvent.WaitAsync(token);

        try
        {
            await task.WaitAsync(TimeSpan.FromMilliseconds(60000), token);

            await Task.Delay(1000);

            Assert.Contains(LogMessages, m =>m.Message.Contains($"Handling of settings {shopSettings.Name} for shop id={shopSettings.ShopId} started"));

            OutputHelper.WriteLine("Event set");
        }
        catch
        {
            OutputErrors();
            OutputWarnings();

            throw;
        }
    }

    private async Task<Data.Shop?> CreateNewShopAsync(CancellationToken cancellationToken = default)
    {
        var shop = new Data.Shop { Name = "Test", Url = $"https://{Guid.NewGuid()}" };
        var response = await WebAppFactory.ShopApiClient.PutAsJsonAsync($"api/Shop", shop, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Data.Shop>(cancellationToken);
    }

    private async Task<Infrastructure.ShopSettings?> CreateNewShopSettingsAsync(int shopId, string name, CancellationToken cancellationToken = default)
    {      

        var shopSettings = SettingsTestRepository.CreateProductShopSettings(shopId, name);
        var services = SettingsTestRepository.CreateShopSettingsServicesTestData(shopSettings);
        var settingsData = new  { ShopSettings = shopSettings, Services = services.ToArray() };
        var settingsPutResponse = await WebAppFactory.ShopSettingsApiClient.PostAsJsonAsync("api/Settings/save", settingsData, cancellationToken);
        settingsPutResponse.EnsureSuccessStatusCode();

        var result = await settingsPutResponse.Content.ReadFromJsonAsync<ShopSettingsWithServices>(cancellationToken);
        return result?.ShopSettings;
    }    
}