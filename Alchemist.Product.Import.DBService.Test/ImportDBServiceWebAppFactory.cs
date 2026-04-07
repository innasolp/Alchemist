using Alchemist.Product.SignalR;
using Alchemist.Test.Host.Interfaces;
using Alchemist.Test.RabbitMQ;
using Alchemist.Test.Server.Fixtures;
using Message.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shop.API.Client;
using Shop.Interfaces;
using Test.PostresqlTestContainer;
using Testcontainers.PostgreSql;

namespace Alchemist.Product.Import.DBService.Test;

public class ImportDBServiceWebAppFactory : TestWebAppFactory<ImportDbServiceProgram>, IAsyncLifetime
{
    private readonly string _dataBase;

    private readonly PostgreSqlContainer _postgreSqlContainer;

    private GrpcServiceWebAppFactory? _grpcWebAppFactory;

    internal GrpcServiceWebAppFactory? GrpcWebAppFactory => _grpcWebAppFactory;

    private ShopAPIWebAppFactory? _shopAPIWebAppFactory;

    internal ShopAPIWebAppFactory? ShopAPIWebAppFactory => _shopAPIWebAppFactory;

    private readonly WebApplicationFactory<Startup> _signalRApplicationFactory;

    private readonly IMessageTestHost _importItemsHost = new RabbitMQTestHost();

    public IConfiguration? Configuration { get; private set; }

    public event Action<IServiceCollection> ConfigureServices;

    public ImportDBServiceWebAppFactory()
    {   
        var settings = new ConfigurationBuilder()
              .AddJsonFile("appsettings.json")
              .Build();

        _dataBase = settings.GetSection("alchemydb").Get<string>() ?? "test_ci_db";

        _postgreSqlContainer = PostgresqlTestContainerHelper.BuildPostgreSqlContainer(Guid.NewGuid().ToString());
        
        _signalRApplicationFactory = new WebApplicationFactory<Startup>();
        _signalRApplicationFactory.CreateClient();            
    }

    public void StartGrpc()
    {
        _grpcWebAppFactory?.CreateClient();            
    }

    private void SetReceiver(IServiceCollection services)
    {
        services.SetRabbitMqReceiver(_importItemsHost.Uri,
                Configuration.GetSection("RabbitMqExchangeOptions:ExchangeName").Get<string>(),
                Configuration.GetSection("RabbitMqQueueOptions:Name").Get<string>());
    }

    public IMessageSender CreateTestSender()
    {
        return _importItemsHost.CreatePublisher(Services,
            Configuration.GetSection("RabbitMqExchangeOptions:ExchangeName").Get<string>());
    }

    protected override void ConfigureWebHostBuilderContext(WebHostBuilderContext context, IServiceCollection services)
    {
        Configuration = context.Configuration;
        
        var joinableTaskFactory = new Microsoft.VisualStudio.Threading.JoinableTaskFactory(new Microsoft.VisualStudio.Threading.JoinableTaskContext());
        joinableTaskFactory.Run(async () =>
        {
            await _importItemsHost.Start();
        });

        var shopApiClient = _shopAPIWebAppFactory.CreateClient();
        shopApiClient.BaseAddress = new Uri(_shopAPIWebAppFactory.ServerAddress);
        services.InterceptImplementation<IShopDataService, ShopApiClient>(new ShopApiClient(shopApiClient));

        SetReceiver(services);

        ConfigureServices?.Invoke(services);
    }

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();

        var connectionString = _postgreSqlContainer.BuildConnectionString(_dataBase);

        _grpcWebAppFactory = new GrpcServiceWebAppFactory(connectionString);

        _shopAPIWebAppFactory = new ShopAPIWebAppFactory(connectionString, _signalRApplicationFactory.Server);
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync();
    }
}