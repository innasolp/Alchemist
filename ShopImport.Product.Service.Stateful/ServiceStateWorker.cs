using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Products.Service;
using ShopImport.ServiceState;

namespace ShopImport.Product.Service.Stateful;

internal class ServiceStateWorker<TCategory>(IServiceStateRepository serviceStateRepository) : IAsyncDisposable
    where TCategory : class, ICategoryProducts, new()
{
    private readonly IServiceStateRepository _serviceStateRepository = serviceStateRepository;

    private byte[] _serviceStateKey = [];

    private ServiceState<TCategory> _serviceState = new();

    public CategoryProcessState CategoryProcessState => _serviceState.CategoryProcessState;

    public TCategory? Category => _serviceState.Category;

    public IProductShopCategory? ProductShopCategory => _serviceState.ProductShopCategory;

    public async Task Start(byte[] serviceStateKey, CancellationToken cancellationToken)
    {
        _serviceStateKey = serviceStateKey;

        if (!_serviceStateRepository.IsConnected)
            await _serviceStateRepository.Connect(cancellationToken);

        _serviceState = await _serviceStateRepository.Load<ServiceState<TCategory>>(_serviceStateKey, cancellationToken) ?? new();
    }

    public Task ResetAsync(CancellationToken cancellationToken)
    {
        _serviceState.Reset();
        return _serviceStateRepository.Save(_serviceStateKey, _serviceState, cancellationToken);
    }

    public Task ResetLoadedCategoryPageAsync(CancellationToken cancellationToken)
    {
        _serviceState.Category = null;
        return _serviceStateRepository.Save(_serviceStateKey, _serviceState, cancellationToken);
    }

    public Task SetProductShopCategoryIfNeedAsync(IProductShopCategory category, CancellationToken cancellationToken)
    {
        if (_serviceState.ProductShopCategory?.Path == category.Path)
            return Task.CompletedTask;

        _serviceState.Reset();
        _serviceState.Start(category);
        return _serviceStateRepository.Save(_serviceStateKey, _serviceState, cancellationToken);
    }

    public bool IsCategoryCurrent(string categoryPath, int page)
    {
        return _serviceState.Category != null && 
            _serviceState.CategoryProcessState.CategoryPath == categoryPath && 
            _serviceState.CategoryProcessState.Page == page;
    }

    public async Task SaveCategoryAsync(TCategory category, CancellationToken cancellationToken)
    {
        _serviceState.Category = category;
        await _serviceStateRepository.Save(_serviceStateKey, _serviceState, cancellationToken);
    }

    public bool IsCategoryProductItemHandled(ICategoryProductItem categoryProductItem)
    {
        return _serviceState.HandledCategoryProductItemIds.Any(p => p == categoryProductItem.Id);
    }

    public async Task SaveHandledCategoryProductItemAsync(ICategoryProductItem categoryProductItem, CancellationToken cancellationToken)
    {
        if (_serviceState.CurrentCategoryProductItemId != null)
            _serviceState.HandledCategoryProductItemIds.Add(_serviceState.CurrentCategoryProductItemId);

        _serviceState.CurrentCategoryProductItemId = null;

        await _serviceStateRepository.Save(_serviceStateKey, _serviceState, cancellationToken);
    }

    public async Task SetCurrentCategoryProductItemAsync(ICategoryProductItem categoryProductItem, CancellationToken cancellationToken)
    {
        _serviceState.CurrentCategoryProductItemId = categoryProductItem.Id;

        await _serviceStateRepository.Save(_serviceStateKey, _serviceState, cancellationToken);
    }

    public async Task RemoveAsync(CancellationToken cancellationToken = default)
    {
        await _serviceStateRepository.Remove(_serviceStateKey, cancellationToken);        
    }

    public async Task CloseAsync(CancellationToken cancellationToken = default)
    {        
        await _serviceStateRepository.Close(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await CloseAsync();

        await _serviceStateRepository.DisposeAsync();
    }
}