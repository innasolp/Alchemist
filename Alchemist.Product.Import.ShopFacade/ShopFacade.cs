using Alchemist.DataService.Interfaces;
using Alchemist.Product.Import.Model.Shop.Infrastructure;
using Alchemist.Product.Interfaces;

namespace Alchemist.Product.Import.Model.Shop;

internal class ShopFacade(IShopModelFactory modelFactory, IShopDataService shopDataService) : IShopFacade
{
    private readonly IShopModelFactory _modelFactory = modelFactory;

    private readonly Dictionary<Guid, IShopModel> _shops = [];

    private readonly IShopDataService _shopDataService = shopDataService;

    public IShopModel AddShop(IShop shop)
    {
        var newShop = _modelFactory.CreateShopModel(shop.Id);
        newShop.SetFrom(shop);

        _shops.Add(newShop.Guid, newShop);

        return newShop;
    }

    public IShopModel CreateDefaultShop()
    {
        return _modelFactory.CreateShopModel(0); 
    }

    public List<IShopModel> GetShops()
    {
        return _shops.Select(s => s.Value).ToList();
    }

    public async Task<List<IShopModel>> UploadShops()
    {
        var uploadedShops = await _shopDataService.GetShops();

        if (_shops.Count != 0)
        {
            uploadedShops.ToList().ForEach(s =>
            {
                var shopImport = _shops.FirstOrDefault(si => si.Value.Id == s.Id);
                if (shopImport.Value != null)
                    shopImport.Value.SetFrom(s);
                else
                    AddShop(s);
            });

            foreach (var deprecatedShop in _shops.Where(si => !uploadedShops.Any(s => s.Id == si.Value.Id)))
                deprecatedShop.Value.IsDeprecated = true;
        }
        else
            uploadedShops.ToList().ForEach(s => AddShop(s));

        return await Task.FromResult(_shops.Values.ToList());
    }

    public bool TryGetShop(Guid guid, out IShopModel shop)
    {
        return _shops.TryGetValue(guid, out shop) && shop != null;
    }

    public async Task SaveShop(IShopModel shop)
    {
        var shopEntity = new Entities.Shop
        {
            Id = shop.Id,
            Name = shop.Name,
            Caption = shop.Caption,
            Url = shop.Url
        };

        var savedShop = (shop.Id == 0)
          ? await _shopDataService.CreateShop(shopEntity) :
            await _shopDataService.UpdateShop(shopEntity);

        if (!_shops.TryGetValue(shop.Guid, out var existingShop))
            AddShop(savedShop);
        else
            shop.SetFrom(savedShop);
    }
}
