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

        _eventMessageReceiver.On<Settings.ShopSettings>(Messages.Common.Messages.ShopSettingsCreated, OnShopSettingsCreatedAsync);

        _eventMessageReceiver.On<ShopCategory>(Messages.Common.Messages.CategoryAdded, OnShopCategoryAdded);

        _eventMessageReceiver.On<ServiceMessage>(Messages.Common.Messages.ServiceStarting, OnServiceStarting);
        _eventMessageReceiver.On<ServiceStartedMessage>(Messages.Common.Messages.ServiceStarted, OnServiceStarted);
        _eventMessageReceiver.On<Guid>(Messages.Common.Messages.ServiceStop, OnStopServiceAsync);
        _eventMessageReceiver.On<ServiceMessage>(Messages.Common.Messages.ServiceStopped, OnServiceStoppedAsync);
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
            await _mediator.Send(new StopServiceCommand(guid));
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
            await _mediator.Send(new StartServiceCommand(guid), stoppingToken);
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

            await _mediator.Send(new QueueShopCategoryToServicesCommand(shopCategory));
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

    private async Task OnShopSettingsCreatedAsync(Settings.ShopSettings newShopSettings)
    {
        _logger.LogInformation("Handling of settings {Name} for shop id={ShopId} started.", newShopSettings.Name, newShopSettings.ShopId);

        try
        {
            var guid = await _mediator.Send(new AddShopImportServiceFromShopSettingsCommand(newShopSettings));

            _logger.LogInformation("New service {Name} with id {Guid} added", newShopSettings.Name, guid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service creation for shop settings {Name} failed.", newShopSettings.Name);
        }
    }

    private async Task StartNewService(string name, IShopImportSettings shopImportSettings, CancellationToken cancellationToken)
    {
        try
        {
            var guid = await _mediator.Send(new AddImportServiceCommand(name, shopImportSettings), cancellationToken);

            _logger.LogInformation("Service {Name} is initialized.", name);

            await _mediator.Send(new StartServiceCommand(guid), cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Couldn't start the service {Name}.", name);
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
            await _mediator.Send(new StopAllServicesCommand(), cancellationToken);
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
            _shopCategorySemaphore.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error disposing semaphore: {Message}", ex.Message);
        }

        await base.StopAsync(cancellationToken);
    }
}