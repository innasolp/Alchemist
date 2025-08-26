using Alchemist.Common;
using Alchemist.DataService.Interfaces;
using Alchemist.Product.Entities;
using Alchemist.Test.Server.Fixtures;
using Alchemist.Test.SignalRWebAppFactory;
using Message.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;
using Moq;
using System.Collections;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit.Abstractions;

namespace Alchemist.Settings.RestAPI.Test;

public class SettingsAPITest(SettingsAPIWebAppFactory webAppFactory, ITestOutputHelper outputHelper) : TestFixture<SettingsAPIWebAppFactory, SettingsAPIProgram>(webAppFactory, outputHelper)
{
    private Mock<ILogger> _loggerMock = new();

    private  const string ShopSettingsCreatedMessageFormat = "Shop settings created with id={0}";

    record TestLogMessage(LogLevel LogLevel, string CategoryName, EventId EventId, string Message, Exception? Exception);

    private readonly List<TestLogMessage> _messages = [];

    private void Log(LogLevel logLevel, string categoryName, EventId eventId, string message, Exception? exception)
    {
        _messages.Add(new TestLogMessage(logLevel, categoryName, eventId, message, exception));
    }

    [Fact]
    public async Task GetAllParentsSettingsSuccess()
    {
        var httpClient = WebAppFactory.CreateClient();
        var response = await httpClient.GetAsync("api/Settings/allParents");
        response.EnsureSuccessStatusCode();

        var shopSettings = await response.Content.ReadFromJsonAsync<ShopSettings[]>();
        Assert.Equal(1, shopSettings.Length);
    }

    [Fact]
    public async Task MessageSendWhenShopSettingCreatedSuccess()
    { 
        WebAppFactory.ConfigureContextServices += WebAppFactoryAddServices;
        WebAppFactory.FixtureLoggingContext.LoggedMessage += Log;

        var httpClient = WebAppFactory.CreateClient();

        var receiver = WebAppFactory.Services.GetRequiredKeyedService<IMessageReceiver>("testReceiver");
        await receiver.Start();
        receiver.On<ShopSettings>(Messages.ReceiveShopSettingsCreated, OnShopSettingsCreatedAsync);       

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
            foreach (var errorMessage in _messages.Where(m => m.LogLevel == LogLevel.Error))            
                OutputHelper.WriteLine($"{errorMessage.Message} : {errorMessage.Exception?.Message ?? ""}");
            
            throw;
        }
        finally
        {
            WebAppFactory.ConfigureContextServices -= WebAppFactoryAddServices;
            WebAppFactory.FixtureLoggingContext.LoggedMessage -= Log;
        }
    }

    private async Task OnShopSettingsCreatedAsync(ShopSettings settings)
    {
        _loggerMock.Object.LogInformation(string.Format(ShopSettingsCreatedMessageFormat, settings.Id));    
    }

    private void WebAppFactoryAddServices(WebHostBuilderContext context, IServiceCollection services)
    {
        services.SetSignalRTestReceiver("testReceiver", WebAppFactory.SignalRTestServer, "events");
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
    public async Task InterceptorLogErrorWhenInternalServerError()
    {
        var repositoryMock = new Mock<ISettingsRepository>();
        var errorMessage = Guid.NewGuid().ToString();
        repositoryMock.Setup(r => r.GetShopSettings(It.IsAny<int>())).Throws(new Exception(errorMessage));

        void mockSettingsRepository(WebHostBuilderContext context, IServiceCollection services)
        {
            var sd = services.FirstOrDefault(s => s.ServiceType == typeof(ISettingsRepository));
            if (sd != null) services.Remove(sd);
            services.AddSingleton(repositoryMock.Object);
        }

        WebAppFactory.ConfigureContextServices += mockSettingsRepository;
        WebAppFactory.FixtureLoggingContext.LoggedMessage += Log;

        var httpClient = WebAppFactory.CreateClient();

        var response = await httpClient.GetAsync($"api/Settings/byId/1");
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        Assert.Contains(_messages, m => m.Message.Contains(errorMessage));

        WebAppFactory.ConfigureContextServices -= mockSettingsRepository;
        WebAppFactory.FixtureLoggingContext.LoggedMessage -= Log;
    }    
}