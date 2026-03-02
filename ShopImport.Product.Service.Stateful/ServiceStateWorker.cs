using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Products.Service;
using ShopImport.ServiceState;

namespace ShopImport.Product.Service.Stateful;

internal class ServiceStateWorker<TCategory>(IServiceStateRepository serviceStateRepository)
    where TCategory : class, ICategoryProducts, new()
{
    private readonly IServiceStateRepository _serviceStateRepository = serviceStateRepository;

    private byte[] _serviceStateKey = [];

    private ServiceState<TCategory> _serviceState = new();

    public CategoryState CategoryState => _serviceState.CategoryState;

    public TCategory? Category => _serviceState.Category;

    public void Initialize(byte[] serviceStateKey)
    {
        _serviceStateKey = serviceStateKey;
    }

    public async Task Load(CancellationToken cancellationToken)
    {
        _serviceState = await _serviceStateRepository.Load<ServiceState<TCategory>>(_serviceStateKey, cancellationToken) ?? new();
    }

    public async Task SetProductShopCategoryIfNeedAsync(IProductShopCategory category, CancellationToken cancellationToken)
    {
        _serviceState.Reset();
        _serviceState.Start(category);
        await _serviceStateRepository.Save(_serviceStateKey, _serviceState, cancellationToken);
    }

    public bool IsCategoryCurrent(string categoryPath, int page)
    {
        return _serviceState.Category != null && 
            _serviceState.CategoryState.CategoryPath == categoryPath && 
            _serviceState.CategoryState.Page == page;
    }

    public async Task SaveCategoryAsync(TCategory category, CancellationToken cancellationToken)
    {
        _serviceState.Category = category;
        await _serviceStateRepository.Save(_serviceStateKey, _serviceState, cancellationToken);
    }

    public bool IsCategoryProductItemCurrent(ICategoryProductItem categoryProductItem)
    {
        return _serviceState.CurrentCategoryProductItemId == categoryProductItem.Id
            || _serviceState.HandledCategoryProductItemIds.Any(p => p == categoryProductItem.Id);
    }

    public async Task SaveCurrentCategoryProductItemAsync(ICategoryProductItem categoryProductItem, CancellationToken cancellationToken)
    {
        if (_serviceState.CurrentCategoryProductItemId != null)
            _serviceState.HandledCategoryProductItemIds.Add(_serviceState.CurrentCategoryProductItemId);

        _serviceState.CurrentCategoryProductItemId = categoryProductItem.Id;

        await _serviceStateRepository.Save(_serviceStateKey, _serviceState, cancellationToken);
    }
}