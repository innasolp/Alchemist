using Alchemist.BrowserService.Client;
using Alchemist.Import.Factory.BrowserService;
using Alchemist.Import.Settings.DataAdapter;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Import.Background;
using Alchemist.Test.Host.Interfaces;
using Alchemist.Test.Log;
using Alchemist.Test.RabbitMQ;
using Alchemist.Test.Server.Fixtures;
using Alchemist.Test.SignalRWebAppFactory;
using Message.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Alchemist.DataService.Interfaces;
using Alchemist.Product.RestAPIClient;
using Alchemist.Settings.RestAPIClient;
using Microsoft.Extensions.DependencyInjection;
using Alchemist.Test.ShopApiFactory;
using Alchemist.Test.SettingsAPIFactory;
using Alchemist.Product.ImportItemHandler;

namespace Alchemist.Product.Import.Backgorund.Service.IntegrationTest.Infrastructure;

public class ImportBackgroundServiceWebAppFactory : TestWebAppFactory<ImportBackgroundServiceProgram>, ILoggedContext
{
    private readonly SettingsAPIWebAppFactory _settingsAPIWebAppFactory;

    private readonly ShopAPIWebAppFactory _shopAPIWebAppFactory;

    private readonly SignalRLogContextWebAppFactory<FixtureLoggerFactoryContext> _signalRApplicationFactory;

    private readonly TestWebAppKestrelFactory<BrowserServiceProgramm> _browserServiceFactory;
    
    private readonly ITestHost _importItemsHost = new RabbitMQTestHost();

    private IConfiguration? _configuration;

    public FixtureLoggerFactoryContext FixtureLoggingContext { get; } = new FixtureLoggerFactoryContext();

    FixtureLogContext ILoggedContext.FixtureLoggingContext => FixtureLoggingContext;


    public FixtureLoggerFactoryContext SettingsApiFixtureLoggingContext => _settingsAPIWebAppFactory.FixtureLoggingContext;

    public FixtureLoggerFactoryContext ShopApiFixtureLoggingContext => _shopAPIWebAppFactory.FixtureLoggingContext;

    public TestServer SignalRTestServer => _signalRApplicationFactory.Server;

    public HttpClient ShopSettingsApiClient { get; }

    public HttpClient ShopApiClient { get; }   

    public ImportBackgroundServiceWebAppFactory(string connectionSection, int shopAPIHttpPort, int shopAPIHttpsPort,
        int settingsAPIHttpPort, int settingsAPIHttpsPort, 
        int browserServiceHttpPort, int browserServiceHttpsPort)
    {
        var settings = new ConfigurationBuilder()
              .AddJsonFile("appsettings.json")
              .Build();

        var alchemyDbConnectionString = settings.GetConnectionString(connectionSection);

        _signalRApplicationFactory = new SignalRLogContextWebAppFactory<FixtureLoggerFactoryContext>();
        _signalRApplicationFactory.CreateClient();

        _shopAPIWebAppFactory = new ShopAPIWebAppFactory(alchemyDbConnectionString, _signalRApplicationFactory.Server, shopAPIHttpPort, shopAPIHttpsPort);
        ShopApiClient = _shopAPIWebAppFactory.CreateClient();

        _settingsAPIWebAppFactory = new SettingsAPIWebAppFactory(alchemyDbConnectionString, _signalRApplicationFactory.Server, settingsAPIHttpPort, settingsAPIHttpsPort);
        ShopSettingsApiClient = _settingsAPIWebAppFactory.CreateClient();

        _browserServiceFactory = new TestWebAppKestrelFactory<BrowserServiceProgramm>(browserServiceHttpPort, browserServiceHttpsPort);
    }  
     
    public IMessageReceiver CreateImportItemReceiver()
    {
        return _importItemsHost.CreateSubscriber(Services,
            _configuration.GetSection("RabbitMqExchangeOptions:ExchangeName").Get<string>(),
            _configuration.GetSection("RabbitMqQueueOptions:Name").Get<string>());
    }

    protected override void ConfigureWebHostBuilderContext(WebHostBuilderContext context, IServiceCollection services)
    {
        FixtureLoggingContext.ConfigureServices(services);

        _configuration = context.Configuration;

        services.InterceptImplementation<IShopDataService, ShopApiClient>(new ShopApiClient(ShopApiClient));
        services.InterceptImplementation<IShopSettingsDataService, SettingsAPIClient>(new SettingsAPIClient(ShopSettingsApiClient));
        services.InterceptImplementation<ILoaderServiceFactory, BrowserServiceClientFactory>
            ((services) => services.AddBrowserServiceClientFactory(_browserServiceFactory.ServerAddress));

        services.RemoveImplementations<ISettingsAdapter>(typeof(SettingsDataAdapter<,>));

        var joinableTaskFactory = new Microsoft.VisualStudio.Threading.JoinableTaskFactory(new Microsoft.VisualStudio.Threading.JoinableTaskContext());
        joinableTaskFactory.Run(async () =>
        {
            await _importItemsHost.Start();
        });

        services.SetRabbitMqSender("importqueue",
            _importItemsHost.Uri,
            context.Configuration.GetSection("RabbitMqExchangeOptions:ExchangeName").Get<string>());

        services.SetSignalRHubTestSender(ServiceKeys.ImportProductMessageSenderKey, _signalRApplicationFactory.Server, "import");
        services.SetSignalRHubTestSender(ServiceKeys.ImportCategoryMessageSenderKey, _signalRApplicationFactory.Server, "import");
        services.SetSignalRHubTestReceiver(ShopImportWorkerKeys.EventMessageReceiverKey, _signalRApplicationFactory.Server, "events");
        services.SetSignalRHubTestSender(ShopImportWorkerKeys.EventMessageSenderKey, _signalRApplicationFactory.Server, "events");
    }
}
