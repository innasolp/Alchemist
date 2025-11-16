using Alchemist.Product.DataItem.Interfaces;
using Alchemist.Product.Import.Backgorund.Service.IntegrationTest.Infrastructure;
using Alchemist.Product.Import.Background;
using Alchemist.Product.ImportItem.Interfaces;
using Alchemist.Test.Server.Fixtures;
using Alchemist.Test.SettingsAPIFactory;
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

public class ImportBackgroundServiceTestFixtureWebAppFactory : ImportBackgroundServiceWebAppFactory
{
    public ImportBackgroundServiceTestFixtureWebAppFactory() : base("serviceTestDb", 8050, 8051, 8200, 8201, 8302, 8303)
    {
    }
}

public class ImportBackgroundServiceTest : LoggedContextTestFixture<ImportBackgroundServiceTestFixtureWebAppFactory, ImportBackgroundServiceProgram>
{    
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
        messageReceiver.On(Messages.Common.Messages.ShopSettingsCreated, onShopSettingsCreated);

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
        var shopSettings = SettingsTestRepository.CreateProductShopSettings(shopId);
        var services = SettingsTestRepository.CreateShopSettingsServicesTestData(shopSettings);
        var settingsData = new ArrayList() { shopSettings, services.ToArray() };
        var settingsPutResponse = await WebAppFactory.ShopSettingsApiClient.PostAsJsonAsync("api/Settings/save", settingsData);
        settingsPutResponse.EnsureSuccessStatusCode();

        var result = await settingsPutResponse.Content.ReadFromJsonAsync<ArrayList>();
        var settings = JsonSerializer.Deserialize<ShopSettings>(result[0].ToString(),
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        return settings;
    }
    
}