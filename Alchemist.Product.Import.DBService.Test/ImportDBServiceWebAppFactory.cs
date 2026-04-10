using Alchemist.Product.GrpcServiceClient;
using Alchemist.Product.Interfaces;
using Alchemist.Product.SignalR;
using Alchemist.Test.Host.Interfaces;
using Alchemist.Test.Log;
using Alchemist.Test.RabbitMQ;
using Alchemist.Test.Server.Fixtures;
using Message.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shop.API.Client;
using Shop.Interfaces;

namespace Alchemist.Product.Import.DBService.Test;

public class ImportDBServiceWebAppFactory : TestWebAppKestrelFactory<ImportDbServiceProgram>, IAsyncLifetime, ILoggedContext
{
    internal GrpcServiceWebAppFactory GrpcWebAppFactory{ get; }

    internal ShopAPIWebAppFactory ShopAPIWebAppFactory{ get; }

    private readonly WebApplicationFactory<Startup> _signalRApplicationFactory;

    private readonly IMessageTestHost _importItemsHost = new RabbitMQTestHost();

    public IConfiguration? Configuration { get; private set; }

    public event Action<IServiceCollection>? ConfigureServices;

    public FixtureLoggerFactoryContext FixtureLoggingContext { get; } = new FixtureLoggerFactoryContext();

    FixtureLogContext ILoggedContext.FixtureLoggingContext => FixtureLoggingContext;

    public HttpClient? GrpcClient { get; private set; }

    public ImportDBServiceWebAppFactory(int httpPort, int httpsPort, string database, int shopAPIHttpPort, int shopAPIHttpsPort,
        int grpcAPIHttpPort, int grpcAPIHttpsPort) : base(httpPort, httpsPort)
    {   
        //var settings = new ConfigurationBuilder()
        //      .AddJsonFile("appsettings.json")
        //      .Build();

        //_dataBase = settings.GetSection("alchemydb").Get<string>() ?? "test_ci_db";

        ///var database = "test_ci_db";

        _signalRApplicationFactory = new WebApplicationFactory<Startup>();
        _signalRApplicationFactory.CreateClient();

        ShopAPIWebAppFactory = new ShopAPIWebAppFactory(database, shopAPIHttpPort, shopAPIHttpsPort, _signalRApplicationFactory.Server);

        GrpcWebAppFactory = new GrpcServiceWebAppFactory(database, grpcAPIHttpPort, grpcAPIHttpsPort);
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

        
        //GrpcClient = GrpcWebAppFactory.CreateClient();
        //var grpcChannel = this.CreateChannel(GrpcClient.BaseAddress.ToString());
        //services.InterceptImplementation<IProductDataService, AlchemyGrpcServiceClient>(new AlchemyGrpcServiceClient(grpcChannel));
        services.RemoveImplementations<IProductDataService, AlchemyGrpcServiceClient>();
        services.AddSingleton<IProductDataService>(sp =>
        {
            GrpcClient = GrpcWebAppFactory.GetHostHttpClient();
            var grpcChannel = GrpcWebAppFactory.CreateChannel(GrpcWebAppFactory.ServerAddress);
            return new AlchemyGrpcServiceClient(grpcChannel);
        });

        //services.RemoveImplementations<IProductDataService, AlchemyGrpcServiceClient>();
        //services.AddScoped(sp =>
        //{
        //    // Этот код выполнится при первом внедрении MyClient в контроллер/сервис
        //    var handler = _factoryA.Server.CreateHandler();

        //    var channel = GrpcChannel.ForAddress(_factoryA.Server.BaseAddress, new GrpcChannelOptions
        //    {
        //        HttpHandler = handler
        //    });

        //    return new MyClient(channel);
        //});

        //var shopApiClient = ShopAPIWebAppFactory.GetHostHttpClient();
        //services.InterceptImplementation<IShopDataService, ShopApiClient>new ShopApiClient(shopApiClient));
        services.RemoveImplementations<IShopDataService, ShopApiClient>();
        services.AddSingleton<IShopDataService>(sp =>
        {
            var shopApiClient = ShopAPIWebAppFactory.GetHostHttpClient();
            return new ShopApiClient(shopApiClient);
        });
        
        SetReceiver(services);

        ConfigureServices?.Invoke(services);

        FixtureLoggingContext.ConfigureServices(services);
    }

    public async Task InitializeAsync()
    {
        await ShopAPIWebAppFactory.InitializeAsync();

        await GrpcWebAppFactory.InitializeAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await (ShopAPIWebAppFactory as IAsyncLifetime).DisposeAsync();

        await (GrpcWebAppFactory as IAsyncLifetime).DisposeAsync();
    }
}