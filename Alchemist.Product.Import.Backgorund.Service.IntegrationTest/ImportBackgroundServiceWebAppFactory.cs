using Alchemist.BrowserService.Client;
using Alchemist.Import.BrowserService.Factory;
using Alchemist.Import.Settings.DataAdapter;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Import.Background;
using Alchemist.Product.SignalR;
using Alchemist.Test.Host.Interfaces;
using Alchemist.Test.RabbitMQ;
using Alchemist.Test.Server.Fixtures;
using Alchemist.Test.SignalRWebAppFactory;
using Message.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.Product.Import.Backgorund.Service.IntegrationTest;

public class ImportBackgroundServiceWebAppFactory : WebApplicationFactory<ImportBackgroundServiceProgram>
{
    private readonly SettingsAPIWebAppFactory _settingsAPIWebAppFactory;

    private readonly ShopAPIWebAppFactory _shopAPIWebAppFactory;

    private readonly WebApplicationFactory<Startup> _signalRApplicationFactory;

    private readonly TestWebAppKestrelFactory<BrowserServiceProgramm> _browserServiceFactory;
    
    private readonly ITestHost _importItemsHost = new RabbitMQTestHost();

    private IConfiguration? _configuration;

    public FixtureLoggerFactoryContext FixtureLoggingContext { get; } = new FixtureLoggerFactoryContext();

    public ImportBackgroundServiceWebAppFactory()
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

        _browserServiceFactory = new TestWebAppKestrelFactory<BrowserServiceProgramm>(8302, 8303);
    }
        
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureTestServices(FixtureLoggingContext.ConfigureServices);

        builder.ConfigureServices((context, services) =>
        {
            _configuration = context.Configuration;

            SetBrowserServiceClient(services, _browserServiceFactory.ServerAddress);

            RemoveDBJsonAdapter(services);

            var joinableTaskFactory = new Microsoft.VisualStudio.Threading.JoinableTaskFactory(new Microsoft.VisualStudio.Threading.JoinableTaskContext());
            joinableTaskFactory.Run(async () =>
            {
                await _importItemsHost.Start();
            });
            
            services.SetRabbitMqSender("importqueue", 
                _importItemsHost.Uri,
                context.Configuration.GetSection("RabbitMqExchangeOptions:ExchangeName").Get<string>());

            services.SetSignalRTestSender(ShopImportWorkerKeys.ShopsMessageSenderKey, _signalRApplicationFactory.Server, "import");
            services.SetSignalRTestReceiver(ShopImportWorkerKeys.DataMessageReceiverKey, _signalRApplicationFactory.Server, "events");
        });
    }

    private static void RemoveDBJsonAdapter(IServiceCollection services)
    {
        var descriptors = services.Where(s => s.ServiceType == typeof(ISettingsAdapter) && s.ImplementationType == typeof(SettingsDataAdapter<,>));
        descriptors.ToList().ForEach(sd => services.Remove(sd));
    }

    private static void SetBrowserServiceClient(IServiceCollection services, string url)
    {
        var descriptors = services.Where(sd => sd.ServiceType == typeof(IBrowserServiceFactory)
           && sd.ImplementationType == typeof(BrowserServiceClientFactory));
        descriptors.ToList().ForEach(sd=>services.Remove(sd));

        services.AddBrowserServiceClientFactory(url);
    }
     
    public IMessageReceiver CreateImportItemReceiver()
    {
        return _importItemsHost.CreateSubscriber(Services,
            _configuration.GetSection("RabbitMqExchangeOptions:ExchangeName").Get<string>(),
            _configuration.GetSection("RabbitMqQueueOptions:Name").Get<string>());
    }
}
