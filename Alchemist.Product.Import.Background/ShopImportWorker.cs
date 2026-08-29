using Alchemist.Import.Settings;
using Alchemist.Product.Entities;
using Db.Infrastructure;
using Import.Service.Infrastructure;
using Message.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Alchemist.Product.Import.Background;

public class ShopImportWorker : BackgroundService
{
    private readonly ILogger<ShopImportWorker> _logger;
    private readonly IMessageReceiver _eventMessageReceiver;
    private readonly IAcknowlegefulMessageReceiver _ackEventMessageReceiver;
    private readonly IMessageSender _eventMessageSender;
    private readonly IEnumerable<ISettingsAdapter> _initSettingsAdapters;

    private readonly ICommandHandler<StartServiceCommand> _startServiceCommandHandler;
    private readonly ICommandHandler<StopServiceCommand> _stopServiceCommandHandler;
    private readonly ICommandHandler<QueueShopCategoryToServicesCommand> _queueShopCategoryToServicesCommandHandler;
    private readonly ICommandHandler<AddShopImportServiceFromShopSettingsCommand, Guid> _addShopImportServiceFromShopSettingsCommandHandler;
    private readonly ICommandHandler<AddImportServiceCommand, Guid> _addImportServiceCommandHandler;
    private readonly ICommandHandler<StopAllServicesCommand> _stopAllServicesCommandHandler;

    private readonly SemaphoreSlim _shopCategorySemaphore = new(1, 1);

    public ShopImportWorker(ILogger<ShopImportWorker> logger,
        [FromKeyedServices(ShopImportWorkerKeys.EventMessageReceiverKey)] IMessageReceiver eventMessageReceiver,
        [FromKeyedServices(ShopImportWorkerKeys.AckEventMessageReceiverKey)] IAcknowlegefulMessageReceiver ackEventMessageReceiver,
        [FromKeyedServices(ShopImportWorkerKeys.EventMessageSenderKey)] IMessageSender eventMessageSender,
        [FromKeyedServices(ShopImportWorkerKeys.InitImportSettings)] IEnumerable<ISettingsAdapter> initSettingsAdapters,
        ICommandHandler<StartServiceCommand> startServiceCommandHandler,
        ICommandHandler<StopServiceCommand> stopServiceCommandHandler,
        ICommandHandler<QueueShopCategoryToServicesCommand> queueShopCategoryToServicesCommandHandler,
        ICommandHandler<AddShopImportServiceFromShopSettingsCommand, Guid> addShopImportServiceFromShopSettingsCommandHandler,
        ICommandHandler<StopAllServicesCommand> stopAllServicesCommandHandler,
        ICommandHandler<AddImportServiceCommand, Guid> addImportServiceCommandHandler)
    {
        _logger = logger;
        _eventMessageReceiver = eventMessageReceiver;
        _ackEventMessageReceiver = ackEventMessageReceiver;
        _eventMessageSender = eventMessageSender;
        _initSettingsAdapters = initSettingsAdapters;

        _eventMessageReceiver.On<ShopCategory>(Messages.Common.Messages.CategoryAdded, OnShopCategoryAdded);

        _eventMessageReceiver.On<ServiceMessage>(Messages.Common.Messages.ServiceStarting, OnServiceStarting);
        _eventMessageReceiver.On<ServiceStartedMessage>(Messages.Common.Messages.ServiceStarted, OnServiceStarted);
        _eventMessageReceiver.On<Guid>(Messages.Common.Messages.ServiceStop, OnStopServiceAsync);
        _eventMessageReceiver.On<ServiceMessage>(Messages.Common.Messages.ServiceStopped, OnServiceStoppedAsync);

        _startServiceCommandHandler = startServiceCommandHandler;
        _stopServiceCommandHandler = stopServiceCommandHandler;
        _queueShopCategoryToServicesCommandHandler = queueShopCategoryToServicesCommandHandler;
        _addShopImportServiceFromShopSettingsCommandHandler = addShopImportServiceFromShopSettingsCommandHandler;
        _stopAllServicesCommandHandler = stopAllServicesCommandHandler;
        _addImportServiceCommandHandler = addImportServiceCommandHandler;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (!_ackEventMessageReceiver.IsConnected)
                await _ackEventMessageReceiver.Start(cancellationToken);

            await _ackEventMessageReceiver.On<Settings.ShopSettings>(Messages.Common.Messages.ShopSettingsCreated, OnShopSettingsCreatedAsync, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect import service to ack events messaging host: {Message}", ex.Message);
        }

        await base.StartAsync(cancellationToken);
    }   

    private Task OnServiceStarted(ServiceStartedMessage serviceStartedMessage)
    {
        if (serviceStartedMessage.Success)
            _logger.LogInformation("Service {Name} {Guid} started successfully.", serviceStartedMessage.Name, serviceStartedMessage.Guid);
        else
            _logger.LogWarning("Service {Name} {Guid} failed on start.", serviceStartedMessage.Name, serviceStartedMessage.Guid);

        return Task.CompletedTask;
    }

    private Task OnServiceStarting(ServiceMessage serviceMessage)
    {
        _logger.LogInformation("Service {Name} {Guid} starting.", serviceMessage.Name, serviceMessage.Guid);
        return Task.CompletedTask;
    }

    private Task OnServiceStoppedAsync(ServiceMessage serviceMessage)
    {
        _logger.LogInformation("Service {Name} {Guid} stopped.", serviceMessage.Name, serviceMessage.Guid);
        return Task.CompletedTask;
    }

    private async Task OnStopServiceAsync(Guid guid)
    {
        _logger.LogInformation("Stopping service {Guid} started.", guid);

        try
        {
            await _stopServiceCommandHandler.Handle(new StopServiceCommand(guid));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Service {Guid} stopping error.", guid);
        }
    }

    private async Task StartServiceAsync(Guid guid, CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting service with guid {Guid}.", guid);

        try
        {
            await _startServiceCommandHandler.Handle(new StartServiceCommand(guid), stoppingToken);
            _logger.LogInformation("Service {Guid} started.", guid);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Service {Guid} starting failed.", guid);
        }
    }

    private async Task OnShopCategoryAdded(ShopCategory shopCategory)
    {
        if (string.IsNullOrEmpty(shopCategory.Url)) return;

        var entered = false;
        try
        {
            entered = await _shopCategorySemaphore.WaitAsync(TimeSpan.FromSeconds(30));
            if (!entered)
            {
                _logger.LogWarning("Timeout waiting for shop category semaphore for url {Url}. Skipping.", shopCategory.Url);
                return;
            }

            await _queueShopCategoryToServicesCommandHandler.Handle(new QueueShopCategoryToServicesCommand(shopCategory));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while queueing shop category {Url}.", shopCategory.Url);
        }
        finally
        {
            if (entered)
                _shopCategorySemaphore.Release();
        }
    }

    private async Task OnShopSettingsCreatedAsync(string msgId, Settings.ShopSettings newShopSettings)
    {
        _logger.LogInformation("Handling of settings {Name} for shop id={ShopId} started.", newShopSettings.Name, newShopSettings.ShopId);

        try
        {
            var guid = await _addShopImportServiceFromShopSettingsCommandHandler.Handle(new AddShopImportServiceFromShopSettingsCommand(newShopSettings));

            _logger.LogInformation("New service {Name} with id {Guid} added", newShopSettings.Name, guid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service creation for shop settings {Name} failed.", newShopSettings.Name);
        }
    }

    private async Task StartNewService(string name, IShopImportSettings shopImportSettings, CancellationToken cancellationToken)
    {
        Guid serviceGuid;

        try
        {
            serviceGuid = await _addImportServiceCommandHandler.Handle(new AddImportServiceCommand(name, shopImportSettings), cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Couldn't create the service {Name}.", name);
            return;
        }

        _logger.LogInformation("Service {Name} is initialized.", name);

        try
        {
            _logger.LogInformation("Starting service with guid {Guid}.", serviceGuid);
            await _startServiceCommandHandler.Handle(new StartServiceCommand(serviceGuid), cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Couldn't start the service {Name} {serviceGuid}.", name, serviceGuid);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await StartEventMessageReceiverAsync(stoppingToken);

        try
        {
            var allShopImportSettings = await GetAllShopImportSettingsAsync(stoppingToken);

            await Parallel.ForEachAsync(allShopImportSettings, stoppingToken, async (kvp, token) =>
            {
                await StartNewService(kvp.Key, kvp.Value, token);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during startup: {Message}", ex.Message);
        }
    }

    private async Task StartEventMessageReceiverAsync(CancellationToken stoppingToken)
    {
        try
        {
            async Task startServiceAsync(Guid guid) => await StartServiceAsync(guid, stoppingToken);

            await _eventMessageReceiver.Start(stoppingToken);

            _eventMessageReceiver.On<Guid>(Messages.Common.Messages.ServiceStart, startServiceAsync);

            _logger.LogInformation("Import service connected to messaging host.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect import service to messaging host: {Message}", ex.Message);
        }
    }

    private async Task<Dictionary<string, IShopImportSettings>> GetAllShopImportSettingsAsync(CancellationToken cancellationToken = default)
    {
        var allShopImportSettings = new Dictionary<string, IShopImportSettings>(StringComparer.OrdinalIgnoreCase);
        foreach (var adapter in _initSettingsAdapters)
        {
            var shopImportSettings = await adapter.GetAllShopImportSettings(cancellationToken);
            foreach (var kv in shopImportSettings)
            {
                if (!allShopImportSettings.ContainsKey(kv.Key))
                    allShopImportSettings.Add(kv.Key, kv.Value);
            }
        }

        return allShopImportSettings;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _stopAllServicesCommandHandler.Handle(new StopAllServicesCommand(), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error while requesting all services to stop: {Message}", ex.Message);
        }

        try
        {
            await _eventMessageReceiver.Stop(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error stopping event message receiver: {Message}", ex.Message);
        }

        try
        {
            await _eventMessageSender.Stop(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error stopping event message sender: {Message}", ex.Message);
        }

        try
        {
            await _ackEventMessageReceiver.Stop(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error stopping ack event message receiver: {Message}", ex.Message);
        }

        try
        {
            _shopCategorySemaphore.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error disposing semaphore: {Message}", ex.Message);
        }

        await base.StopAsync(cancellationToken);
    }
}