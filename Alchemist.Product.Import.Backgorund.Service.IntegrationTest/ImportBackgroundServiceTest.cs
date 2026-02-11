using Alchemist.Product.BeautyAndHealth;
using Alchemist.Product.CategoryData;
using Alchemist.Product.Import.Backgorund.Service.IntegrationTest.Infrastructure;
using Alchemist.Test.Server.Fixtures;
using Alchemist.Test.SettingsAPIFactory;
using Alchemist.Test.SignalRWebAppFactory;
using Microsoft.VisualStudio.Threading;
using Moq;
using System.Net;
using System.Net.Http.Json;
using Xunit.Abstractions;
namespace Alchemist.Product.Import.Backgorund.Service.IntegrationTest;

public class ImportBackgroundServiceTestFixtureWebAppFactory : ImportBackgroundServiceWebAppFactory
{
    public ImportBackgroundServiceTestFixtureWebAppFactory() : base("serviceTestDb", 8050, 8051, 8200, 8201, 8302, 8303)
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
        importReceiver.On("category", onHandleCategoryMessage, typeof(Alchemist.Product.CategoryData.CategoryData));        
        
        var httpClient = WebAppFactory.CreateClient();

        var token = new CancellationToken();
        var task = asyncAutoResetEvent.WaitAsync(token);

        try
        {
            await task.WaitAsync(TimeSpan.FromMilliseconds(45000), token);

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
        async Task onShopSettingsCreatedAsync(Data.ShopSettings settings)
        {
            if(settings.Name == shopSettingsName)
                asyncAutoResetEvent.Set();

            await Task.FromResult(true);
        }

        var messageReceiver = SignalRHelper.CreateTestSignalRMessageHubReceiver(WebAppFactory.Services, WebAppFactory.SignalRTestServer, "events");
        messageReceiver.On<Data.ShopSettings>(Messages.Common.Messages.ShopSettingsCreated, onShopSettingsCreatedAsync);
        await messageReceiver.Start();

        var httpClient = WebAppFactory.CreateClient();

        var shop = await CreateNewShopAsync();

        var shopSettings = await CreateNewShopSettingsAsync(shop.Id, shopSettingsName);        

        var token = new CancellationToken();
        var task = asyncAutoResetEvent.WaitAsync(token);

        try
        {
            await task.WaitAsync(TimeSpan.FromMilliseconds(30000), token);

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

    private async Task<Data.Shop> CreateNewShopAsync()
    {
        var shop = new Data.Shop { Name = "Test", Url = $"https://{Guid.NewGuid()}" };
        var response = await WebAppFactory.ShopApiClient.PutAsJsonAsync($"api/Shop", shop);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Data.Shop>();
    }

    private async Task<Infrastructure.ShopSettings> CreateNewShopSettingsAsync(int shopId, string name)
    {      

        var shopSettings = SettingsTestRepository.CreateProductShopSettings(shopId, name);
        var services = SettingsTestRepository.CreateShopSettingsServicesTestData(shopSettings);
        var settingsData = new  { ShopSettings = shopSettings, Services = services.ToArray() };
        var settingsPutResponse = await WebAppFactory.ShopSettingsApiClient.PostAsJsonAsync("api/Settings/save", settingsData);
        settingsPutResponse.EnsureSuccessStatusCode();

        var result = await settingsPutResponse.Content.ReadFromJsonAsync<ShopSettingsWithServices>();
        return result.ShopSettings;
    }    
}