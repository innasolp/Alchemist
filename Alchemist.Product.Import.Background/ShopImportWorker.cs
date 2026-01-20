using Alchemist.Import.Settings;
using Alchemist.Product.Entities;
using Import.Service.Commands;
using MediatR;
using Message.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Alchemist.Product.Import.Background;

public class ShopImportWorker : BackgroundService
{
    private readonly ILogger<ShopImportWorker> _logger;
    private readonly IMessageReceiver _eventMessageReceiver;
    private readonly IMessageSender _eventMessageSender;
    private readonly IEnumerable<ISettingsAdapter> _initSettingsAdapters;
    private readonly IMediator _mediator;

    private readonly SemaphoreSlim _shopCategorySemaphore = new(1, 1);

    public ShopImportWorker(ILogger<ShopImportWorker> logger,
        IMediator mediator,
        [FromKeyedServices(ShopImportWorkerKeys.EventMessageReceiverKey)] IMessageReceiver eventMessageReceiver,
        [FromKeyedServices(ShopImportWorkerKeys.EventMessageSenderKey)] IMessageSender eventMessageSender,
        [FromKeyedServices(ShopImportWorkerKeys.InitImportSettings)]  IEnumerable<ISettingsAdapter> initSettingsAdapters)
    {
        _logger = logger;
        _mediator = mediator;
        _eventMessageReceiver = eventMessageReceiver;
        _eventMessageSender = eventMessageSender;
        _initSettingsAdapters = initSettingsAdapters;

        _eventMessageReceiver.On<Settings.ShopSettings>(Messages.Common.Messages.ShopSettingsCreated, OnShopSettingsCreatedAsync);//, typeof(ShopSettings));

        _eventMessageReceiver.On<ShopCategory>(Messages.Common.Messages.CategoryAdded, OnShopCategoryAdded);

        _eventMessageReceiver.On<Guid>(Messages.Common.Messages.ServiceStop, OnStopServiceAsync);
        _eventMessageReceiver.On<ServiceMessage>(Messages.Common.Messages.ServiceStopped, OnServiceStoppedAsync);
    }

    private async Task OnServiceStoppedAsync(ServiceMessage serviceMessage)
    {
        _logger.LogInformation($"Service {serviceMessage.Name} {serviceMessage.Guid} stopped");
    }

    private async Task OnStopServiceAsync(Guid guid)
    {
        _logger.LogInformation($"Stopping service {guid} started.");

        try
        {
            await _mediator.Send(new StopServiceCommand(guid));            
        }
        catch(Exception ex)
        {
            _logger.LogWarning($"Service {guid} stopping error : {ex.Message}", ex);
        }
    }

    private async Task OnStartServiceAsync(Guid guid, CancellationToken stoppingToken)
    {
        _logger.LogInformation($"Starting service with guid {guid}.");

        try
        {
            await _mediator.Send(new StartServiceCommand(guid), stoppingToken);
            _logger.LogInformation($"Service {guid} started");
        }
        catch(Exception e)
        {
            _logger.LogError($"Service {guid} starting failed with error", e);
        }
    }

    private async Task OnShopCategoryAdded(ShopCategory shopCategory)
    {
        if (string.IsNullOrEmpty(shopCategory.Url)) return;

        await _shopCategorySemaphore.WaitAsync();

        try
        {
            await _mediator.Send(new QueueShopCategoryToServicesCommand(shopCategory));
        }
        finally
        {
            _shopCategorySemaphore.Release();
        }
    }

    private async Task OnShopSettingsCreatedAsync(Settings.ShopSettings newShopSettings)//  (object newShopSettingsDto)
    {
        _logger.LogInformation($"Handling of settings {newShopSettings.Name} for shop id={newShopSettings.ShopId} started.");

        try
        {
            var (success, guid) = await _mediator.Send(new AddShopImportServiceFromShopSettingsCommand(newShopSettings));

            if(success)
             _logger.LogInformation($"New service {newShopSettings.Name} with id {guid} added");
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
            var allShopImportSettings = await GetAllShopImportSettingsAsync(stoppingToken);

            await Task.WhenAll(allShopImportSettings.Select(s => _mediator.Send(new AddShopImportServiceCommand(s.Key, s.Value), stoppingToken)));

            _logger.LogInformation("Import services initialized.");    
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }

        try
        {
            var startAllTask = _mediator.Send(new StartAllServicesCommand(), stoppingToken);
            await startAllTask;
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }
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

    private async Task<Dictionary<string, IShopImportSettings>> GetAllShopImportSettingsAsync(CancellationToken cancellationToken = default)
    {
        var allShopImportSettings = new Dictionary<string, IShopImportSettings>();
        foreach (var adapter in _initSettingsAdapters)
        {
            var shopImportSettings = await adapter.GetAllShopImportSettings(cancellationToken);
            var newSettings = shopImportSettings.Where(s => !allShopImportSettings.Any(s2 => s2.Key == s.Key));
            newSettings.ToList().ForEach(s => allShopImportSettings.Add(s.Key, s.Value));            
        }

        return allShopImportSettings;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _mediator.Send(new StopAllServicesCommand(), cancellationToken);    

        await _eventMessageReceiver.Stop(cancellationToken);   
        
        await _eventMessageSender.Stop(cancellationToken);

        await base.StopAsync(cancellationToken);
    }  
}