using Alchemist.Product.Entities;
using Grpc.Net.Client;
using Alchemist.Product.DataService.Interfaces;
using Alchemist.Product.GrpcService.Extensions;
using Alchemist.Product.GrpcService;
using Grpc.Core.Interceptors;
using Grpc.Client.Interceptors;

namespace Alchemist.Product.GrpcServiceClient;

public class AlchemyGrpcServiceClient : IProductDataService
{
    private readonly AlchemyGrpcService.AlchemyGrpcServiceClient _serviceClient;

    private readonly GrpcChannel _channel;

    private readonly IEnumerable<Interceptor> _interceptors;

    public AlchemyGrpcServiceClient(GrpcChannel channel):this(channel, [])
    {
    }

    public AlchemyGrpcServiceClient(GrpcChannel channel, IEnumerable<Interceptor> interceptors)
    {
        _channel = channel;
        _interceptors = interceptors;
        var invoker = _channel.Intercept(_interceptors.ToArray())
            .Intercept(new ClientExceptionErrorInfoInterceptor())
            .Intercept(new ClientNotFoundInterceptor());
        _serviceClient = new AlchemyGrpcService.AlchemyGrpcServiceClient(invoker);        
    }

    public async Task<Brand> CreateBrand(Brand brand)
    {
        var brandReply = await _serviceClient.CreateBrandAsync(new CreateBrandRequest { Name = brand.Name, Comment = brand.Comment, Countryid = brand.CountryId });
        return await Task.FromResult(new Brand
        {
            Id = brandReply.Id,
            Name = brandReply.Name,
            Comment = brandReply.Comment,
            CountryId = (short)brandReply.Countryid
        });
    }

    public async Task<Component> CreateComponent(Component component)
    {
        var componentReply = await _serviceClient.CreateComponentAsync(component.ToMessage<CreateComponentRequest>());
        var newComponent = componentReply.FromMessage<Component>();
        newComponent.Id = componentReply.Id;
        return await Task.FromResult(newComponent);
    }

    public async Task<Country> CreateCountry(Country country)
    {
        var countryReply = await _serviceClient.CreateCountryAsync(new CreateCountryRequest { Name = country.Name, Transcript = country.Transcript });
        return await Task.FromResult(new Country
        {
            Id = (short)countryReply.Id,
            Name = countryReply.Name,
            Transcript = countryReply.Transcript
        });
    }

    public async Task<Currency> CreateCurrency(Currency currency)
    {
        var currencyReply = await _serviceClient.CreateCurrencyAsync(currency.ToMessage<CreateCurrencyRequest>());
        var newCurrency = currencyReply.FromMessage<Currency>();
        newCurrency.Id = (short)currencyReply.Id;
        return await Task.FromResult(newCurrency);
    }

    public async Task<Entities.Product> CreateProduct(Entities.Product product)
    {
        var request = product.ToMessage<CreateProductRequest>();
        var productReply = await _serviceClient.CreateProductAsync(request);
        product = productReply.FromMessage<Entities.Product>();
        product.Id = productReply.Id;
        return await Task.FromResult(product);
    }

    public async Task<ProductType> CreateProductType(ProductType productType)
    {
        var productTypeReply = await _serviceClient.CreateProductTypeAsync(new CreateProductTypeRequest { Name = productType.Name });
        return await Task.FromResult(new ProductType
        {
            Id = (short)productTypeReply.Id,
            Name = productTypeReply.Name,
        });
    }

    public async Task<PurposeType> CreatePurposeType(PurposeType purposeType)
    {
        var productTypeReply = await _serviceClient.CreatePurposeTypeAsync(new CreatePurposeTypeRequest { Name = purposeType.Name });
        return await Task.FromResult(new PurposeType
        {
            Id = (short)productTypeReply.Id,
            Name = productTypeReply.Name,
        });
    }

    public async Task<ShopProduct> CreateShopProduct(ShopProduct shopProduct)
    {
        var request = shopProduct.ToMessage<CreateShopProductRequest>();
        var shopProductReply = await _serviceClient.CreateShopProductAsync(request);
        shopProduct = shopProductReply.FromMessage<ShopProduct>();
        shopProduct.Id = shopProductReply.Id;
        return await Task.FromResult(shopProduct);
    }

    public async Task<ShopProductPrice> CreateShopProductPrice(ShopProductPrice shopProductPrice)
    {
        var request = shopProductPrice.ToMessage<CreateShopProductPriceRequest>();
        var reply = await _serviceClient.CreateShopProductPriceAsync(request);
        shopProductPrice = reply.FromMessage<ShopProductPrice>();
        shopProductPrice.Id = reply.Id;
        return await Task.FromResult(shopProductPrice);
    }

    public async Task<Brand?> FindBrandByName(string name)
    {
        var request = new FindByNameRequest { Name = name };
        var reply = await _serviceClient.FindBrandByNameAsync(request);
        if (reply == null) return await Task.FromResult(default(Brand));
        var brand = new Brand { Id = reply.Id, Name = reply.Name, CountryId = (short)reply.Countryid, Comment = reply.Comment };
        return await Task.FromResult(brand);
    }

    public async Task<Component?> FindComponentByName(string name)
    {
        var request = new FindByNameRequest { Name = name };
        var reply = await _serviceClient.FindComponentByNameAsync(request);
        if (reply == null) return await Task.FromResult(default(Component));
        var component = reply.FromMessage<Component>();
        component.Id = reply.Id;
        return await Task.FromResult(component);
    }

    public async Task<Country?> FindCountryByName(string name)
    {
        var request = new FindByNameRequest { Name = name };
        var reply = await _serviceClient.FindCountryByNameAsync(request);
        if (reply == null) return await Task.FromResult(default(Country));
        return await Task.FromResult(new Country { Id = (short)reply.Id, Name = reply.Name });
    }

    public async Task<Entities.Product?> FindProductByName(string name)
    {
        var request = new FindByNameRequest { Name = name };
        var reply = await _serviceClient.FindProductByNameAsync(request);
        if (reply == null) return await Task.FromResult(default(Entities.Product));
        var product = reply.FromMessage<Entities.Product>();
        product.Id = reply.Id;
        return await Task.FromResult(product);
    }

    public async Task<Entities.Product?> FindProductByNameAndBrand(string name, string brand)
    {
        var request = new FindProductByNameAndBrandRequest { Name = name, Brand = brand };
        var reply = await _serviceClient.FindProductByNameAndBrandAsync(request);
        if (reply == null) return await Task.FromResult(default(Entities.Product));
        var product = reply.FromMessage<Entities.Product>();
        product.Id = reply.Id;
        return await Task.FromResult(product);
    }

    public async Task<ProductType?> FindProductTypeByName(string name)
    {
        var request = new FindByNameRequest { Name = name };
        var reply = await _serviceClient.FindProductTypeByNameAsync(request);
        if (reply == null) return await Task.FromResult(default(ProductType));
        return await Task.FromResult(new ProductType { Id = (short)reply.Id, Name = reply.Name });
    }

    public async Task<PurposeType?> FindPurposeTypeByName(string name)
    {
        var request = new FindByNameRequest { Name = name };
        var reply = await _serviceClient.FindPurposeTypeByNameAsync(request);
        if (reply == null) return await Task.FromResult(default(PurposeType));
        return await Task.FromResult(new PurposeType { Id = (short)reply.Id, Name = reply.Name });
    }

    public async Task<Currency?> GetCurrencyByCode(short code)
    {
        var request = new GetCurrencyByCodeRequest { Code = code };
        var reply = await _serviceClient.GetCurrencyByCodeAsync(request);
        if (reply == null) return await Task.FromResult(default(Currency));
        var currency = reply.FromMessage<Currency>();
        currency.Id = (short)reply.Id;
        return await Task.FromResult(currency);
    }

    public async Task<Currency?> GetCurrencyByName(string name)
    {
        var request = new FindByNameRequest { Name = name };
        var reply = await _serviceClient.GetCurrencyByNameAsync(request);
        if (reply == null) return await Task.FromResult(default(Currency));
        var currency = reply.FromMessage<Currency>();
        currency.Id = (short)reply.Id;
        return await Task.FromResult(currency);
    }    

    public async Task<ShopProduct?> GetShopProductByShopAndApiUrl(int shopId, string apiUrl)
    {
        var request = new GetShopProductByShopAndApiUrlRequest { Apiurl = apiUrl, Shopid = shopId };

        var reply = await _serviceClient.GetShopProductByShopAndApiUrlAsync(request);
        if (reply == null) return await Task.FromResult(default(ShopProduct));
        var shopProduct = reply.FromMessage<ShopProduct>();
        shopProduct.Id = reply.Id;
        return await Task.FromResult(shopProduct);
    }
    
    public async Task<ShopProduct?> GetShopProductByShopAndItemId(int shopId, string itemId)
    {
        var request = new GetShopProductByShopAndItemIdRequest { Itemid = itemId, Shopid = shopId };

        var reply = await _serviceClient.GetShopProductByShopAndItemIdAsync(request);
        if (reply == null) return await Task.FromResult(default(ShopProduct));
        var shopProduct = reply.FromMessage<ShopProduct>();
        shopProduct.Id = reply.Id;
        return await Task.FromResult(shopProduct);
    }

    public async Task<ShopProductPrice?> GetShopProductPrice(long shopProductId)
    {
        var request = new GetByIdInt64Request { Id = shopProductId };

        var reply = await _serviceClient.GetShopProductPriceAsync(request);
        if (reply == null) return await Task.FromResult(default(ShopProductPrice));
        var shopProduct = reply.FromMessage<ShopProductPrice>();
        shopProduct.Id = reply.Id;
        return await Task.FromResult(shopProduct);
    }

    public async Task<ProductComponent> SetProductComponent(ProductComponent productComponent)
    {
        var request = productComponent.ToMessage<SetProductComponentRequest>();
        var reply = await _serviceClient.SetProductComponentAsync(request);
        var newProductComponent = reply.FromMessage<ProductComponent>();
        return await Task.FromResult(newProductComponent);
    }

    public async Task<bool> UpdateShopProduct(ShopProduct shopProduct)
    {
        var request = shopProduct.ToMessage<UpdateShopProductRequest>();
        request.Id = shopProduct.Id;
        var result = await _serviceClient.UpdateShopProductAsync(request);
        return await Task.FromResult(result.Value);
    }

    public async Task<bool> UpdateShopProductPrice(ShopProductPrice shopProductPrice)
    {
        var request = shopProductPrice.ToMessage<UpdateShopProductPriceRequest>();
        request.Id = shopProductPrice.Id;
        var result = await _serviceClient.UpdateShopProductPriceAsync(request);
        return await Task.FromResult(result.Value);
    }
}
