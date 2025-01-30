using Alchemist.Product.DataService.Interfaces;
using Alchemist.Product.Entities;
using Alchemist.Product.GrpcService.Extensions;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Alchemist.Product.GrpcService.Services;

public class AlchemyService(IAlchemyRepository db) : AlchemyGrpcService.AlchemyGrpcServiceBase
{
    readonly IAlchemyRepository _repository = db;
    public override async Task<ProductTypeReply> CreateProductType(CreateProductTypeRequest request, ServerCallContext context)
    {
        var productType = new ProductType { Name = request.Name };
        var newProductType = await _repository.CreateProductType(productType);       
        var reply = new ProductTypeReply() { Id = newProductType.Id, Name = newProductType.Name };
        return await Task.FromResult(reply);
    }

    public override async Task<PurposeTypeReply> CreatePurposeType(CreatePurposeTypeRequest request, ServerCallContext context)
    {
        var purposeType = new PurposeType { Name = request.Name };
        var newPurposeType = await _repository.CreatePurposeType(purposeType);
        var reply = new PurposeTypeReply() { Id = newPurposeType.Id, Name = newPurposeType.Name };
        return await Task.FromResult(reply);
    }

    public override async Task<CountryReply> CreateCountry(CreateCountryRequest request, ServerCallContext context)
    {
        var country = new Country { Name = request.Name };
        var newCountry = await _repository.CreateCountry(country);
        var reply = new CountryReply() { Id = newCountry.Id, Name = newCountry.Name };
        return await Task.FromResult(reply);
    }

    public override async Task<BrandReply> CreateBrand(CreateBrandRequest request, ServerCallContext context)
    {
        var brand = new Brand { Name = request.Name, CountryId = (short)request.Countryid, Comment = request.Comment };
        var entity = await _repository.CreateBrand(brand);
        var reply = new BrandReply() { Id = entity.Id, Name = entity.Name, Countryid = entity.CountryId, Comment = entity.Comment };
        return await Task.FromResult(reply);
    }

    public override async Task<ComponentReply> CreateComponent(CreateComponentRequest request, ServerCallContext context)
    {
        var component = request.FromMessage<Component>();
        var entity = await _repository.CreateComponent(component);
        var reply = entity.ToMessage<ComponentReply>();
        reply.Id = entity.Id;
        return await Task.FromResult(reply);
    }
    public override async Task<ComponentReply> GetComponent(GetByIdInt32Request request, ServerCallContext context)
    {
        var component = await _repository.GetComponent(request.Id)
            ?? throw new RpcException(new Status(StatusCode.NotFound,$"Component with id={request.Id} not found"));
        var reply = component.ToMessage<ComponentReply>();
        reply.Id = component.Id;
        return await Task.FromResult(reply);
    }

    public override async Task<ProductReply> CreateProduct(CreateProductRequest request, ServerCallContext context)
    {
        var product = request.FromMessage<Entities.Product>();
        var entity =  await _repository.CreateProduct(product);
        var reply = entity.ToMessage<ProductReply>();
        reply.Id = entity.Id;
        return await Task.FromResult(reply);
    }
    public override async Task<ShopProductReply> CreateShopProduct(CreateShopProductRequest request, ServerCallContext context)
    {
        var shopProduct = request.FromMessage<ShopProduct>();
        var entity = await _repository.CreateShopProduct(shopProduct);
        var reply = entity.ToMessage<ShopProductReply>();
        reply.Id = entity.Id;
        return await Task.FromResult(reply);
    }

    public override async Task<BrandReply> FindBrandByName(FindByNameRequest request, ServerCallContext context)
    {
        var brand = await _repository.FindBrandByName(request.Name)
         ?? throw new RpcException(new Status(StatusCode.NotFound,$"Brand with name '{request.Name}' not found"));
        return  await Task.FromResult(new BrandReply { Id = brand.Id, Name = brand.Name, Countryid = brand.CountryId, Comment = brand.Comment });
    }

    public override async Task<ComponentReply> FindComponentByName(FindByNameRequest request, ServerCallContext context)
    {
        var component = await _repository.FindComponentByName(request.Name)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Component with name '{request.Name}' not found"));
        var reply = component.ToMessage<ComponentReply>();
        reply.Id = component.Id;
        return  await Task.FromResult(reply);
    }

    public override async Task<CountryReply> FindCountryByName(FindByNameRequest request, ServerCallContext context)
    {
        var country = await _repository.FindCountryByName(request.Name)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Country with name '{request.Name}' not found"));
        return  await Task.FromResult(new CountryReply { Id = country.Id, Name = country.Name });

    }

    public override async Task<ProductReply> FindProductByName(FindByNameRequest request, ServerCallContext context)
    {
        var product = await _repository.FindProductByName(request.Name)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Product with name '{request.Name}' not found"));
        var reply = product.ToMessage<ProductReply>();
        reply.Id = product.Id;
        return  await Task.FromResult(reply);
    }

    public override async Task<ProductTypeReply> FindProductTypeByName(FindByNameRequest request, ServerCallContext context)
    {
        var productType = await _repository.FindProductTypeByName(request.Name)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Product type with name '{request.Name}' not found"));
        return  await Task.FromResult(new ProductTypeReply { Id = productType.Id, Name = productType.Name });
    }

    public override async Task<PurposeTypeReply> FindPurposeTypeByName(FindByNameRequest request, ServerCallContext context)
    {
        var purposeType = await _repository.FindPurposeTypeByName(request.Name)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Purpose type with name '{request.Name}' not found"));
        return  await Task.FromResult(new PurposeTypeReply { Id = purposeType.Id, Name = purposeType.Name });
    }

    public override async Task<ShopProductReply> GetShopProductByShopAndApiUrl(GetShopProductByShopAndApiUrlRequest request, ServerCallContext context)
    {
        var shopProduct = await _repository.GetShopProductByShopAndApiUrl(request.Shopid, request.Apiurl)
            ?? throw new RpcException(new Status(StatusCode.NotFound, "ShopProduct not found"));
        var reply = shopProduct.ToMessage<ShopProductReply>();
        reply.Id = shopProduct.Id;
        return await Task.FromResult(reply);
    }
    
    public override async Task<ShopProductReply> GetShopProductByShopAndItemId(GetShopProductByShopAndItemIdRequest request, ServerCallContext context)
    {
        var shopProduct = await _repository.GetShopProductByShopAndItemId(request.Shopid, request.Itemid)
            ?? throw new RpcException(new Status(StatusCode.NotFound, "ShopProduct not found"));
        var reply = shopProduct.ToMessage<ShopProductReply>();
        reply.Id = shopProduct.Id;
        return await Task.FromResult(reply);
    }

    public override async Task<ProductComponentReply> SetProductComponent(SetProductComponentRequest request, ServerCallContext context)
    {
        var productComponent =request.FromMessage<ProductComponent>();
        var entity = await _repository.SetProductComponent(productComponent);
        var reply = entity.ToMessage<ProductComponentReply>();
        return await Task.FromResult(reply);
    }

    public override async Task<BoolValue> UpdateShopProduct(UpdateShopProductRequest request, ServerCallContext context)
    {
        var shopProduct = request.FromMessage<ShopProduct>();
        var result = await _repository.UpdateShopProduct(shopProduct);
        return await Task.FromResult(new BoolValue { Value = result });
    }

    public override async Task<CurrencyReply> GetCurrencyByName(FindByNameRequest request, ServerCallContext context)
    {
        var currency = await _repository.GetCurrencyByName(request.Name)
             ?? throw new RpcException(new Status(StatusCode.NotFound, $"Currency type with name '{request.Name}' not found"));
        var reply = currency.ToMessage<CurrencyReply>();
        reply.Id = currency.Id;
        return await Task.FromResult(reply);
    }

    public override async Task<CurrencyReply> GetCurrencyByCode(GetCurrencyByCodeRequest request, ServerCallContext context)
    {
        var currency = await _repository.GetCurrencyByCode((short)request.Code)
             ?? throw new RpcException(new Status(StatusCode.NotFound, $"Currency type with code '{request.Code}' not found"));
        var reply = currency.ToMessage<CurrencyReply>();
        reply.Id = currency.Id;
        return await Task.FromResult(reply);
    }

    public override async Task<CurrencyReply> CreateCurrency(CreateCurrencyRequest request, ServerCallContext context)
    {
        var currency = request.FromMessage<Currency>();
        var entity = await _repository.CreateCurrency(currency);
        var reply = entity.ToMessage<CurrencyReply>();
        reply.Id = currency.Id;
        return await Task.FromResult(reply);
    }

    public override async Task<ShopProductPriceReply> CreateShopProductPrice(CreateShopProductPriceRequest request, ServerCallContext context)
    {
        var shopProductPrice = request.FromMessage<ShopProductPrice>();
        var entity = await _repository.CreateShopProductPrice(shopProductPrice);
        var reply = entity.ToMessage<ShopProductPriceReply>();
        reply.Id = entity.Id; ;
        return await Task.FromResult(reply);
    }

    public override async Task<BoolValue> UpdateShopProductPrice(UpdateShopProductPriceRequest request, ServerCallContext context)
    {
        var shopProductPrice = request.FromMessage<ShopProductPrice>();
        var result = await _repository.UpdateShopProductPrice(shopProductPrice);
        return await Task.FromResult(new BoolValue { Value = result });
    }

    public override async Task<ShopProductPriceReply> GetShopProductPrice(GetByIdInt64Request request, ServerCallContext context)
    {
        var shopProductPrice = await _repository.GetShopProductPrice(request.Id)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Shop product price id={request.Id}' not found")); ;
        var reply = shopProductPrice.ToMessage<ShopProductPriceReply>();
        reply.Id = shopProductPrice.Id;
        return await Task.FromResult(reply);
    }
}
