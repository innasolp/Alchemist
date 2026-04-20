using Alchemist.Test.Server.Fixtures;
using Alchemist.Test.SignalRWebAppFactory;
using Message.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;
using Moq;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit.Abstractions;

namespace Alchemist.Settings.RestAPI.Test;

public class SettingsAPITest(SettingsApiConfigurationPostgresWebAppFactory webAppFactory, ITestOutputHelper outputHelper) 
    : LoggedContextTestFixture<SettingsApiConfigurationPostgresWebAppFactory, SettingsAPIProgram>(webAppFactory, outputHelper)
{
    private record ShopSettingsWithServices(Product.Data.ShopSettings ShopSettings, Product.Data.ShopSettings[] Services);

    private readonly Mock<ILogger> _loggerMock = new();

    private  const string ShopSettingsCreatedMessageFormat = "Shop settings created with id={0}";   

    [Fact]
    public async Task GetShopSettingsByNameSuccess()
    {
        var settingsName = "TestShop1_category";

        try
        {
            var httpClient = WebAppFactory.CreateClient();
            var response = await httpClient.GetAsync($"api/Settings/byName?name={settingsName}");
            response.EnsureSuccessStatusCode();

            var shopSettings = await response.Content.ReadFromJsonAsync<Product.Data.ShopSettings>();
            Assert.Equal(settingsName, shopSettings.Name);
        }
        catch
        {
            OutputErrors();
            OutputWarnings();

            throw;
        }
        finally
        {
            await WebAppFactory.ResetDatabaseIfAvailableAsync();
        }
    }

    [Fact]
    public async Task MessageSendWhenShopSettingCreatedSuccess()
    { 
        var httpClient = WebAppFactory.CreateClient();

        string messageId = "";
        async Task OnShopSettingsCreatedAsync(string msgId, Product.Data.ShopSettings settings)
        {
            messageId = msgId;
            _loggerMock.Object.LogInformation(string.Format(ShopSettingsCreatedMessageFormat, settings.Id));
        }
        await using var receiver = SignalRHelper.CreateTestSignalRMessageHubAckReceiver(WebAppFactory.Services, WebAppFactory.SignalRTestServer, "events");
        await receiver.Start();
        await receiver.On<Product.Data.ShopSettings>(Messages.Common.Messages.ShopSettingsCreated, OnShopSettingsCreatedAsync);       

        var productShopSettings = TestRepository.CreateProductShopSettings(WebAppFactory.Shops[1].Id, WebAppFactory.Shops[1].Name);

        var autoResetEvent = new AsyncAutoResetEvent();
        bool shopSettingsCreated = false;
        async Task shopSettingsCreatedAckAsync(object sender, AcknowlegeEventArgs e)
        {
            Assert.Equal(messageId, e.RequestId);
            shopSettingsCreated = true;
            autoResetEvent.Set();
        }
        await using var testMessageSender = WebAppFactory.Services.GetRequiredService<IAcknowlegefulMessageSender>();
        testMessageSender.AcknowlegeCallbackAsync += shopSettingsCreatedAckAsync;

        try
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var response = await httpClient.PostAsJsonAsync($"api/Settings", productShopSettings);

            response.EnsureSuccessStatusCode();

            cancellationTokenSource.CancelAfter(4000);

            if (!shopSettingsCreated)
                await autoResetEvent.WaitAsync(cancellationTokenSource.Token);

            var savedShopSettings = await response.Content.ReadFromJsonAsync<Product.Data.ShopSettings>();

            Assert.True(shopSettingsCreated);
            Assert.Equal(productShopSettings.Name, savedShopSettings?.Name);            

            await receiver.Stop();
        }
        catch
        {
            OutputErrors();
            OutputWarnings();
            
            throw;
        }
        finally
        {
            await WebAppFactory.ResetDatabaseIfAvailableAsync();
        }
    }

    [Fact]
    public async Task SaveShopSettingsWithServicesSuccess()
    {
        try
        {
            var httpClient = WebAppFactory.CreateClient();

            var productShopSettings = TestRepository.CreateProductShopSettings(WebAppFactory.Shops[1].Id, WebAppFactory.Shops[1].Name);

            var response = await httpClient.PostAsJsonAsync($"api/Settings", productShopSettings);
            response.EnsureSuccessStatusCode();
            var savedShopSettings = await response.Content.ReadFromJsonAsync<Product.Data.ShopSettings>();

            var services = TestRepository.CreateShopSettingsServicesTestData(productShopSettings.ShopId, savedShopSettings.Id);

            var data = new { ShopSettings = savedShopSettings, Services = services.ToArray() };
            var saveSettingsWithServicesResponse = await httpClient.PostAsJsonAsync($"api/Settings/save", data);
            saveSettingsWithServicesResponse.EnsureSuccessStatusCode();

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var result = await saveSettingsWithServicesResponse.Content.ReadFromJsonAsync<ShopSettingsWithServices>(options);
            Assert.NotNull(result);

            Assert.Equal(savedShopSettings.Id, result.ShopSettings.Id);
            Assert.Equal(productShopSettings.Name, result.ShopSettings.Name);

            Assert.True(result.Services.All(s => s.Id != 0));
            Assert.Equal(services.Count, result.Services.Length);
            Assert.True(services.All(s => result.Services.Any(rs => rs.Name == s.Name && rs.ParentSettingsId == savedShopSettings.Id)));
        }
        catch
        {
            OutputErrors();
            OutputWarnings();

            throw;
        }
        finally
        {
            await WebAppFactory.ResetDatabaseIfAvailableAsync();
        }
    }

    [Fact]
    public async Task InterceptorLogInfoOnCallSuccess()
    {
        try
        {
            var httpClient = WebAppFactory.CreateClient();

            var response = await httpClient.GetAsync($"api/Settings/byId/1");
            response.EnsureSuccessStatusCode();

            Assert.Contains(LogMessages, m => m.LogLevel == LogLevel.Information
            && m.CategoryName == "Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware"
            && m.Message.Contains("api/Settings/byId/1"));
        }
        catch
        {
            OutputErrors();
            OutputWarnings();

            throw;
        }
        finally
        {
            await WebAppFactory.ResetDatabaseIfAvailableAsync();
        }
    }    
}