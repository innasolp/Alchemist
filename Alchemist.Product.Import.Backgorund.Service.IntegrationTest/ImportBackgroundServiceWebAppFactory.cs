using Alchemist.Product.SignalR;
using Alchemist.Test.EventBus.Interfaces;
using Alchemist.Test.RabbitMQ;
using Message.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Alchemist.Test.SignalRWebAppFactory;
using Alchemist.Product.Import.Background;

namespace Alchemist.Product.Import.Backgorund.Service.IntegrationTest;

public class ImportBackgroundServiceWebAppFactory : WebApplicationFactory<ImportBackgroundServiceProgram>
{
    private readonly SettingsAPIWebAppFactory _settingsAPIWebAppFactory;

    private readonly ShopAPIWebAppFactory _shopAPIWebAppFactory;

    private readonly WebApplicationFactory<Startup> _signalRApplicationFactory;
    
    private readonly ITestHost _importItemsHost = new RabbitMQTestHost();

    private IConfiguration? _configuration;

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
    }
        
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureServices((context, services) =>
        {
            _configuration = context.Configuration;

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
     
    public IMessageReceiver CreateImportItemReceiver()
    {
        return _importItemsHost.CreateSubscriber(Services,
            _configuration.GetSection("RabbitMqExchangeOptions:ExchangeName").Get<string>(),
            _configuration.GetSection("RabbitMqQueueOptions:Name").Get<string>());
    }
}
