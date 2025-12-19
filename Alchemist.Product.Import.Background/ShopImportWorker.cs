using Alchemist.Product.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Message.Interfaces;
using Alchemist.DataService.Interfaces;
using Import.Interfaces;
using Import.Settings.Interfaces;
using Alchemist.Product.Interfaces;
using Alchemist.Import.Settings.Extensions;
using Alchemist.Product.Import.Background.Models;
using Alchemist.Messages.Common;
using Alchemist.Import.Settings.DataAdapter;
using System.Collections.Concurrent;
using Import.Factory.Interfaces;
using Alchemist.Import.Settings;

namespace Alchemist.Product.Import.Background;

public class ShopImportWorker : BackgroundService
{
    private readonly ILogger<ShopImportWorker> _logger;
    private readonly IEnumerable<IImportServiceFactory> _shopServiceFactories;
    private readonly IMessageReceiver _eventMessageReceiver;
    private readonly IMessageSender _eventMessageSender;
    private readonly IShopDataService _shopDataService;
    private readonly IEnumerable<ISettingsAdapter> _initSettingsAdapters;
    private readonly IDictionary<ShopSettingType, ISettingsDataAdapter> _processedSettingsAdapters;

    private record ServiceToken(IImportService Service, CancellationTokenSource InnerTokenSource);   

    private List<IImportSource> ShopModels { get; } = [];

    private readonly ConcurrentDictionary<Guid, ServiceToken> _servicesTokens = [];

    public ShopImportWorker(ILogger<ShopImportWorker> logger,
        [FromKeyedServices(ShopImportWorkerKeys.EventMessageReceiverKey)] IMessageReceiver eventMessageReceiver,
        [FromKeyedServices(ShopImportWorkerKeys.EventMessageSenderKey)] IMessageSender eventMessageSender,
        IShopDataService shopDataService,
        [FromKeyedServices(ShopImportWorkerKeys.InitImportSettings)]  IEnumerable<ISettingsAdapter> initSettingsAdapters,
        [FromKeyedServices(ShopImportWorkerKeys.ProcessedImportSettings)] IDictionary<ShopSettingType, ISettingsDataAdapter> processedSettingsAdapters,
        IEnumerable<IImportServiceFactory> shopImportFactories
        )
    {
        _logger = logger;
        _eventMessageReceiver = eventMessageReceiver;
        _eventMessageSender = eventMessageSender;
        _shopDataService = shopDataService;
        _initSettingsAdapters = initSettingsAdapters;
        _processedSettingsAdapters = processedSettingsAdapters;

        _shopServiceFactories = shopImportFactories;

        _eventMessageReceiver.On<ShopSettings>(Messages.Common.Messages.ShopSettingsCreated, OnShopSettingsCreatedAsync);       

        _eventMessageReceiver.On<ShopCategory>(Messages.Common.Messages.CategoryAdded, OnShopCategoryAdded);       

        _eventMessageReceiver.On<Guid>(Messages.Common.Messages.ServiceStop, OnStopServiceAsync);
    }

    private async Task OnStopServiceAsync(Guid guid)
    {
        _logger.LogInformation($"Stopping service with guid {guid} started.");
        if (_servicesTokens.TryGetValue(guid, out var serviceWithToken))
        {
            await serviceWithToken.InnerTokenSource.CancelAsync();

            _logger.LogInformation($"Service {serviceWithToken.Service.Name} stopped");

            await SendServiceMessageAsync(Messages.Common.Messages.ServiceStopped, guid);
        }
        else
        {
            _logger.LogWarning($"Stopping service is not available. Service with guid {guid} not found");

            await SendServiceMessageAsync(Messages.Common.Messages.ServiceEventError, 
                new ServiceErrorMessage 
                { Guid = guid, EventName = Messages.Common.Messages.ServiceStop, Message =  $"Service with guid {guid} not found" });
        }
    }

    private async Task OnStartServiceAsync(Guid guid, CancellationToken stoppingToken)
    {
        _logger.LogInformation($"Starting service with guid {guid}.");
        if (_servicesTokens.TryGetValue(guid, out var serviceWithToken))
            await StartServiceAsync(guid, serviceWithToken.Service, CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, serviceWithToken.InnerTokenSource.Token).Token);
        else
            await SendServiceMessageAsync(Messages.Common.Messages.ServiceEventError,
                new ServiceErrorMessage
                { Guid = guid, EventName = Messages.Common.Messages.ServiceStart, Message = $"Service with guid {guid} not found" });
    }

    private void OnShopCategoryAdded(ShopCategory shopCategory)
    {
        var shop = ShopModels.OfType<ProductShopModel>().FirstOrDefault(s => s.Id == shopCategory.ShopId);
        shop?.Categories.Add(new ProductShopCategoryModel { Category = shopCategory.Category, ItemId = shopCategory.ItemId });
    }

    private async Task OnShopSettingsCreatedAsync(ShopSettings newShopSettings)
    {
        if (ShopModels.OfType<ShopModel>().Any(s => s.Id == newShopSettings.ShopId))
            return;

        _logger.LogInformation($"Handling of settings {newShopSettings.Name} for shop id={newShopSettings.ShopId} started.");

        try
        {
            if (!_processedSettingsAdapters.TryGetValue(newShopSettings.Type, out var adapter))
                return;

            var shopImportSettings = await adapter.GetShopImportSettings(newShopSettings.Id);

            var shopModel = await _shopDataService.GetShopModelAsync(shopImportSettings);
            if (!ShopModels.Any(s => s.Name == shopModel.Name))
                ShopModels.Add(shopModel);

            if (!TryGetImportService(newShopSettings.Name, shopImportSettings, shopModel, out var service)
                || service is null) return;

            var guid = Guid.NewGuid();
            _servicesTokens.TryAdd(guid, new ServiceToken(service, new CancellationTokenSource()));

            _logger.LogInformation($"New service for shop {shopModel.Name} added");

            await SendServiceMessageAsync(Messages.Common.Messages.ServiceCreated, 
                new ServiceMessage { Guid = guid, Name = service.Name });
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
            var allShopImportSettings = await GetAllShopImportSettingsAsync();

            await Task.WhenAll(allShopImportSettings.Select(s => TryCreateServiceFromImportSettingsAsync(s.Key, s.Value)));

            _logger.LogInformation("Import services initialized.");
            
            foreach (var service in _servicesTokens)
            {
                await SendServiceMessageAsync(Messages.Common.Messages.ServiceCreated, 
                    new ServiceMessage { Guid = service.Key, Name = service.Value.Service.Name },
                    stoppingToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }

        try
        {
            await Parallel.ForEachAsync(_servicesTokens, (s, t) =>
                new ValueTask(StartServiceAsync(s.Key, s.Value.Service, 
                CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, s.Value.InnerTokenSource.Token).Token)));
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }
    }

    private async Task TryCreateServiceFromImportSettingsAsync(string settingsKey, IShopImportSettings shopImportSettings)
    {
        var shopModel = await _shopDataService.GetShopModelAsync(shopImportSettings);
        ShopModels.Add(shopModel);

        if (!TryGetImportService(settingsKey, shopImportSettings, shopModel, out var shopImportService)
            || shopImportService is null)
            return;

        var guid = Guid.NewGuid();
        _servicesTokens.TryAdd(guid, new ServiceToken(shopImportService, new CancellationTokenSource()));
    }

    private async Task StartEventMessageReceiverAsync(CancellationToken stoppingToken)
    {        
        try
        {
            async Task startServiceAsync(Guid guid) => await OnStartServiceAsync(guid, stoppingToken);

            await _eventMessageReceiver.Start(stoppingToken);

            _eventMessageReceiver.On<Guid>(Messages.Common.Messages.ServiceStart, startServiceAsync);
            
            _logger.LogInformation("Import service connected to messaging host.");
        }
        catch (Exception ex)
        {
            _logger.LogError( ex, ex.Message);
        }
    }

    private async Task<Dictionary<string, IShopImportSettings>> GetAllShopImportSettingsAsync()
    {
        var allShopImportSettings = new Dictionary<string, IShopImportSettings>();
        foreach (var adapter in _initSettingsAdapters)
        {
            var shopImportSettings = await adapter.GetAllShopImportSettings();
            var newSettings = shopImportSettings.Where(s => !allShopImportSettings.Any(s2 => s2.Key == s.Key));
            newSettings.ToList().ForEach(s => allShopImportSettings.Add(s.Key, s.Value));            
        }

        return allShopImportSettings;
    }

    private bool TryGetImportService(string name, IShopImportSettings shopImportSettings, IImportSource shopModel, out IImportService? service)
    {
        service = default;

        var importServiceSettings = shopImportSettings.GetImportService();
        if (importServiceSettings == null)
            return false;

        var serviceFactory = _shopServiceFactories.FirstOrDefault(f => f.ServiceImplementationType.Name == importServiceSettings.ImplementationTypeName);
        if (serviceFactory == null) return false;

        service = serviceFactory.Create(name, shopModel, shopImportSettings);
        return true;
    }

    private async Task StartServiceAsync(Guid guid, IImportService service, CancellationToken stoppingToken)
    {
        try
        {
            var serviceStartTask = service.Start(stoppingToken);

            await SendServiceMessageAsync(Messages.Common.Messages.ServiceStarted, guid, stoppingToken);

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

    private async Task SendServiceMessageAsync(string eventName, object message, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_eventMessageSender.IsConnected)
                await _eventMessageSender.Start(cancellationToken);

            await _eventMessageSender.Send(message, eventName, cancellationToken);                  
        }
        catch(Exception e)
        {
            _logger.LogError(e, $"Sending service message {message} for event {eventName} failed.");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach(var serviceToken in _servicesTokens.Where(s=>!s.Value.InnerTokenSource.IsCancellationRequested))        
            await serviceToken.Value.InnerTokenSource.CancelAsync();        

        await _eventMessageReceiver.Stop(cancellationToken);   
        
        await _eventMessageSender.Stop(cancellationToken);

        await base.StopAsync(cancellationToken);
    }  
}