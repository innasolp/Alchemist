using Alchemist.Product.BeautyAndHealth;
using Alchemist.Product.Import.Backgorund.Service.IntegrationTest.Infrastructure;
using Alchemist.Test.Server.Fixtures;
using Alchemist.Test.SettingsAPIFactory;
using Alchemist.Test.SignalRWebAppFactory;
using Microsoft.VisualStudio.Threading;
using System.Net;
using System.Net.Http.Json;
using Test.DbContainer.Abstractions;
using Test.PostresqlTestContainer;
using Test.RedisTestContainer;
using Xunit.Abstractions;
namespace Alchemist.Product.Import.Backgorund.Service.IntegrationTest;

[Collection(nameof(PostgresRedisDbCollection))]
public class ImportBackgroundServiceTest(ITestOutputHelper outputHelper,

    DbTestContainerFixture<PostgresqlTestDbContainer> postgresFixture,
    DbTestContainerFixture<RedisTestDbContainer> redisFixture) : LoggedContextTest(outputHelper)
{
    private record ShopSettingsWithServices(Infrastructure.ShopSettings ShopSettings, Infrastructure.ShopSettings[] Services);

    private async Task<ImportBackgroundServiceWebAppFactory> CreateWebAppFactoryAsync(int redisIndex, string childjobstorage, int[] ports)
    {
        if (ports.Length < 6)
            throw new Exception($"No 6 ports in range");
        var webAppFactory = new ImportBackgroundServiceWebAppFactory(postgresFixture.Container, redisFixture.Container, "serviceMessageTestDb",
             ports[0], ports[1], ports[2], ports[3], ports[4], ports[5], redisIndex, childjobstorage);

        await webAppFactory.InitializeAsync();

        webAppFactory.FixtureLoggingContext.LoggedMessage += Log;
        webAppFactory.ShopApiFixtureLoggingContext.LoggedMessage += Log;
        webAppFactory.SettingsApiFixtureLoggingContext.LoggedMessage += Log;

        return webAppFactory;
    }

    private async Task ClearWebAppFactoryAsync(ImportBackgroundServiceWebAppFactory webAppFactory)
    {
        webAppFactory.FixtureLoggingContext.LoggedMessage -= Log;
        webAppFactory.ShopApiFixtureLoggingContext.LoggedMessage += Log;
        webAppFactory.SettingsApiFixtureLoggingContext.LoggedMessage += Log;

        await (webAppFactory as IAsyncLifetime).DisposeAsync();
    }

    [Fact]
    public async Task HelloResponseWhenStartingSuccessAsync()
    {
        await using var webAppFactory = await CreateWebAppFactoryAsync(1, "start", [8058, 8059, 8208, 8209, 8310, 8311]);

        try
        {
            var httpClient = webAppFactory.CreateClient();
            var response = await httpClient.GetAsync("/");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var hello = await response.Content.ReadAsStringAsync();
            Assert.Equal("Hello ImportBackgroundService!", hello);
        }
        finally
        {
            await ClearWebAppFactoryAsync(webAppFactory);
        }
    }

    [Fact]
    public async Task WaitForImportMessageAsync()
    {
        await using var webAppFactory = await CreateWebAppFactoryAsync(2, "importmessagewait", [8060, 8061, 8210, 8211, 8312, 8313]);

        var importReceiver = webAppFactory.CreateImportItemReceiver();
        AsyncAutoResetEvent asyncAutoResetEvent = new();
        void onHandleProductMessage(object obj) => asyncAutoResetEvent.Set();
        void onHandleCategoryMessage(object obj) => asyncAutoResetEvent.Set();

        await importReceiver.Start();
        importReceiver.On("product", onHandleProductMessage, typeof(BeautyAndHealthProductData));
        importReceiver.On("category", onHandleCategoryMessage, typeof(CategoryData.CategoryData));        
        
        var httpClient = webAppFactory.CreateClient();

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

            await ClearWebAppFactoryAsync(webAppFactory);
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

        await using var webAppFactory = await CreateWebAppFactoryAsync(3, "importnewshopsettings", [8062, 8063, 8212, 8213, 8314, 8315]);

        await using var messageReceiver = SignalRHelper.CreateTestSignalRMessageHubAckReceiver(webAppFactory.Services, webAppFactory.SignalRTestServer, "events");
        await messageReceiver.Start();
        await messageReceiver.On<Data.ShopSettings>(Messages.Common.Messages.ShopSettingsCreated, onShopSettingsCreatedAsync);

        var httpClient = webAppFactory.CreateClient();

        var shopTokenSource = new CancellationTokenSource();
        shopTokenSource.CancelAfter(10000);
        
        var shop = await CreateNewShopAsync(webAppFactory, shopTokenSource.Token);

        var shopSettings = await CreateNewShopSettingsAsync(webAppFactory, shop.Id, shopSettingsName, shopTokenSource.Token);        

        var token = new CancellationToken();
        var task = asyncAutoResetEvent.WaitAsync(token);

        try
        {
            await task.WaitAsync(TimeSpan.FromMilliseconds(120000), token);

            Assert.Contains(LogMessages, m =>m.Message.Contains($"Handling of settings {shopSettings.Name} for shop id={shopSettings.ShopId} started"));

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
            await ClearWebAppFactoryAsync(webAppFactory);
        }
    }

    private async Task<Data.Shop?> CreateNewShopAsync(ImportBackgroundServiceWebAppFactory webAppFactory, CancellationToken cancellationToken = default)
    {
        var shop = new Data.Shop { Name = "Test", Url = $"https://{Guid.NewGuid()}" };
        var response = await webAppFactory.ShopApiClient.PutAsJsonAsync($"api/Shop", shop, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Data.Shop>(cancellationToken);
    }

    private static async Task<Infrastructure.ShopSettings?> CreateNewShopSettingsAsync(ImportBackgroundServiceWebAppFactory webAppFactory, 
        int shopId, string name, CancellationToken cancellationToken = default)
    {      

        var shopSettings = SettingsTestRepository.CreateProductShopSettings(shopId, name);
        var services = SettingsTestRepository.CreateShopSettingsServicesTestData(shopSettings);
        var settingsData = new  { ShopSettings = shopSettings, Services = services.ToArray() };
        var settingsPutResponse = await webAppFactory.ShopSettingsApiClient.PostAsJsonAsync("api/Settings/save", settingsData, cancellationToken);
        settingsPutResponse.EnsureSuccessStatusCode();

        var result = await settingsPutResponse.Content.ReadFromJsonAsync<ShopSettingsWithServices>(cancellationToken);
        return result?.ShopSettings;
    }
}