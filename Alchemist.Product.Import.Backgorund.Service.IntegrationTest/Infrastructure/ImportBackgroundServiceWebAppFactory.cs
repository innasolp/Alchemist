using Alchemist.BrowserService.Client;
using Alchemist.Import.Settings;
using Alchemist.Import.Settings.DataAdapter;
using Alchemist.Product.Import.Background;
using Alchemist.Settings.RestAPIClient;
using Alchemist.Test.DBApiWebAppFactory.Configuration;
using Alchemist.Test.Host.Interfaces;
using Alchemist.Test.Log;
using Alchemist.Test.RabbitMQ;
using Alchemist.Test.Server.Fixtures;
using Alchemist.Test.SettingsAPIFactory;
using Alchemist.Test.ShopApiFactory;
using Alchemist.Test.SignalRWebAppFactory;
using Hangfire.AggregateJobs.ChildJobStorages;
using Import.Factory.Interfaces;
using Message.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shop.API.Client;
using Shop.Interfaces;
using ShopSettings.Interfaces;
using Test.PostresqlTestContainer;
using Test.RedisTestContainer;

namespace Alchemist.Product.Import.Backgorund.Service.IntegrationTest.Infrastructure;

public class ImportBackgroundServiceWebAppFactory : TestWebAppFactory<ImportBackgroundServiceProgram>, ILoggedContext, IAsyncLifetime, IWebHostConfigure
{
    private readonly string _dataBase;
    private readonly int _shopAPIHttpPort;
    private readonly int _shopAPIHttpsPort;
    private readonly int _settingsAPIHttpPort;
    private readonly int _settingsAPIHttpsPort;
    private readonly int _defaultRedisIndex;

    private readonly PostgresqlTestDbContainer _postgresTestDbContainer;

    private readonly RedisTestDbContainer _redisContainer;

    private readonly SettingsApiConfigurationWebAppFactory<PostgresqlTestDbContainer, PostgresDbRespawner, PostgresDbHelper> _settingsAPIWebAppFactory;

    private readonly ShopApiConfigurationWebAppFactory<PostgresqlTestDbContainer, PostgresDbRespawner, PostgresDbHelper> _shopAPIWebAppFactory;

    private readonly SignalRLogContextWebAppFactory<FixtureLoggerFactoryContext> _signalRApplicationFactory;

    private readonly TestWebAppKestrelFactory<BrowserServiceProgramm> _browserServiceFactory;
    
    private readonly IMessageTestHost _importItemsHost = new RabbitMQTestHost();

    private readonly DbConfigurationContainerWebAppInterceptor<AggregateJobDbContext, PostgresqlTestDbContainer, PostgresDbRespawner, PostgresDbHelper> _childJobDbInterceptor;

    private IConfiguration? _configuration;

    public FixtureLoggerFactoryContext FixtureLoggingContext { get; } = new FixtureLoggerFactoryContext();

    FixtureLogContext ILoggedContext.FixtureLoggingContext => FixtureLoggingContext;

    public FixtureLoggerFactoryContext? SettingsApiFixtureLoggingContext => _settingsAPIWebAppFactory?.FixtureLoggingContext;

    public FixtureLoggerFactoryContext? ShopApiFixtureLoggingContext => _shopAPIWebAppFactory?.FixtureLoggingContext;

    public TestServer SignalRTestServer => _signalRApplicationFactory.Server;

    public HttpClient? ShopSettingsApiClient { get; private set; }

    public HttpClient? ShopApiClient { get; private set; }


    private Action<IHost>? _configureHost;

    event Action<IHost> IWebHostConfigure.ConfigureHost
    {
        add
        {
            _configureHost += value;
        }
        remove
        {
            _configureHost -= value;
        }
    }

    public ImportBackgroundServiceWebAppFactory(
        PostgresqlTestDbContainer postgresTestDbContainer,
        RedisTestDbContainer redisTestDbContainer,
        string dataBaseSection,
        int shopAPIHttpPort, int shopAPIHttpsPort,
        int settingsAPIHttpPort, int settingsAPIHttpsPort, 
        int browserServiceHttpPort, int browserServiceHttpsPort,
        int defaultRedisIndex = 1,
        string childjobstorage = "childjobstorage") : base()
    {
        _postgresTestDbContainer = postgresTestDbContainer;
        _redisContainer = redisTestDbContainer;

        _shopAPIHttpPort = shopAPIHttpPort;
        _shopAPIHttpsPort = shopAPIHttpsPort;
        _settingsAPIHttpPort = settingsAPIHttpPort;
        _settingsAPIHttpsPort = settingsAPIHttpsPort;
        _defaultRedisIndex = defaultRedisIndex;

        var settings = new ConfigurationBuilder()
              .AddJsonFile("appsettings.json")
              .Build();

        _dataBase = settings.GetSection(dataBaseSection).Get<string>() ?? "test_ci_db";

        _signalRApplicationFactory = new SignalRLogContextWebAppFactory<FixtureLoggerFactoryContext>();
        _signalRApplicationFactory.CreateClient();        

        _browserServiceFactory = new TestWebAppKestrelFactory<BrowserServiceProgramm>(browserServiceHttpPort, browserServiceHttpsPort);

        _shopAPIWebAppFactory = new ShopApiConfigurationWebAppFactory<PostgresqlTestDbContainer,PostgresDbRespawner, PostgresDbHelper>
            ("ConnectionStrings:DbContext2", _dataBase, 5432, "postgres", "P@ssw0rd", _shopAPIHttpPort, _shopAPIHttpsPort, _signalRApplicationFactory.Server, _postgresTestDbContainer);

        _settingsAPIWebAppFactory = new SettingsApiConfigurationWebAppFactory<PostgresqlTestDbContainer, PostgresDbRespawner, PostgresDbHelper>
            ("ConnectionStrings:DbContext2", _dataBase, 5432, "postgres", "P@ssw0rd", _settingsAPIHttpPort, _settingsAPIHttpsPort, _signalRApplicationFactory.Server, _postgresTestDbContainer);

        _childJobDbInterceptor = new DbConfigurationContainerWebAppInterceptor<AggregateJobDbContext, PostgresqlTestDbContainer, PostgresDbRespawner, PostgresDbHelper>(
            this,
            this,
            "ConnectionStrings:ChildJobStoragePostgres",
            childjobstorage,
            "pguser",
            "p@ssw0rd",
            5432,
            _postgresTestDbContainer);
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
        services.SetSignalRHubTestAckReceiver(ShopImportWorkerKeys.AckEventMessageReceiverKey, _signalRApplicationFactory.Server, "events");
        services.SetSignalRHubTestSender(ShopImportWorkerKeys.EventMessageSenderKey, _signalRApplicationFactory.Server, "events");
    }

    public async Task InitializeAsync()
    {
        await _shopAPIWebAppFactory.InitializeAsync();
        
        await _settingsAPIWebAppFactory.InitializeAsync();

        await _childJobDbInterceptor.InitializeAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await (_settingsAPIWebAppFactory as IAsyncLifetime).DisposeAsync();

        await (_shopAPIWebAppFactory as IAsyncLifetime).DisposeAsync();

        await _childJobDbInterceptor.DisposeAsync();
    }

    protected override void ConfigureApp(WebHostBuilderContext context, IConfigurationBuilder config)
    {
        context.Configuration["ConnectionStrings:ServicesStoreRedis"] = _redisContainer.BuildConnectionString(_defaultRedisIndex.ToString(), -1);

        base.ConfigureApp(context, config);
    }

    public Task ResetDatabaseIfAvailableAsync()
    {
        return _childJobDbInterceptor.ResetDatabaseIfAvailableAsync();
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        _configureHost?.Invoke(host);

        return host;
    }
}