using Alchemist.Product.Entities;
using Alchemist.Test.Server.Fixtures;
using Alchemist.Test.SignalRWebAppFactory;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit.Abstractions;

namespace Alchemist.Settings.RestAPI.Test;

public class SettingsAPITest(SettingsAPIWebAppFactory webAppFactory, ITestOutputHelper outputHelper) 
    : LoggedContextTestFixture<SettingsAPIWebAppFactory, SettingsAPIProgram>(webAppFactory, outputHelper)
{
    private Mock<ILogger> _loggerMock = new();

    private  const string ShopSettingsCreatedMessageFormat = "Shop settings created with id={0}";   

    [Fact]
    public async Task GetShopSettingsByNameSuccess()
    {
        var settingsName = "TestShop1_category";
        var httpClient = WebAppFactory.CreateClient();
        var response = await httpClient.GetAsync($"api/Settings/byName?name={settingsName}");
        response.EnsureSuccessStatusCode();

        var shopSettings = await response.Content.ReadFromJsonAsync<ShopSettings>();
        Assert.Equal(settingsName, shopSettings.Name);
    }

    [Fact]
    public async Task MessageSendWhenShopSettingCreatedSuccess()
    { 
        var httpClient = WebAppFactory.CreateClient();

        var receiver = SignalRHelper.CreateTestSignalRMessageHubReceiver(WebAppFactory.Services, WebAppFactory.SignalRTestServer, "events");
        await receiver.Start();
        receiver.On<ShopSettings>(Messages.Common.Messages.ShopSettingsCreated, OnShopSettingsCreatedAsync);       

        var productShopSettings = TestRepository.CreateProductShopSettings(WebAppFactory.Shops[1].Id, WebAppFactory.Shops[1].Name); 

        try
        {
            var response = await httpClient.PostAsJsonAsync($"api/Settings", productShopSettings);

            response.EnsureSuccessStatusCode();

            var savedShopSettings = await response.Content.ReadFromJsonAsync<ShopSettings>();

            Assert.NotEqual(0, savedShopSettings.Id);

            await Task.Delay(500);

            await receiver.Stop();

            VerifyInfoLog(string.Format(ShopSettingsCreatedMessageFormat, savedShopSettings.Id));
        }
        catch
        {
            OutputErrors();
            OutputWarnings();
            
            throw;
        }
    }

    private async Task OnShopSettingsCreatedAsync(ShopSettings settings)
    {
        _loggerMock.Object.LogInformation(string.Format(ShopSettingsCreatedMessageFormat, settings.Id));    
    }

    private void VerifyInfoLog(string message)
    {
        _loggerMock.Verify(l => l.Log(
               It.Is<LogLevel>(v => v == LogLevel.Information),
               It.IsAny<EventId>(),
               It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
               It.Is<Exception?>(v => v == null),
               It.IsAny<Func<It.IsAnyType, Exception?, string>>()));
    }

    [Fact]
    public async Task SaveShopSettingsWithServicesSuccess()
    {
        var httpClient = WebAppFactory.CreateClient();

        var productShopSettings = TestRepository.CreateProductShopSettings(WebAppFactory.Shops[1].Id, WebAppFactory.Shops[1].Name);

        var response = await httpClient.PostAsJsonAsync($"api/Settings", productShopSettings);
        response.EnsureSuccessStatusCode();
        var savedShopSettings = await response.Content.ReadFromJsonAsync<ShopSettings>();

        var services = TestRepository.CreateShopSettingsServicesTestData(productShopSettings.ShopId, savedShopSettings.Id);

        var data = new ArrayList() { savedShopSettings, services.ToArray() };
        var saveSettingsWithServicesResponse = await httpClient.PostAsJsonAsync($"api/Settings/save", data);
        saveSettingsWithServicesResponse.EnsureSuccessStatusCode();

        var result = await saveSettingsWithServicesResponse.Content.ReadFromJsonAsync<ArrayList>();
        Assert.Equal(2, result?.Count);

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        var resultShopSettings = JsonSerializer.Deserialize<ShopSettings>(result[0].ToString(), options);
        Assert.Equal(savedShopSettings.Id, resultShopSettings.Id);
        Assert.Equal(productShopSettings.Name, resultShopSettings.Name);

        var resultServices = JsonSerializer.Deserialize<ShopSettings[]>(result[1].ToString(), options);
        Assert.True(resultServices.All(s => s.Id != 0));
        Assert.Equal(services.Count, resultServices.Length);
        Assert.True(services.All(s => resultServices.Any(rs => rs.Name == s.Name && rs.ParentSettingsId == savedShopSettings.Id)));
    }

    [Fact]
    public async Task InterceptorLogInfoOnCallSuccess()
    {
        var httpClient = WebAppFactory.CreateClient();

        var response = await httpClient.GetAsync($"api/Settings/byId/1");

        Assert.Contains(LogMessages, m => m.LogLevel == LogLevel.Information && m.Message.Contains("api/Settings/byId/1"));
    }    
}