using Alchemist.Product.Entities;
using Grpc.Net.Client;
using Alchemist.Product.GrpcService;
using Grpc.Core.Interceptors;
using Grpc.Client.Interceptors;
using Alchemist.Product.Interfaces;
using Grpc.Message.Extensions;
using Mapster;

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
        _interceptors = interceptors ?? [];

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
        return brandReply.Adapt<Brand>();
    }

    public async Task<IComponent> CreateComponent(IComponent component, CancellationToken cancellationToken = default)
    {
        var componentReply = await _serviceClient.CreateComponentAsync(component.Adapt<CreateComponentRequest>(), cancellationToken: cancellationToken).ConfigureAwait(false);
        return componentReply.Adapt<Component>();
    }

    public async Task<ICountry> CreateCountry(ICountry country, CancellationToken cancellationToken = default)
    {
        var countryReply = await _serviceClient.CreateCountryAsync(
            new CreateCountryRequest { Name = country.Name, Transcript = country.Transcript }, 
            cancellationToken: cancellationToken).ConfigureAwait(false);

        return countryReply.Adapt<Country>(); 
    }

    public async Task<ICurrency> CreateCurrency(ICurrency currency, CancellationToken cancellationToken = default)
    {
        var currencyReply = await _serviceClient.CreateCurrencyAsync(currency.Adapt<CreateCurrencyRequest>(), cancellationToken: cancellationToken).ConfigureAwait(false);
        return currencyReply.Adapt<Currency>();
    }

    public async Task<IProduct> CreateProduct(IProduct product, CancellationToken cancellationToken = default)
    {
        var request = product.Adapt<CreateProductRequest>();
        var productReply = await _serviceClient.CreateProductAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        return productReply.Adapt<Entities.Product>();
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
        var request = shopProduct.Adapt<CreateShopProductRequest>();
        var shopProductReply = await _serviceClient.CreateShopProductAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        return shopProductReply.Adapt<ShopProduct>();
    }

    public async Task<IShopProductPrice> CreateShopProductPrice(IShopProductPrice shopProductPrice, CancellationToken cancellationToken = default)
    {
        var request = shopProductPrice.Adapt<CreateShopProductPriceRequest>();
        var reply = await _serviceClient.CreateShopProductPriceAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        return reply.Adapt<ShopProductPrice>();
    }

    public async Task<List<IShopProductCategory>> GetShopProductCategories(long shopProductId, CancellationToken cancellationToken = default)
    {
        var request = new GetByIdInt64Request { Id = shopProductId };
        var reply = await _serviceClient.GetShopProductCategoriesAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (reply == null) return await Task.FromResult(default(List<IShopProductCategory>));
        return await reply.FromListReply<ShopProductCategoryListReply, ShopProductCategoryReply, IShopProductCategory>((s) =>
        {
            var entity = s.Adapt<ShopProductCategory>();
            entity.Id = s.Id;
            return entity;
        });
    }

    public async Task<IShopProductCategory> AddShopProductCategory(IShopProductCategory shopProductCategory, CancellationToken cancellationToken = default)
    {
        var request = shopProductCategory.Adapt<ShopProductCategoryRequest>();
        var reply = await _serviceClient.AddShopProductCategoryAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        return reply.Adapt<ShopProductCategory>();
    }

    public async Task<IShopProductCategory> AddShopProductCategory(long shopProductId, int shopCategoryId, CancellationToken cancellationToken = default)
    {
        var request = new ShopProductCategoryRequest { Shopcategoryid = shopCategoryId, Shopproductid = shopProductId };
        var reply = await _serviceClient.AddShopProductCategoryAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        return reply.Adapt<ShopProductCategory>();
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
        return reply?.Adapt<Brand>();
    }

    public async Task<IComponent?> FindComponentByName(string name, CancellationToken cancellationToken = default)
    {
        var request = new FindByNameRequest { Name = name };
        var reply = await _serviceClient.FindComponentByNameAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        return reply?.Adapt<Component>();
    }

    public async Task<ICountry?> FindCountryByName(string name, CancellationToken cancellationToken = default)
    {
        var request = new FindByNameRequest { Name = name };
        var reply = await _serviceClient.FindCountryByNameAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        return reply?.Adapt<Country>();
    }

    public async Task<IProduct?> FindProductByName(string name, CancellationToken cancellationToken = default)
    {
        var request = new FindByNameRequest { Name = name };
        var reply = await _serviceClient.FindProductByNameAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        return reply?.Adapt<Entities.Product>();
    }

    public async Task<IProduct?> FindProductByNameAndBrand(string name, string brand, CancellationToken cancellationToken = default)
    {
        var request = new FindProductByNameAndBrandRequest { Name = name, Brand = brand };
        var reply = await _serviceClient.FindProductByNameAndBrandAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        return reply?.Adapt<Entities.Product>();
    }

    public async Task<IProductType?> FindProductTypeByName(string name, CancellationToken cancellationToken = default)
    {
        var request = new FindByNameRequest { Name = name };
        var reply = await _serviceClient.FindProductTypeByNameAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        return reply?.Adapt<ProductType>();
    }

    public async Task<IPurposeType?> FindPurposeTypeByName(string name, CancellationToken cancellationToken = default)
    {
        var request = new FindByNameRequest { Name = name };
        var reply = await _serviceClient.FindPurposeTypeByNameAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        return reply?.Adapt<PurposeType>();
    }

    public async Task<ICurrency?> GetCurrencyByCode(short code, CancellationToken cancellationToken = default)
    {
        var request = new GetCurrencyByCodeRequest { Code = code };
        var reply = await _serviceClient.GetCurrencyByCodeAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        return reply?.Adapt<Currency>();
    }

    public async Task<ICurrency?> GetCurrencyByName(string name, CancellationToken cancellationToken = default)
    {
        var request = new FindByNameRequest { Name = name };
        var reply = await _serviceClient.GetCurrencyByNameAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        return reply?.Adapt<Currency>();
    }

    public async Task<IShopProduct?> GetShopProductByShopIdAndApiUrl(int shopId, string apiUrl, CancellationToken cancellationToken = default)
    {
        var request = new GetShopProductByShopIdAndApiUrlRequest { Apiurl = apiUrl, Shopid = shopId };

        var reply = await _serviceClient.GetShopProductByShopIdAndApiUrlAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        return reply?.Adapt<ShopProduct>();
    }

    public async Task<IShopProduct?> GetShopProductByShopIdAndItemId(int shopId, string itemId, CancellationToken cancellationToken = default)
    {
        var request = new GetShopProductByShopIdAndItemIdRequest { Itemid = itemId, Shopid = shopId };

        var reply = await _serviceClient.GetShopProductByShopIdAndItemIdAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        return reply?.Adapt<ShopProduct>();
    }

    public async Task<IShopProduct?> GetShopProductByShopIdAndProductId(int shopId, long productId, CancellationToken cancellationToken = default)
    {
        var request = new GetShopProductByShopIdAndProductIdRequest { Productid = productId, Shopid = shopId };

        var reply = await _serviceClient.GetShopProductByShopIdAndProductIdAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        return reply?.Adapt<ShopProduct>();
    }

    public async Task<IShopProductPrice?> GetShopProductPrice(long shopProductId, CancellationToken cancellationToken = default)
    {
        var request = new GetByIdInt64Request { Id = shopProductId };

        var reply = await _serviceClient.GetShopProductPriceAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        return reply?.Adapt<ShopProductPrice>();
    }

    public async Task<IProductComponent> SetProductComponent(IProductComponent productComponent, CancellationToken cancellationToken = default)
    {
        var request = productComponent.Adapt<SetProductComponentRequest>();
        var reply = await _serviceClient.SetProductComponentAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        return reply.Adapt<ProductComponent>();
    }

    public async Task<IShopProduct> UpdateShopProduct(IShopProduct shopProduct, CancellationToken cancellationToken = default)
    {
        var request = shopProduct.Adapt<UpdateShopProductRequest>();
        request.Id = shopProduct.Id;
        var reply = await _serviceClient.UpdateShopProductAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        return reply.Adapt<ShopProduct>();
    }

    public async Task<IShopProductPrice> UpdateShopProductPrice(IShopProductPrice shopProductPrice, CancellationToken cancellationToken = default)
    {
        var request = shopProductPrice.Adapt<UpdateShopProductPriceRequest>();
        request.Id = shopProductPrice.Id;
        var reply = await _serviceClient.UpdateShopProductPriceAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        return reply.Adapt<ShopProductPrice>();
    }

    public async Task<List<IPurposeType>> GetProductPurposes(long productId, CancellationToken cancellationToken = default)
    {
        var request = new GetProductPurposesRequest { Productid = productId };
        var reply = await _serviceClient.GetProductPurposesAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        return await reply.FromListReply<PurposeTypeListReply, PurposeTypeReply, IPurposeType>((s) =>s.Adapt<PurposeType>());
    }

    public async Task<IProductPurpose> SetProductPurpose(IProductPurpose productPurpose, CancellationToken cancellationToken = default)
    {
        var request = new SetProductPurposeRequest { Productid = productPurpose.ProductId, Purposetypeid = productPurpose.PurposeTypeId };
        var reply = await _serviceClient.SetProductPurposeAsync(request, cancellationToken: cancellationToken);
        return reply.Adapt<ProductPurpose>();
    }
}