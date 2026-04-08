using Alchemist.BrowserService.Client;
using Alchemist.Import.Settings;
using Alchemist.Import.Settings.DataAdapter;
using Alchemist.Product.Import.Background;
using Alchemist.Settings.RestAPIClient;
using Alchemist.Test.Host.Interfaces;
using Alchemist.Test.Log;
using Alchemist.Test.RabbitMQ;
using Alchemist.Test.Server.Fixtures;
using Alchemist.Test.SettingsAPIFactory;
using Alchemist.Test.ShopApiFactory;
using Alchemist.Test.SignalRWebAppFactory;
using Import.Factory.Interfaces;
using Message.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shop.API.Client;
using Shop.Interfaces;
using ShopSettings.Interfaces;
using Test.PostresqlTestContainer;

namespace Alchemist.Product.Import.Backgorund.Service.IntegrationTest.Infrastructure;

public class ImportBackgroundServiceWebAppFactory : TestWebAppFactory<ImportBackgroundServiceProgram>, ILoggedContext, IAsyncLifetime
{
    private readonly string _dataBase;
    private readonly int _shopAPIHttpPort;
    private readonly int _shopAPIHttpsPort;
    private readonly int _settingsAPIHttpPort;
    private readonly int _settingsAPIHttpsPort;

    private readonly SettingsApiConfigurationWebAppFactory<PostgresqlTestDbContainer, PostgresDbRespawner> _settingsAPIWebAppFactory;

    private readonly ShopApiConfigurationWebAppFactory<PostgresqlTestDbContainer, PostgresDbRespawner> _shopAPIWebAppFactory;

    private readonly SignalRLogContextWebAppFactory<FixtureLoggerFactoryContext> _signalRApplicationFactory;

    private readonly TestWebAppKestrelFactory<BrowserServiceProgramm> _browserServiceFactory;
    
    private readonly IMessageTestHost _importItemsHost = new RabbitMQTestHost();

    private IConfiguration? _configuration;

    public FixtureLoggerFactoryContext FixtureLoggingContext { get; } = new FixtureLoggerFactoryContext();

    FixtureLogContext ILoggedContext.FixtureLoggingContext => FixtureLoggingContext;

    public FixtureLoggerFactoryContext? SettingsApiFixtureLoggingContext => _settingsAPIWebAppFactory?.FixtureLoggingContext;

    public FixtureLoggerFactoryContext? ShopApiFixtureLoggingContext => _shopAPIWebAppFactory?.FixtureLoggingContext;

    public TestServer SignalRTestServer => _signalRApplicationFactory.Server;

    public HttpClient? ShopSettingsApiClient { get; private set; }

    public HttpClient? ShopApiClient { get; private set; }

    public ImportBackgroundServiceWebAppFactory(string dataBaseSection,
        int shopAPIHttpPort, int shopAPIHttpsPort,
        int settingsAPIHttpPort, int settingsAPIHttpsPort, 
        int browserServiceHttpPort, int browserServiceHttpsPort)
    {
        _shopAPIHttpPort = shopAPIHttpPort;
        _shopAPIHttpsPort = shopAPIHttpsPort;
        _settingsAPIHttpPort = settingsAPIHttpPort;
        _settingsAPIHttpsPort = settingsAPIHttpsPort;
        var settings = new ConfigurationBuilder()
              .AddJsonFile("appsettings.json")
              .Build();

        _dataBase = settings.GetSection(dataBaseSection).Get<string>() ?? "test_ci_db";

        _signalRApplicationFactory = new SignalRLogContextWebAppFactory<FixtureLoggerFactoryContext>();
        _signalRApplicationFactory.CreateClient();        

        _browserServiceFactory = new TestWebAppKestrelFactory<BrowserServiceProgramm>(browserServiceHttpPort, browserServiceHttpsPort);

        _shopAPIWebAppFactory = new ShopApiConfigurationWebAppFactory<PostgresqlTestDbContainer,PostgresDbRespawner>
            ("ConnectionStrings:DbContext2", _dataBase, 5432, "postgres", "P@ssw0rd", _shopAPIHttpPort, _shopAPIHttpsPort, _signalRApplicationFactory.Server);

        _settingsAPIWebAppFactory = new SettingsApiConfigurationWebAppFactory<PostgresqlTestDbContainer, PostgresDbRespawner>
            ("ConnectionStrings:DbContext2", _dataBase, 5432, "postgres", "P@ssw0rd", _settingsAPIHttpPort, _settingsAPIHttpsPort, _signalRApplicationFactory.Server);
    }

    public IMessageReceiver CreateImportItemReceiver()
    {
        return _importItemsHost.CreateSubscriber(Services,
            _configuration?.GetSection("RabbitMqExchangeOptions:ExchangeName")?.Get<string>(),
            _configuration?.GetSection("RabbitMqQueueOptions:Name")?.Get<string>());
    }

    protected override void ConfigureWebHostBuilderContext(WebHostBuilderContext context, IServiceCollection services)
    {
        FixtureLoggingContext.ConfigureServices(services);

        _configuration = context.Configuration;

        ShopApiClient = _shopAPIWebAppFactory.CreateClient();
        services.InterceptImplementation<IShopDataService, ShopApiClient>(new ShopApiClient(ShopApiClient));

        ShopSettingsApiClient = _settingsAPIWebAppFactory.CreateClient();
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

        services.SetSignalRHubTestSender(BeautyAndHealth.ImportItemHandler.ServiceKeys.ImportProductMessageSenderKey, _signalRApplicationFactory.Server, "import");
        services.SetSignalRHubTestSender(Category.ImportItemHandler.ServiceKeys.ImportCategoryMessageSenderKey, _signalRApplicationFactory.Server, "import");
        services.SetSignalRHubTestReceiver(ShopImportWorkerKeys.EventMessageReceiverKey, _signalRApplicationFactory.Server, "events");
        services.SetSignalRHubTestSender(ShopImportWorkerKeys.EventMessageSenderKey, _signalRApplicationFactory.Server, "events");
    }

    public async Task InitializeAsync()
    {
        await _shopAPIWebAppFactory.InitializeAsync();
        
        await _settingsAPIWebAppFactory.InitializeAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await (_settingsAPIWebAppFactory as IAsyncLifetime).DisposeAsync();

        await (_shopAPIWebAppFactory as IAsyncLifetime).DisposeAsync();
    }
}