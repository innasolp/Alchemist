using Alchemist.Product.Data;
using Alchemist.Product.Infrastructure;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Mediator.Infrastructure.Command;
using Mediator.Infrastructure.Request;
using MediatR;
using Mapster;
using GrpcExtensions.Interception;
using GrpcExtensions.Message;

namespace Alchemist.Product.GrpcService.Services;

public class AlchemyService(IMediator mediator) : AlchemyGrpcService.AlchemyGrpcServiceBase
{
    private readonly IMediator _mediator = mediator;

    public override async Task<ProductTypeReply> CreateProductType(CreateProductTypeRequest request, ServerCallContext context)
    {
        if (string.IsNullOrEmpty(request.Name))
            throw GrpcStatuses.GetBadRequestRpcException(nameof(CreateProductTypeRequest.Name));

        var productType = new ProductType { Name = request.Name };
        var newProductType = await _mediator.Send(new CreateCommand<ProductType>(productType), context.CancellationToken);
        var reply = new ProductTypeReply() { Id = newProductType.Id, Name = newProductType.Name };
        return await Task.FromResult(reply);
    }

    public override async Task<PurposeTypeReply> CreatePurposeType(CreatePurposeTypeRequest request, ServerCallContext context)
    {
        if (string.IsNullOrEmpty(request.Name))
            throw GrpcStatuses.GetBadRequestRpcException(nameof(CreatePurposeTypeRequest.Name));

        var purposeType = new PurposeType { Name = request.Name };
        var newPurposeType = await _mediator.Send(new CreateCommand<PurposeType>(purposeType), context.CancellationToken);
        var reply = new PurposeTypeReply() { Id = newPurposeType.Id, Name = newPurposeType.Name };
        return await Task.FromResult(reply);
    }

    public override async Task<CountryReply> CreateCountry(CreateCountryRequest request, ServerCallContext context)
    {
        if (string.IsNullOrEmpty(request.Name))
            throw GrpcStatuses.GetBadRequestRpcException(nameof(CreateCountryRequest.Name));

        var country = new Country { Name = request.Name, Transcript = request.Transcript };
        var newCountry = await _mediator.Send(new CreateCommand<Country>(country), context.CancellationToken);
        var reply = new CountryReply() { Id = newCountry.Id, Name = newCountry.Name, Transcript = newCountry.Transcript };
        return await Task.FromResult(reply);
    }

    public override async Task<BrandReply> CreateBrand(CreateBrandRequest request, ServerCallContext context)
    {
        if (string.IsNullOrEmpty(request.Name))
            throw GrpcStatuses.GetBadRequestRpcException(nameof(CreateBrandRequest.Name));

        if (request.Countryid <= 0)
            throw GrpcStatuses.GetBadRequestRpcException(nameof(CreateBrandRequest.Countryid), "Invalid value");

        var brand = new Brand { Name = request.Name, CountryId = (short?)request.Countryid, Comment = request.Comment };
        var entity = await _mediator.Send(new CreateCommand<Brand>(brand), context.CancellationToken);
        var reply = new BrandReply() { Id = entity.Id, Name = entity.Name, Countryid = entity.CountryId, Comment = entity.Comment };
        return await Task.FromResult(reply);
    }

    public override async Task<ComponentReply> CreateComponent(CreateComponentRequest request, ServerCallContext context)
    {
        if (string.IsNullOrEmpty(request.Name))
            throw GrpcStatuses.GetBadRequestRpcException(nameof(CreateComponentRequest.Name));

        var component = request.Adapt<Component>();
        var entity = await _mediator.Send(new CreateCommand<Component>(component), context.CancellationToken);
        var reply = entity.Adapt<ComponentReply>();
        reply.Id = entity.Id;
        return await Task.FromResult(reply);
    }

    public override async Task<ComponentReply> GetComponent(GetByIdInt32Request request, ServerCallContext context)
    {
        if (request.Id <= 0)
            throw GrpcStatuses.GetBadRequestRpcException(nameof(GetByIdInt32Request.Id), "Invalid value");

        var component = await _mediator.Send(new GetByIdRequest<int, Component>(request.Id), context.CancellationToken)
            ?? throw new RpcException(new Status(StatusCode.NotFound,$"Component with id={request.Id} not found"));
        var reply = component.Adapt<ComponentReply>();
        reply.Id = component.Id;
        return await Task.FromResult(reply);
    }

    public override async Task<ProductReply> CreateProduct(CreateProductRequest request, ServerCallContext context)
    {
        if (string.IsNullOrEmpty(request.Name))
            throw GrpcStatuses.GetBadRequestRpcException(nameof(CreateProductRequest.Name));
        
        if (request.Producttypeid <= 0)
            throw GrpcStatuses.GetBadRequestRpcException(nameof(CreateProductRequest.Producttypeid), "InvalidValue");

        var product = request.Adapt<Data.Product>();
        var entity = await _mediator.Send(new CreateCommand<Data.Product>(product), context.CancellationToken);
        var reply = entity.Adapt<ProductReply>();
        reply.Id = entity.Id;
        return await Task.FromResult(reply);
    }
    public override async Task<ShopProductReply> CreateShopProduct(CreateShopProductRequest request, ServerCallContext context)
    {
        if (request.Productid <= 0)
            throw GrpcStatuses.GetBadRequestRpcException(nameof(CreateShopProductRequest.Productid), "InvalidValue");

        if (request.Shopid <= 0)
            throw GrpcStatuses.GetBadRequestRpcException(nameof(CreateShopProductRequest.Shopid), "InvalidValue");

        var shopProduct = request.Adapt<ShopProduct>();
        var entity = await _mediator.Send(new CreateCommand<ShopProduct>(shopProduct), context.CancellationToken);
        var reply = entity.Adapt<ShopProductReply>();
        reply.Id = entity.Id;
        return await Task.FromResult(reply);
    }

    public override async Task<ShopProductCategoryReply> AddShopProductCategory(ShopProductCategoryRequest request, ServerCallContext context)
    {
        if (request.Shopproductid <= 0)
            throw GrpcStatuses.GetBadRequestRpcException(nameof(ShopProductCategoryRequest.Shopproductid), "InvalidValue");
        
        if (request.Shopcategoryid <= 0)
            throw GrpcStatuses.GetBadRequestRpcException(nameof(ShopProductCategoryRequest.Shopcategoryid), "InvalidValue");

        var shopProductCategory = request.Adapt<ShopProductCategory>();
        var entity = await _mediator.Send(new CreateCommand<ShopProductCategory>(shopProductCategory), context.CancellationToken);
        var reply = entity.Adapt<ShopProductCategoryReply>();
        reply.Id = entity.Id;
        return await Task.FromResult(reply);
    }

    public override async Task<BoolValue> CheckShopProductCategory(ShopProductCategoryRequest request, ServerCallContext context)
    {
        if (request.Shopproductid <= 0)
            throw GrpcStatuses.GetBadRequestRpcException(nameof(ShopProductCategoryRequest.Shopproductid), "InvalidValue");

        if (request.Shopcategoryid <= 0)
            throw GrpcStatuses.GetBadRequestRpcException(nameof(ShopProductCategoryRequest.Shopcategoryid), "InvalidValue");

        var result = await _mediator.Send(new CheckShopProductCategoryRequest(request.Shopproductid, request.Shopcategoryid), context.CancellationToken);
        return await Task.FromResult(new BoolValue { Value = result });
    }

    public override async Task<ShopProductCategoryListReply> GetShopProductCategories(GetByIdInt64Request request, ServerCallContext context)
    {
        if (request.Id <= 0)
            throw GrpcStatuses.GetBadRequestRpcException(nameof(GetByIdInt64Request.Id), "InvalidValue");        

        var shopProductCategories = await _mediator.Send(new GetShopProductCategoriesRequest(request.Id), context.CancellationToken)
             ?? throw new RpcException(new Status(StatusCode.NotFound, $"categories for shop product with id '{request.Id}' not found"));
        var replyList = await shopProductCategories.ToListReply<ShopProductCategoryListReply, ShopProductCategoryReply, ShopProductCategory>((s) =>
        {
            var reply = s.Adapt<ShopProductCategoryReply>();
            reply.Id = s.Id;
            return reply;
        });

        return await Task.FromResult(replyList);
    }

    public override async Task<BrandReply> FindBrandByName(FindByNameRequest request, ServerCallContext context)
    {
        if (string.IsNullOrEmpty(request.Name))
            throw GrpcStatuses.GetBadRequestRpcException(nameof(FindByNameRequest.Name));

        var brand = await _mediator.Send(new FindByNameRequest<Brand>(request.Name, e => e.Name), context.CancellationToken)
                 ?? throw new RpcException(new Status(StatusCode.NotFound, $"Brand with name '{request.Name}' not found")); ;


        return brand.Adapt<BrandReply>();
    }

    public override async Task<ComponentReply> FindComponentByName(FindByNameRequest request, ServerCallContext context)
    {
        if (string.IsNullOrEmpty(request.Name))
            throw GrpcStatuses.GetBadRequestRpcException(nameof(FindByNameRequest.Name));        
        
        var component = await _mediator.Send(new FindByNameRequest<Component>(request.Name, e => e.Name), context.CancellationToken)
                ?? throw new RpcException(new Status(StatusCode.NotFound, $"Component with name '{request.Name}' not found"));        

        var reply = component.Adapt<ComponentReply>();
        reply.Id = component.Id;
        return  await Task.FromResult(reply);
    }

    public override async Task<CountryReply> FindCountryByName(FindByNameRequest request, ServerCallContext context)
    {
        if (string.IsNullOrEmpty(request.Name))
            throw GrpcStatuses.GetBadRequestRpcException(nameof(FindByNameRequest.Name));

        var country = await _mediator.Send(new FindByNameRequest<Country>(request.Name, e => e.Name), context.CancellationToken)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Country with name '{request.Name}' not found"));  
        
        return  await Task.FromResult(new CountryReply { Id = country.Id, Name = country.Name });
    }

    public override async Task<ProductReply> FindProductByName(FindByNameRequest request, ServerCallContext context)
    {
        if (string.IsNullOrEmpty(request.Name))
            throw GrpcStatuses.GetBadRequestRpcException(nameof(FindByNameRequest.Name));

        var product = await _mediator.Send(new FindByNameRequest<Data.Product>(request.Name, e => e.Name), context.CancellationToken)
               ?? throw new RpcException(new Status(StatusCode.NotFound, $"Product with name '{request.Name}' not found"));

        var reply = product.Adapt<ProductReply>();
        reply.Id = product.Id;
        return  await Task.FromResult(reply);
    }

    public override async Task<ProductReply> FindProductByNameAndBrand(FindProductByNameAndBrandRequest request, ServerCallContext context)
    {
        if (string.IsNullOrEmpty(request.Name))
            throw GrpcStatuses.GetBadRequestRpcException(nameof(FindProductByNameAndBrandRequest.Name));
        
        if (string.IsNullOrEmpty(request.Brand))
            throw GrpcStatuses.GetBadRequestRpcException(nameof(FindProductByNameAndBrandRequest.Brand));

        var product = await _mediator.Send(new Infrastructure.FindProductByNameAndBrandRequest(request.Name, request.Brand), context.CancellationToken)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Product with name '{request.Name}' and brand {request.Brand} not found"));
        var reply = product.Adapt<ProductReply>();
        reply.Id = product.Id;
        return await Task.FromResult(reply);
    }

    public override async Task<ProductTypeReply> FindProductTypeByName(FindByNameRequest request, ServerCallContext context)
    {
        if (string.IsNullOrEmpty(request.Name))
            throw GrpcStatuses.GetBadRequestRpcException(nameof(FindByNameRequest.Name));

        var productType = await _mediator.Send(new FindByNameRequest<ProductType>(request.Name, e => e.Name), context.CancellationToken)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Product type with name '{request.Name}' not found"));
        return  await Task.FromResult(new ProductTypeReply { Id = productType.Id, Name = productType.Name });
    }

    public override async Task<PurposeTypeReply> FindPurposeTypeByName(FindByNameRequest request, ServerCallContext context)
    {
        if (string.IsNullOrEmpty(request.Name))
            throw GrpcStatuses.GetBadRequestRpcException(nameof(FindByNameRequest.Name));

        var purposeType = await _mediator.Send(new FindByNameRequest<PurposeType>(request.Name, e => e.Name), context.CancellationToken)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Purpose type with name '{request.Name}' not found"));
        return  await Task.FromResult(new PurposeTypeReply { Id = purposeType.Id, Name = purposeType.Name });
    }

    public override async Task<ShopProductReply> GetShopProductByShopIdAndApiUrl(GetShopProductByShopIdAndApiUrlRequest request, ServerCallContext context)
    {
        if (string.IsNullOrEmpty(request.Apiurl))
            throw GrpcStatuses.GetBadRequestRpcException(nameof(GetShopProductByShopIdAndApiUrlRequest.Apiurl));

        if (request.Shopid <= 0)
            throw GrpcStatuses.GetBadRequestRpcException(nameof(GetShopProductByShopIdAndApiUrlRequest.Shopid), "Invalid value");

        var shopProduct = await _mediator.Send(new GetShopProductByShopAndItemUrlRequest(request.Shopid, request.Apiurl), context.CancellationToken)
            ?? throw new RpcException(new Status(StatusCode.NotFound, "ShopProduct not found"));
        var reply = shopProduct.Adapt<ShopProductReply>();
        reply.Id = shopProduct.Id;
        return await Task.FromResult(reply);
    }
    
    public override async Task<ShopProductReply> GetShopProductByShopIdAndItemId(GetShopProductByShopIdAndItemIdRequest request, ServerCallContext context)
    {
        if (request.Shopid <= 0)
            throw GrpcStatuses.GetBadRequestRpcException(nameof(GetShopProductByShopIdAndItemIdRequest.Shopid), "Invalid value");

        if (string.IsNullOrEmpty(request.Itemid))
            throw GrpcStatuses.GetBadRequestRpcException(nameof(GetShopProductByShopIdAndItemIdRequest.Itemid));

        var shopProduct = await _mediator.Send(new GetShopProductByShopAndItemIdRequest(request.Shopid, request.Itemid), context.CancellationToken)
            ?? throw new RpcException(new Status(StatusCode.NotFound, "ShopProduct not found"));
        var reply = shopProduct.Adapt<ShopProductReply>();
        reply.Id = shopProduct.Id;
        return await Task.FromResult(reply);
    }

    public override async Task<ShopProductReply> GetShopProductByShopIdAndProductId(GetShopProductByShopIdAndProductIdRequest request, ServerCallContext context)
    {
        if (request.Shopid <= 0)
            throw GrpcStatuses.GetBadRequestRpcException(nameof(GetShopProductByShopIdAndProductIdRequest.Shopid), "InvalidValue");

        if (request.Productid <= 0)
            throw GrpcStatuses.GetBadRequestRpcException(nameof(GetShopProductByShopIdAndProductIdRequest.Productid), "InvalidValue");

        var shopProduct = await _mediator.Send(new GetShopProductByShopAndProductIdRequest(request.Shopid, request.Productid), context.CancellationToken)
            ?? throw new RpcException(new Status(StatusCode.NotFound, "ShopProduct not found"));
        var reply = shopProduct.Adapt<ShopProductReply>();
        reply.Id = shopProduct.Id;
        return await Task.FromResult(reply);
    }

    public override async Task<ProductComponentReply> SetProductComponent(SetProductComponentRequest request, ServerCallContext context)
    {
        if (request.Productid <= 0)
            throw GrpcStatuses.GetBadRequestRpcException(nameof(SetProductComponentRequest.Productid), "Invalid value");

        if (request.Componentid <= 0)
            throw GrpcStatuses.GetBadRequestRpcException(nameof(SetProductComponentRequest.Componentid), "Invalid value");
        
        if (request.SequalNumber <= 0)
            throw GrpcStatuses.GetBadRequestRpcException(nameof(SetProductComponentRequest.SequalNumber), "Invalid value");

        var productComponent =request.Adapt<ProductComponent>();
        var entity = await _mediator.Send(new SetProductComponentCommand(productComponent), context.CancellationToken);
        var reply = entity.Adapt<ProductComponentReply>();
        return await Task.FromResult(reply);
    }

    public override async Task<ShopProductReply> UpdateShopProduct(UpdateShopProductRequest request, ServerCallContext context)
    {
        if (request.Productid <= 0)
            throw GrpcStatuses.GetBadRequestRpcException(nameof(UpdateShopProductRequest.Productid), "Invalid value");
        
        if (request.Shopid <= 0)
            throw GrpcStatuses.GetBadRequestRpcException(nameof(UpdateShopProductRequest.Shopid), "Invalid value");

        if (string.IsNullOrEmpty(request.Itemid))
            throw GrpcStatuses.GetBadRequestRpcException(nameof(UpdateShopProductRequest.Itemid));
        
        if (string.IsNullOrEmpty(request.Apiurl))
            throw GrpcStatuses.GetBadRequestRpcException(nameof(UpdateShopProductRequest.Apiurl));

        var shopProduct = request.Adapt<ShopProduct>();
        shopProduct.Id = request.Id;
        var result = await _mediator.Send(new UpdateCommand<ShopProduct>(shopProduct), context.CancellationToken);
        var reply = result.Adapt<ShopProductReply>();
        reply.Id = result.Id;
        return await Task.FromResult(reply);
    }

    public override async Task<CurrencyReply> GetCurrencyByName(FindByNameRequest request, ServerCallContext context)
    {
        if (string.IsNullOrEmpty(request.Name))
            throw GrpcStatuses.GetBadRequestRpcException(nameof(FindByNameRequest.Name));

        var currency = await _mediator.Send(new FindByNameRequest<Currency>(request.Name, e => e.Name), context.CancellationToken)
             ?? throw new RpcException(new Status(StatusCode.NotFound, $"Currency type with name '{request.Name}' not found"));
        var reply = currency.Adapt<CurrencyReply>();
        reply.Id = currency.Id;
        return await Task.FromResult(reply);
    }

    public override async Task<CurrencyReply> GetCurrencyByCode(GetCurrencyByCodeRequest request, ServerCallContext context)
    {
        if (request.Code <= 0)
            throw GrpcStatuses.GetBadRequestRpcException(nameof(GetCurrencyByCodeRequest.Code), "Invalid value");

        var currency = await _mediator.Send(new Infrastructure.GetCurrencyByCodeRequest((short)request.Code), context.CancellationToken)
             ?? throw new RpcException(new Status(StatusCode.NotFound, $"Currency type with code '{request.Code}' not found"));
        var reply = currency.Adapt<CurrencyReply>();
        reply.Id = currency.Id;
        return await Task.FromResult(reply);
    }

    public override async Task<CurrencyReply> CreateCurrency(CreateCurrencyRequest request, ServerCallContext context)
    {
        if (string.IsNullOrEmpty(request.Name))
            throw GrpcStatuses.GetBadRequestRpcException(nameof(CreateCurrencyRequest.Name));

        var currency = request.Adapt<Currency>();
        var entity = await _mediator.Send(new CreateCommand<Currency>(currency), context.CancellationToken);
        var reply = entity.Adapt<CurrencyReply>();
        reply.Id = currency.Id;
        return await Task.FromResult(reply);
    }

    public override async Task<ShopProductPriceReply> CreateShopProductPrice(CreateShopProductPriceRequest request, ServerCallContext context)
    {
        if (request.Shopproductid <= 0)
            throw GrpcStatuses.GetBadRequestRpcException(nameof(CreateShopProductPriceRequest.Shopproductid), "Invalid value");
        
        if (request.Price <= 0)
            throw GrpcStatuses.GetBadRequestRpcException(nameof(CreateShopProductPriceRequest.Price), "Invalid value");

        var shopProductPrice = request.Adapt<ShopProductPrice>();
        var entity = await _mediator.Send(new CreateCommand<ShopProductPrice>(shopProductPrice), context.CancellationToken);
        var reply = entity.Adapt<ShopProductPriceReply>();
        reply.Id = entity.Id; 
        return await Task.FromResult(reply);
    }

    public override async Task<ShopProductPriceReply> UpdateShopProductPrice(UpdateShopProductPriceRequest request, ServerCallContext context)
    {
        if (request.Shopproductid <= 0)
            throw GrpcStatuses.GetBadRequestRpcException(nameof(UpdateShopProductPriceRequest.Shopproductid), "Invalid value");

        if (request.Price <= 0)
            throw GrpcStatuses.GetBadRequestRpcException(nameof(UpdateShopProductPriceRequest.Price), "Invalid value");

        var shopProductPrice = request.Adapt<ShopProductPrice>();
        shopProductPrice.Id = request.Id;
        var result = await _mediator.Send(new UpdateCommand<ShopProductPrice>(shopProductPrice), context.CancellationToken);
        return await Task.FromResult(new ShopProductPriceReply 
            { Id = result.Id, Currencyid = result.CurrencyId, Price = result.Price, Shopproductid = result.ShopProductId });
    }

    public override async Task<ShopProductPriceReply> GetShopProductPrice(GetByIdInt64Request request, ServerCallContext context)
    {
        if (request.Id <= 0)
            throw GrpcStatuses.GetBadRequestRpcException(nameof(GetByIdInt64Request.Id), "Invalid value");

        var shopProductPrice = await _mediator.Send(new GetByIdRequest<long, ShopProductPrice>(request.Id), context.CancellationToken)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Shop product price id={request.Id}' not found")); ;
        var reply = shopProductPrice.Adapt<ShopProductPriceReply>();
        reply.Id = shopProductPrice.Id;
        return await Task.FromResult(reply);
    }

    public override async Task<PurposeTypeListReply> GetProductPurposes(GetProductPurposesRequest request, ServerCallContext context)
    {
        if (request.Productid <= 0)
            throw GrpcStatuses.GetBadRequestRpcException(nameof(GetProductPurposesRequest.Productid), "Invalid value");

        var purposeTypes = await _mediator.Send(new GetProductPurposeTypesRequest(request.Productid), context.CancellationToken);
        
        var reply = await purposeTypes.ToListReply<PurposeTypeListReply, PurposeTypeReply, PurposeType>(
            (purpose) => new PurposeTypeReply { Id = purpose.Id, Name = purpose.Name  }
        );

        return await Task.FromResult(reply);
    }

    public override async Task<ProductPurposeReply> SetProductPurpose(SetProductPurposeRequest request, ServerCallContext context)
    {
        if (request.Productid <= 0)
            throw GrpcStatuses.GetBadRequestRpcException(nameof(SetProductPurposeRequest.Productid), "Invalid value");

        if (request.Purposetypeid <= 0)
            throw GrpcStatuses.GetBadRequestRpcException(nameof(SetProductPurposeRequest.Purposetypeid), "Invalid value");

        var productPurpose = new ProductPurpose { ProductId = request.Productid, PurposeTypeId = (short)request.Purposetypeid };
        var entity = await _mediator.Send( new SetProductPurposeCommand(productPurpose), context.CancellationToken);
        var reply = new ProductPurposeReply { Productid = entity.ProductId, Purposetypeid = entity.PurposeTypeId };

        return await Task.FromResult(reply);
    }
}