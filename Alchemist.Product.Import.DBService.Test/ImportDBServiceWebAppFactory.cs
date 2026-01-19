using Alchemist.Test.Server.Fixtures;
using Alchemist.Product.SignalR;
using Alchemist.Test.Host.Interfaces;
using Alchemist.Test.RabbitMQ;
using Message.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shop.API.Client;
using Shop.Interfaces;

namespace Alchemist.Product.Import.DBService.Test;

public class ImportDBServiceWebAppFactory : TestWebAppFactory<ImportDbServiceProgram>
{
    private readonly GrpcServiceWebAppFactory _grpcWebAppFactory;

    internal GrpcServiceWebAppFactory GrpcWebAppFactory => _grpcWebAppFactory;

    private readonly ShopAPIWebAppFactory _shopAPIWebAppFactory;

    internal ShopAPIWebAppFactory ShopAPIWebAppFactory => _shopAPIWebAppFactory;

    private readonly WebApplicationFactory<Startup> _signalRApplicationFactory;

    private readonly ITestHost _importItemsHost = new RabbitMQTestHost();

    public IConfiguration? Configuration { get; private set; }

    public event Action<IServiceCollection> ConfigureServices;

    public ImportDBServiceWebAppFactory()
    {   
        var settings = new ConfigurationBuilder()
              .AddJsonFile("appsettings.json")
              .Build();

        var alchemyDbConnectionString = settings.GetConnectionString("alchemydb");

        _grpcWebAppFactory = new GrpcServiceWebAppFactory(alchemyDbConnectionString);       

        _signalRApplicationFactory = new WebApplicationFactory<Startup>();
        _signalRApplicationFactory.CreateClient();

        _shopAPIWebAppFactory = new ShopAPIWebAppFactory(alchemyDbConnectionString, _signalRApplicationFactory.Server);       
    }

    public void StartGrpc()
    {
        _grpcWebAppFactory.CreateClient();            
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
}
