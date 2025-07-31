using Alchemist.Common;
using Alchemist.Product.SignalR;
using Alchemist.Test.Server.Fixtures;
using Message.Interfaces;
using Message.RabbitMQ;
using Message.RabbitMQ.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Alchemist.Product.Import.Backgorund.Service.IntegrationTest;

public class ImportBackgroundServiceWebAppFactory : TestWebAppKestrelFactory<ImportBackgroundServiceProgram>
{
    private readonly SettingsAPIWebAppFactory _settingsAPIWebAppFactory;

    private readonly ShopAPIWebAppFactory _shopAPIWebAppFactory;

    private readonly WebApplicationFactory<Startup> _signalRApplicationFactory;


    private readonly ConcurrentDictionary<string, object> _disposedServices = new();
    
    private RabbitMQOptions _messageReceiverOptions;

    private IMessageReceiver _testMessageReceiver;

    public ImportBackgroundServiceWebAppFactory() : base(8132,8133)
    {
        var settings = new ConfigurationBuilder()
              .AddJsonFile("appsettings.json")
              .Build();

        var alchemyDbConnectionString = settings.GetConnectionString("alchemydb");

        _signalRApplicationFactory = new WebApplicationFactory<Startup>();
        _signalRApplicationFactory.CreateClient();

        _shopAPIWebAppFactory = new ShopAPIWebAppFactory(alchemyDbConnectionString, _signalRApplicationFactory.Server);
        _shopAPIWebAppFactory.CreateClient();

        _settingsAPIWebAppFactory = new SettingsAPIWebAppFactory(alchemyDbConnectionString);
        _settingsAPIWebAppFactory.CreateClient();
    }

    public async Task<bool> SubscribeToEventAsync(string eventName, Func<object, Task> handler)
    {
        if (_testMessageReceiver == null) return await Task.FromResult(false);

        if(!_testMessageReceiver.IsConnected)        
            await _testMessageReceiver.Start();        

        _testMessageReceiver.On(eventName, handler);

        return await Task.FromResult(true);
    }

    private bool TryGetNotDisposedService<T, TDisposable>(string key, out TDisposable disposable)
        where T : class
        where TDisposable : class
    {
        disposable = default;       

        if (!_disposedServices.TryGetValue(key, out var value) && 
            Services.GetRequiredKeyedService<T>(key) is TDisposable disposableService)
        {
            disposable = disposableService;
            return true;
        }

        return false;
    }

    private bool TryAddDisposableService(string key, object disposable)
    {
        return _disposedServices.TryAdd(key, disposable);   
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing
            && TryGetNotDisposedService<IMessageSender, IDisposable>("importqueue", out var disposable))
        {
            disposable.Dispose();
            TryAddDisposableService("importqueue", disposable);
        }
        
        base.Dispose(disposing);
    }

    public override async ValueTask DisposeAsync()
    {
        if (TryGetNotDisposedService<IMessageSender, IAsyncDisposable>("importqueue", out var asyncDisposable))
        {
            await asyncDisposable.DisposeAsync();
            TryAddDisposableService("importqueue", asyncDisposable);
        }

        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureServices((context, services) =>
        {
            _messageReceiverOptions = CreateReceiverOptions(context.Configuration);
        });
    }

    protected override void ConfigureClient(HttpClient client)
    {
        _testMessageReceiver = CreateReceiver(Services, _messageReceiverOptions);

        base.ConfigureClient(client);
    }

    private static RabbitMQOptions CreateReceiverOptions(IConfiguration configuration)
    {
        var options = configuration.GetRabbitMQOptions("RabbitMqServiceOptions", "RabbitMqQueueOptions", "RabbitMqExchangeOptions");
        options.RabbitMqServiceOptions.HostName = options.RabbitMqServiceOptions.HostName.SetEnvironmentLocalHostIfNeed();
        return options;
    }
   
    private static IMessageReceiver CreateReceiver(IServiceProvider services, RabbitMQOptions messageReceiverOptions)
    {       
        return new RabbitMQMessageReceiver(services.GetRequiredService<ILogger<RabbitMQMessageReceiver>>(),
                messageReceiverOptions.RabbitMqServiceOptions.HostName,
                messageReceiverOptions.RabbitMqServiceOptions.Port,
                messageReceiverOptions.RabbitMQExchangeSettings.ExchangeName,
                messageReceiverOptions.RabbitMqQueueOptions.Name);
    }
}
