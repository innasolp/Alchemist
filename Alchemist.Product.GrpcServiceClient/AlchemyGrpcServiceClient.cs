using Alchemist.Product.Entities;
using Grpc.Net.Client;
using Alchemist.Product.GrpcService.Extensions;
using Alchemist.Product.GrpcService;
using Grpc.Core.Interceptors;
using Grpc.Client.Interceptors;
using Alchemist.Product.Interfaces;
using Alchemist.DataService.Interfaces;
using Grpc.Message.Extensions;

namespace Alchemist.Product.GrpcServiceClient;

public class AlchemyGrpcServiceClient : IProductDataService
{
    private readonly AlchemyGrpcService.AlchemyGrpcServiceClient _serviceClient;

    private readonly GrpcChannel _channel;

    private readonly IEnumerable<Interceptor> _interceptors;

    public AlchemyGrpcServiceClient(GrpcChannel channel) : this(channel, Array.Empty<Interceptor>())
    {
    }

    public AlchemyGrpcServiceClient(GrpcChannel channel, IEnumerable<Interceptor> interceptors)
    {
        _channel = channel;
        _interceptors = interceptors ?? Array.Empty<Interceptor>();

        var invoker = _channel.CreateCallInvoker();
        foreach (var interceptor in _interceptors)
        {
            if (interceptor is not null)
            {
                invoker = invoker.Intercept(interceptor);
            }
        }

        invoker = invoker
            .Intercept(new ClientNotFoundInterceptor())
            .Intercept(new ClientExceptionErrorInfoInterceptor());

        _serviceClient = new AlchemyGrpcService.AlchemyGrpcServiceClient(invoker);
    }

    public async Task<IBrand> CreateBrand(IBrand brand, CancellationToken cancellationToken = default)
    {
        using var call = _serviceClient.CreateBrandAsync(
            new CreateBrandRequest { Name = brand.Name, Comment = brand.Comment, Countryid = brand.CountryId },
            cancellationToken: cancellationToken);
        var headers = await call.ResponseHeadersAsync.ConfigureAwait(false);

        var brandReply = await call.ResponseAsync.ConfigureAwait(false);
        return await Task.FromResult(new Brand
        {
            Id = brandReply.Id,
            Name = brandReply.Name,
            Comment = brandReply.Comment,
            CountryId = (short?)brandReply.Countryid
        });
    }

    public async Task<IComponent> CreateComponent(IComponent component, CancellationToken cancellationToken = default)
    {
        var componentReply = await _serviceClient.CreateComponentAsync(component.ToMessage<CreateComponentRequest>(), cancellationToken: cancellationToken).ConfigureAwait(false);
        var newComponent = componentReply.FromMessage<Component>();
        newComponent.Id = componentReply.Id;
        return await Task.FromResult(newComponent);
    }

    public async Task<ICountry> CreateCountry(ICountry country, CancellationToken cancellationToken = default)
    {
        var countryReply = await _serviceClient.CreateCountryAsync(new CreateCountryRequest { Name = country.Name, Transcript = country.Transcript }, cancellationToken: cancellationToken).ConfigureAwait(false);
        return await Task.FromResult(new Country
        {
            Id = (short)countryReply.Id,
            Name = countryReply.Name,
            Transcript = countryReply.Transcript
        });
    }

    public async Task<ICurrency> CreateCurrency(ICurrency currency, CancellationToken cancellationToken = default)
    {
        var currencyReply = await _serviceClient.CreateCurrencyAsync(currency.ToMessage<CreateCurrencyRequest>(), cancellationToken: cancellationToken).ConfigureAwait(false);
        var newCurrency = currencyReply.FromMessage<Currency>();
        newCurrency.Id = (short)currencyReply.Id;
        return await Task.FromResult(newCurrency);
    }

    public async Task<IProduct> CreateProduct(IProduct product, CancellationToken cancellationToken = default)
    {
        var request = product.ToMessage<CreateProductRequest>();
        var productReply = await _serviceClient.CreateProductAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        product = productReply.FromMessage<Entities.Product>();
        product.Id = productReply.Id;
        return await Task.FromResult(product);
    }

    public async Task<IProductType> CreateProductType(IProductType productType, CancellationToken cancellationToken = default)
    {
        var productTypeReply = await _serviceClient.CreateProductTypeAsync(new CreateProductTypeRequest { Name = productType.Name }, cancellationToken: cancellationToken).ConfigureAwait(false);
        return await Task.FromResult(new ProductType
        {
            Id = (short)productTypeReply.Id,
            Name = productTypeReply.Name,
        });
    }

    public async Task<IPurposeType> CreatePurposeType(IPurposeType purposeType, CancellationToken cancellationToken = default)
    {
        var productTypeReply = await _serviceClient.CreatePurposeTypeAsync(new CreatePurposeTypeRequest { Name = purposeType.Name }, cancellationToken: cancellationToken).ConfigureAwait(false);
        return await Task.FromResult(new PurposeType
        {
            Id = (short)productTypeReply.Id,
            Name = productTypeReply.Name,
        });
    }

    public async Task<IShopProduct> CreateShopProduct(IShopProduct shopProduct, CancellationToken cancellationToken = default)
    {
        var request = shopProduct.ToMessage<CreateShopProductRequest>();
        var shopProductReply = await _serviceClient.CreateShopProductAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        shopProduct = shopProductReply.FromMessage<ShopProduct>();
        shopProduct.Id = shopProductReply.Id;
        return await Task.FromResult(shopProduct);
    }

    public async Task<IShopProductPrice> CreateShopProductPrice(IShopProductPrice shopProductPrice, CancellationToken cancellationToken = default)
    {
        var request = shopProductPrice.ToMessage<CreateShopProductPriceRequest>();
        var reply = await _serviceClient.CreateShopProductPriceAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        shopProductPrice = reply.FromMessage<ShopProductPrice>();
        shopProductPrice.Id = reply.Id;
        return await Task.FromResult(shopProductPrice);
    }

    public async Task<List<IShopProductCategory>> GetShopProductCategories(long shopProductId, CancellationToken cancellationToken = default)
    {
        var request = new GetByIdInt64Request { Id = shopProductId };
        var reply = await _serviceClient.GetShopProductCategoriesAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (reply == null) return await Task.FromResult(default(List<IShopProductCategory>));
        return await reply.FromListReply<ShopProductCategoryListReply, ShopProductCategoryReply, IShopProductCategory>((s) =>
        {
            var entity = s.FromMessage<ShopProductCategory>();
            entity.Id = s.Id;
            return entity;
        });
    }

    public async Task<IShopProductCategory> AddShopProductCategory(IShopProductCategory shopProductCategory, CancellationToken cancellationToken = default)
    {
        var request = shopProductCategory.ToMessage<ShopProductCategoryRequest>();
        var reply = await _serviceClient.AddShopProductCategoryAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        shopProductCategory = reply.FromMessage<ShopProductCategory>();
        shopProductCategory.Id = reply.Id;
        return await Task.FromResult(shopProductCategory);
    }

    public async Task<IShopProductCategory> AddShopProductCategory(long shopProductId, int shopCategoryId, CancellationToken cancellationToken = default)
    {
        var request = new ShopProductCategoryRequest { Shopcategoryid = shopCategoryId, Shopproductid = shopProductId };
        var reply = await _serviceClient.AddShopProductCategoryAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        var shopProductCategory = reply.FromMessage<ShopProductCategory>();
        shopProductCategory.Id = reply.Id;
        return await Task.FromResult(shopProductCategory);
    }

    public async Task<bool> CheckShopProductCategory(long shopProductId, int shopCategoryId, CancellationToken cancellationToken = default)
    {
        var request = new ShopProductCategoryRequest { Shopcategoryid = shopCategoryId, Shopproductid = shopProductId };
        var reply = await _serviceClient.CheckShopProductCategoryAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        return await Task.FromResult(reply.Value);
    }

    public async Task<IBrand?> FindBrandByName(string name, CancellationToken cancellationToken = default)
    {
        var request = new FindByNameRequest { Name = name };
        var reply = await _serviceClient.FindBrandByNameAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (reply == null) return await Task.FromResult(default(Brand));
        var brand = new Brand { Id = reply.Id, Name = reply.Name, CountryId = (short?)reply.Countryid, Comment = reply.Comment };
        return await Task.FromResult(brand);
    }

    public async Task<IComponent?> FindComponentByName(string name, CancellationToken cancellationToken = default)
    {
        var request = new FindByNameRequest { Name = name };
        var reply = await _serviceClient.FindComponentByNameAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (reply == null) return await Task.FromResult(default(Component));
        var component = reply.FromMessage<Component>();
        component.Id = reply.Id;
        return await Task.FromResult(component);
    }

    public async Task<ICountry?> FindCountryByName(string name, CancellationToken cancellationToken = default)
    {
        var request = new FindByNameRequest { Name = name };
        var reply = await _serviceClient.FindCountryByNameAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (reply == null) return await Task.FromResult(default(Country));
        return await Task.FromResult(new Country { Id = (short)reply.Id, Name = reply.Name });
    }

    public async Task<IProduct?> FindProductByName(string name, CancellationToken cancellationToken = default)
    {
        var request = new FindByNameRequest { Name = name };
        var reply = await _serviceClient.FindProductByNameAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (reply == null) return await Task.FromResult(default(Entities.Product));
        var product = reply.FromMessage<Entities.Product>();
        product.Id = reply.Id;
        return await Task.FromResult(product);
    }

    public async Task<IProduct?> FindProductByNameAndBrand(string name, string brand, CancellationToken cancellationToken = default)
    {
        var request = new FindProductByNameAndBrandRequest { Name = name, Brand = brand };
        var reply = await _serviceClient.FindProductByNameAndBrandAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (reply == null) return await Task.FromResult(default(Entities.Product));
        var product = reply.FromMessage<Entities.Product>();
        product.Id = reply.Id;
        return await Task.FromResult(product);
    }

    public async Task<IProductType?> FindProductTypeByName(string name, CancellationToken cancellationToken = default)
    {
        var request = new FindByNameRequest { Name = name };
        var reply = await _serviceClient.FindProductTypeByNameAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (reply == null) return await Task.FromResult(default(ProductType));
        return await Task.FromResult(new ProductType { Id = (short)reply.Id, Name = reply.Name });
    }

    public async Task<IPurposeType?> FindPurposeTypeByName(string name, CancellationToken cancellationToken = default)
    {
        var request = new FindByNameRequest { Name = name };
        var reply = await _serviceClient.FindPurposeTypeByNameAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (reply == null) return await Task.FromResult(default(PurposeType));
        return await Task.FromResult(new PurposeType { Id = (short)reply.Id, Name = reply.Name });
    }

    public async Task<ICurrency?> GetCurrencyByCode(short code, CancellationToken cancellationToken = default)
    {
        var request = new GetCurrencyByCodeRequest { Code = code };
        var reply = await _serviceClient.GetCurrencyByCodeAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (reply == null) return await Task.FromResult(default(Currency));
        var currency = reply.FromMessage<Currency>();
        currency.Id = (short)reply.Id;
        return await Task.FromResult(currency);
    }

    public async Task<ICurrency?> GetCurrencyByName(string name, CancellationToken cancellationToken = default)
    {
        var request = new FindByNameRequest { Name = name };
        var reply = await _serviceClient.GetCurrencyByNameAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (reply == null) return await Task.FromResult(default(Currency));
        var currency = reply.FromMessage<Currency>();
        currency.Id = (short)reply.Id;
        return await Task.FromResult(currency);
    }

    public async Task<IShopProduct?> GetShopProductByShopAndApiUrl(int shopId, string apiUrl, CancellationToken cancellationToken = default)
    {
        var request = new GetShopProductByShopAndApiUrlRequest { Apiurl = apiUrl, Shopid = shopId };

        var reply = await _serviceClient.GetShopProductByShopAndApiUrlAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (reply == null) return await Task.FromResult(default(ShopProduct));
        var shopProduct = reply.FromMessage<ShopProduct>();
        shopProduct.Id = reply.Id;
        return await Task.FromResult(shopProduct);
    }

    public async Task<IShopProduct?> GetShopProductByShopAndItemId(int shopId, string itemId, CancellationToken cancellationToken = default)
    {
        var request = new GetShopProductByShopAndItemIdRequest { Itemid = itemId, Shopid = shopId };

        var reply = await _serviceClient.GetShopProductByShopAndItemIdAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (reply == null) return await Task.FromResult(default(ShopProduct));
        var shopProduct = reply.FromMessage<ShopProduct>();
        shopProduct.Id = reply.Id;
        return await Task.FromResult(shopProduct);
    }

    public async Task<IShopProduct?> GetShopProductByShopAndProductId(int shopId, long productId, CancellationToken cancellationToken = default)
    {
        var request = new GetShopProductByShopAndProductIdRequest { Productid = productId, Shopid = shopId };

        var reply = await _serviceClient.GetShopProductByShopAndProductIdAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (reply == null) return await Task.FromResult(default(ShopProduct));
        var shopProduct = reply.FromMessage<ShopProduct>();
        shopProduct.Id = reply.Id;
        return await Task.FromResult(shopProduct);
    }

    public async Task<IShopProductPrice?> GetShopProductPrice(long shopProductId, CancellationToken cancellationToken = default)
    {
        var request = new GetByIdInt64Request { Id = shopProductId };

        var reply = await _serviceClient.GetShopProductPriceAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (reply == null) return await Task.FromResult(default(ShopProductPrice));
        var shopProduct = reply.FromMessage<ShopProductPrice>();
        shopProduct.Id = reply.Id;
        return await Task.FromResult(shopProduct);
    }

    public async Task<IProductComponent> SetProductComponent(IProductComponent productComponent, CancellationToken cancellationToken = default)
    {
        var request = productComponent.ToMessage<SetProductComponentRequest>();
        var reply = await _serviceClient.SetProductComponentAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        var newProductComponent = reply.FromMessage<ProductComponent>();
        return await Task.FromResult(newProductComponent);
    }

    public async Task<bool> UpdateShopProduct(IShopProduct shopProduct, CancellationToken cancellationToken = default)
    {
        var request = shopProduct.ToMessage<UpdateShopProductRequest>();
        request.Id = shopProduct.Id;
        var result = await _serviceClient.UpdateShopProductAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        return await Task.FromResult(result.Value);
    }

    public async Task<bool> UpdateShopProductPrice(IShopProductPrice shopProductPrice, CancellationToken cancellationToken = default)
    {
        var request = shopProductPrice.ToMessage<UpdateShopProductPriceRequest>();
        request.Id = shopProductPrice.Id;
        var result = await _serviceClient.UpdateShopProductPriceAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        return await Task.FromResult(result.Value);
    }

    public async Task<List<IPurposeType>> GetProductPurposes(long productId, CancellationToken cancellationToken = default)
    {
        var request = new GetProductPurposesRequest { Productid = productId };
        var reply = await _serviceClient.GetProductPurposesAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        return await reply.FromListReply<PurposeTypeListReply, PurposeTypeReply, IPurposeType>(
            (s) =>new PurposeType { Id = (short)s.Id, Name = s.Name  });
    }

    public async Task<IProductPurpose> SetProductPurpose(IProductPurpose productPurpose, CancellationToken cancellationToken = default)
    {
        var request = new SetProductPurposeRequest { Productid = productPurpose.ProductId, Purposetypeid = productPurpose.PurposeTypeId };
        var reply = await _serviceClient.SetProductPurposeAsync(request, cancellationToken: cancellationToken);
        return new ProductPurpose { ProductId = reply.Productid, PurposeTypeId = (short)reply.Purposetypeid };
    }
}