using Alchemist.Product.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Message.Interfaces;
using Alchemist.Common;
using Alchemist.DataService.Interfaces;
using Alchemist.Import.Interfaces;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.Import.Category.Interfaces;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Product.Interfaces;
using Alchemist.Import.Settings.Extensions;
using Alchemist.Product.Import.Background.Settings;
using Alchemist.Product.Import.Background.Models;
using Alchemist.Import.Service.Factory.Interfaces;

namespace Alchemist.Product.Import.Background;

public class ShopImportWorker : BackgroundService
{
    private readonly ILogger<ShopImportWorker> _logger;
    private readonly IEnumerable<IShopImportServiceFactory> _shopServiceFactories;
    private readonly IMessageReceiver _eventMessageReceiver;
    private readonly IMessageSender _eventMessageSender;
    private readonly IShopDataService _shopDataService;
    private readonly IEnumerable<ISettingsAdapter> _settingsAdapters;
    private readonly IEnumerable<IMessageSender> _itemMessageSenders;
    private readonly IProductItemHandler _productDataHandler;
    private readonly ICategoryItemHandler? _categoryDataHandler;
    private readonly IShopSettingsDataService _settingsDataService;

    private record ServiceWithToken(IImportService Service, CancellationTokenSource InnerTokenSource);   

    private List<IShopItem> ShopModels { get; } = [];

    private readonly Dictionary<Guid, ServiceWithToken> _servicesWithTokens = [];

    public ShopImportWorker(ILogger<ShopImportWorker> logger,
        [FromKeyedServices(ShopImportWorkerKeys.EventMessageReceiverKey)]
        IMessageReceiver eventMessageReceiver,
        [FromKeyedServices(ShopImportWorkerKeys.EventMessageSenderKey)]
        IMessageSender eventMessageSender,
        IShopDataService shopDataService,
        IEnumerable<ISettingsAdapter> settingsAdapters,
        IEnumerable<IShopImportServiceFactory> shopImportFactories,
        [FromKeyedServices(ShopImportWorkerKeys.ShopsMessageSenderKey)]
        IEnumerable<IMessageSender> itemMessageSenders,
        IProductItemHandler productDataHandler,
        IShopSettingsDataService settingsDataService)
    {
        _logger = logger;
        _eventMessageReceiver = eventMessageReceiver;
        _eventMessageSender = eventMessageSender;
        _shopDataService = shopDataService;
        _itemMessageSenders = itemMessageSenders;
        _productDataHandler = productDataHandler;
        _settingsAdapters = settingsAdapters;

        _shopServiceFactories = shopImportFactories;

        _productDataHandler.ItemProcessed += ProductItemHandledAsync;

        _eventMessageReceiver.On<ShopSettings>(Messages.Common.Messages.ReceiveShopSettingsCreated, OnShopSettingsCreatedAsync);       

        _eventMessageReceiver.On<ShopCategory>(Messages.Common.Messages.ReceiveCategoryAdded, OnShopCategoryAdded);       

        _eventMessageReceiver.On<Guid>(Messages.Common.Messages.ReceiveServiceStop, StopServiceAsync);

        _settingsDataService = settingsDataService;
    }

    private async Task StopServiceAsync(Guid guid)
    {
        _logger.LogInformation($"Stopping service with guid {guid} started.");
        if (_servicesWithTokens.TryGetValue(guid, out var serviceWithToken))
        {
            await serviceWithToken.InnerTokenSource.CancelAsync();

            _logger.LogInformation($"Service {serviceWithToken.Service.Name} stopped");

            await SendServiceMessageAsync(Messages.Common.Messages.SendServiceStopped, guid);
        }
        else
        {
            _logger.LogWarning($"Stopping service is not available. Service with guid {guid} not found");

            await SendServiceMessageAsync(Messages.Common.Messages.SendServiceEventError, 
                guid, 
                null,
                Messages.Common.Messages.ReceiveServiceStop, 
                $"Service with guid {guid} not found");
        }
    }

    private async Task OnStartServiceAsync(Guid guid, CancellationToken stoppingToken)
    {
        _logger.LogInformation($"Starting service with guid {guid}.");
        if (_servicesWithTokens.TryGetValue(guid, out var serviceWithToken))
            await StartServiceAsync(guid, serviceWithToken.Service, CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, serviceWithToken.InnerTokenSource.Token).Token);
        else
            await SendServiceMessageAsync(Messages.Common.Messages.SendServiceEventError,
                guid, null, Messages.Common.Messages.ReceiveServiceStart,  $"Service for guid {guid}  not found");
    }

    public ShopImportWorker(ILogger<ShopImportWorker> logger,
        [FromKeyedServices(ShopImportWorkerKeys.EventMessageReceiverKey)]
        IMessageReceiver eventMessageReceiver,
        [FromKeyedServices(ShopImportWorkerKeys.EventMessageSenderKey)]
        IMessageSender eventMessageSender,
        IShopDataService shopDataService,
        IEnumerable<ISettingsAdapter> settingsAdapters,
        IEnumerable<IShopImportServiceFactory> shopImportFactories,
        [FromKeyedServices(ShopImportWorkerKeys.ShopsMessageSenderKey)]
        IEnumerable<IMessageSender> itemMessageSenders,
        IProductItemHandler productDataHandler,
        ICategoryItemHandler categoryDataHandler,
        IShopSettingsDataService settingsDataService)
        : this(logger, eventMessageReceiver, eventMessageSender, shopDataService, settingsAdapters, shopImportFactories, itemMessageSenders, productDataHandler, settingsDataService)
    {
        _categoryDataHandler = categoryDataHandler;
        _categoryDataHandler.ItemProcessed += CategoryHandledAsync;
    }

    private async Task CategoryHandledAsync(object sender, IImportCategory item, ResultStatus processStatus)
    {
        var categoryModel = new ImportCategory { Category = item.Category.Name, ItemId = item.Category.Id, Status = processStatus };

        if (item.CategoryShopModel is IShop shop) categoryModel.ShopId = shop.Id;
        await SendItemMessagesAsync(categoryModel, Messages.Common.Messages.SendCategoryItem);
    }

    private async Task SendItemMessagesAsync<T>(T item, string eventName)
        where T:class, IItem
    {
        foreach (var itemMessageSender in _itemMessageSenders)
        {
            try
            {
                if (!itemMessageSender.IsConnected)
                    await itemMessageSender.Start();

                await itemMessageSender.Send(item, eventName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Category item {item.Name} handling for event {eventName} failed in message sender {itemMessageSender.GetType()}.");
            }
        }
    }


    private async Task ProductItemHandledAsync(object sender, IImportProduct item, ResultStatus status)
    {
        var productItemModel = new ImportProduct { Name = item.ProductItem.Name, ShopName = item.Shop.ShopName, Url = item.ProductItem.Url, Status = status };
        await SendItemMessagesAsync(productItemModel, Messages.Common.Messages.SendProductItem);
    }

    private void OnShopCategoryAdded(ShopCategory shopCategory)
    {
        var shop = ShopModels.OfType<ProductShopModel>().FirstOrDefault(s => s.Id == shopCategory.ShopId);
        shop?.Categories.Add(new ProductShopCategoryModel { Category = shopCategory.Category, ItemId = shopCategory.ItemId });
    }

    private async Task OnShopSettingsCreatedAsync(ShopSettings newShopSettings)
    {
        if (ShopModels.OfType<IShopModel>().Any(s => s.Id == newShopSettings.ShopId))
            return;

        _logger.LogInformation($"Handling of settings {newShopSettings.Name} for shop id={newShopSettings.ShopId} started.");

        try
        {
            var serviceSettings = await _settingsDataService.GetChildSettings(newShopSettings.Id);
            var shopImportSettings = newShopSettings.GetShopImportSettings(serviceSettings);

            var shopModel = await _shopDataService.GetShopModelAsync(shopImportSettings);
            if (!ShopModels.Any(s => s.ShopName == shopModel.ShopName))
                ShopModels.Add(shopModel);

            if (!TryGetImportService(shopImportSettings, shopModel, out var service)) return;

            var guid = Guid.NewGuid();
            _servicesWithTokens.Add(guid, new ServiceWithToken(service, new CancellationTokenSource()));

            _logger.LogInformation($"New service for shop {shopModel.ShopName} added");

            await SendServiceMessageAsync(Messages.Common.Messages.SendServiceCreated, guid, service.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Service creation for shop settings {newShopSettings.Name} failed.");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await StartEventMessageReceiverAsync(stoppingToken);

        try
        {
            List<IShopImportSettings> allShopImportSettings = await GetAllShopImportSettingsAsync();

            await CreateServicesFromImportSettingsAsync(allShopImportSettings);           

            _logger.LogInformation("Import services initialized.");
            
            foreach (var service in _servicesWithTokens)
            {
                await SendServiceMessageAsync(Messages.Common.Messages.SendServiceCreated, service.Key, service.Value.Service.Name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
        }

        try
        {
            await Parallel.ForEachAsync(_servicesWithTokens, (s, t) =>
                new ValueTask(StartServiceAsync(s.Key, s.Value.Service, CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, s.Value.InnerTokenSource.Token).Token)));
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }
    }

    private async Task CreateServicesFromImportSettingsAsync(IEnumerable<IShopImportSettings> allShopImportSettings)
    {
        foreach (var shopImportSettings in allShopImportSettings)
        {
            var shopModel = await _shopDataService.GetShopModelAsync(shopImportSettings);
            ShopModels.Add(shopModel);

            if(!TryGetImportService(shopImportSettings, shopModel, out var shopImportService))
                continue;

            var guid = Guid.NewGuid();
            _servicesWithTokens.Add(guid, new ServiceWithToken(shopImportService, new CancellationTokenSource()));            
        }
    }

    private async Task StartEventMessageReceiverAsync(CancellationToken stoppingToken)
    {
        async Task startServiceAsync(Guid guid) => await OnStartServiceAsync(guid, stoppingToken);
        try
        {
            await _eventMessageReceiver.Start();

            _eventMessageReceiver.On(Messages.Common.Messages.ReceiveServiceStart, (Func<Guid, Task>)startServiceAsync);
            
            _logger.LogInformation("Import service connected to messaging host.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
        }
    }

    private async Task<List<IShopImportSettings>> GetAllShopImportSettingsAsync()
    {
        var allShopImportSettings = new List<IShopImportSettings>();
        foreach (var adapter in _settingsAdapters)
        {
            var shopImportSettings = await adapter.GetAllShopImportSettings();
            var newSettings = shopImportSettings.Where(s => !allShopImportSettings.Any(s2 => s2.Name == s.Name));
            allShopImportSettings.AddRange(newSettings);
        }

        return allShopImportSettings;
    }

    private bool TryGetImportService(IShopImportSettings shopImportSettings, IShopItem shopModel, out IImportService service)
    {
        service = default;

        var importServiceSettings = shopImportSettings.GetImportService();
        if (importServiceSettings == null)
            return false;

        var serviceFactory = _shopServiceFactories.FirstOrDefault(f => f.ServiceImplementationType.Name == importServiceSettings.ImplementationTypeName);
        if (serviceFactory == null) return false;

        service = serviceFactory.Create(shopModel, shopImportSettings);
        return true;
    }

    private async Task StartServiceAsync(Guid guid, IImportService service, CancellationToken stoppingToken)
    {
        try
        {
            var serviceStartTask = service.Start(stoppingToken);

            await SendServiceMessageAsync(Messages.Common.Messages.SendServiceStarted, guid);

            await serviceStartTask.WaitAsync(stoppingToken);
        }
        catch (AggregateException ae)
        {
            foreach (Exception e in ae.InnerExceptions)
            {
                if (e is TaskCanceledException)
                {
                    //todo send message
                    if (e.InnerException != null)
                        _logger.LogError(e.InnerException, $"Service {service.Name} was canceled with errror.");
                    else
                        _logger.LogInformation($"Service {service.Name} was canceled");
                }
                else
                {
                    //todo send message
                    _logger.LogError(e, $"Service {service.Name} was failed.");
                } 
            }
        }
    }

    private async Task SendServiceMessageAsync(string eventName, Guid guid, string? serviceName  = null, params object[]? parameters)
    {
        try
        {
            if (!_eventMessageSender.IsConnected)
                await _eventMessageSender.Start();

            if(serviceName == null)
                await _eventMessageSender.Send(guid, eventName);
            else
            {
                var messageParameters = new List<object>() { guid, serviceName };
                if (parameters != null) messageParameters.AddRange(parameters);

                await _eventMessageSender.Send(messageParameters.ToArray(), eventName);
            }            
        }
        catch(Exception e)
        {
            _logger.LogError(e, $"Sending service {serviceName} message for event {eventName} failed.");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _eventMessageReceiver.Stop();   
        
        await _eventMessageSender.Stop();

        foreach (var itemMessageSender in _itemMessageSenders)
            await itemMessageSender.Stop();

        await base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _productDataHandler.ItemProcessed -= ProductItemHandledAsync;
        
        if (_categoryDataHandler != null)
            _categoryDataHandler.ItemProcessed -= CategoryHandledAsync;
        
        base.Dispose();
    }
}
