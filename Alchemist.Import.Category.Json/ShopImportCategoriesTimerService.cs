using Alchemist.Import.Category.Interfaces;
using Import.Interfaces;
using Microsoft.Extensions.Logging;
using ShopImport.Category.Loader.Interfaces;

namespace Alchemist.Import.Category.Service;

public class ShopImportCategoriesTimerService : ShopImportCategoriesService
{
    private readonly int _defaultInterval = 3600;

    private readonly PeriodicTimer _timer;

    public ShopImportCategoriesTimerService(ILogger logger, 
        string name,
        ILoaderService loader, 
        string categorySourceUrl, 
        string sourceName, 
        string url, 
        IEnumerable<ICategoryLoader> categoryLoadStages, 
        ICategoryItemHandler itemHandler,
        int? timerSecondsInterval = null, 
        object? categoryLoadData = null) 
        : base(logger, name, loader, categorySourceUrl, sourceName, url, categoryLoadStages, itemHandler, categoryLoadData)
    {
        _timer = new(TimeSpan.FromSeconds(timerSecondsInterval ?? _defaultInterval));
    }

    protected override void Dispose()
    {
        _timer.Dispose();
        base.Dispose();
    }

    protected override async Task ProcessAsync(CancellationToken stoppingToken)
    {
        await base.ProcessAsync(stoppingToken);

        var categoryLoadData = GetLoadData();
        while ((await _timer.WaitForNextTickAsync(stoppingToken).AsTask()) && !stoppingToken.IsCancellationRequested)
        {
            await LoadCategoriesAsync(categoryLoadData, stoppingToken);
        }
    }
}