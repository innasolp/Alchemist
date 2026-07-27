using Alchemist.Product.Data;
using Db.Infrastructure;
using Db.Infrastructure.Commands;
using Db.Infrastructure.Requests;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcExtensions.Interception;
using GrpcExtensions.Message;
using Mapster;
using Product.Data.Infrastructure;

using DbFindProductByNameAndBrandRequest = Product.Data.Infrastructure.FindProductByNameAndBrandRequest;
using DbGetCurrencyByCodeRequest = Product.Data.Infrastructure.GetCurrencyByCodeRequest;

namespace Alchemist.Product.GrpcService.Services;

public class AlchemyService(IServiceScopeFactory scopeFactory) : AlchemyGrpcService.AlchemyGrpcServiceBase
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

    public override async Task<ProductTypeReply> CreateProductType(CreateProductTypeRequest request, ServerCallContext context)
    {
        if (string.IsNullOrEmpty(request.Name))
            throw GrpcStatuses.GetBadRequestRpcException(nameof(CreateProductTypeRequest.Name), "Value is null or empty");

        var productType = new ProductType { Name = request.Name };
        var commandHandler = context.GetHttpContext().RequestServices.GetRequiredService<ICommandHandler<CreateCommand<ProductType>>>();
        await commandHandler.Handle(new CreateCommand<ProductType>(productType), context.CancellationToken);
        var reply = new ProductTypeReply() { Id = productType.Id, Name = productType.Name };
        return await Task.FromResult(reply);
    }

    public override async Task<PurposeTypeReply> CreatePurposeType(CreatePurposeTypeRequest request, ServerCallContext context)
    {
        if (string.IsNullOrEmpty(request.Name))
            throw GrpcStatuses.GetBadRequestRpcException(nameof(CreatePurposeTypeRequest.Name), "Value is null or empty");

        var purposeType = new PurposeType { Name = request.Name };
        var commandHandler = context.GetHttpContext().RequestServices.GetRequiredService<ICommandHandler<CreateCommand<PurposeType>>>();
        await commandHandler.Handle(new CreateCommand<PurposeType>(purposeType), context.CancellationToken);
        var reply = new PurposeTypeReply() { Id = purposeType.Id, Name = purposeType.Name };
        return await Task.FromResult(reply);
    }

    public override async Task<CountryReply> CreateCountry(CreateCountryRequest request, ServerCallContext context)
    {
        if (string.IsNullOrEmpty(request.Name))
            throw GrpcStatuses.GetBadRequestRpcException(nameof(CreateCountryRequest.Name));

        var country = new Country { Name = request.Name, Transcript = request.Transcript };
        var commandHandler = context.GetHttpContext().RequestServices.GetRequiredService<ICommandHandler<CreateCommand<Country>>>();
        await commandHandler.Handle(new CreateCommand<Country>(country), context.CancellationToken);
        var reply = new CountryReply() { Id = country.Id, Name = country.Name, Transcript = country.Transcript };
        return await Task.FromResult(reply);
    }

    public override async Task<BrandReply> CreateBrand(CreateBrandRequest request, ServerCallContext context)
    {
        if (string.IsNullOrEmpty(request.Name))
            throw GrpcStatuses.GetBadRequestRpcException(nameof(CreateBrandRequest.Name), "Value is null or empty");

        if (request.Countryid <= 0)
            throw GrpcStatuses.GetBadRequestRpcException(nameof(CreateBrandRequest.Countryid), "Invalid value");

        var brand = new Brand { Name = request.Name, CountryId = (short?)request.Countryid, Comment = request.Comment };
        var commandHandler = context.GetHttpContext().RequestServices.GetRequiredService<ICommandHandler<CreateCommand<Brand>>>();
        await commandHandler.Handle(new CreateCommand<Brand>(brand), context.CancellationToken);
        var reply = new BrandReply() { Id = brand.Id, Name = brand.Name, Countryid = brand.CountryId, Comment = brand.Comment };
        return await Task.FromResult(reply);
    }

    public override async Task<ComponentReply> CreateComponent(CreateComponentRequest request, ServerCallContext context)
    {
        if (string.IsNullOrEmpty(request.Name))
            throw GrpcStatuses.GetBadRequestRpcException(nameof(CreateComponentRequest.Name), "Value is null or empty");

        var component = request.Adapt<Component>();
        var commandHandler = context.GetHttpContext().RequestServices.GetRequiredService<ICommandHandler<CreateCommand<Component>>>();
        await commandHandler.Handle(new CreateCommand<Component>(component), context.CancellationToken);
        var reply = component.Adapt<ComponentReply>();
        reply.Id = component.Id;
        return await Task.FromResult(reply);
    }

    public override async Task<ComponentReply> GetComponent(GetByIdInt32Request request, ServerCallContext context)
    {
        if (request.Id <= 0)
            throw GrpcStatuses.GetBadRequestRpcException(nameof(GetByIdInt32Request.Id), "Invalid value");

        var requestHandler = context.GetHttpContext().RequestServices.GetRequiredService<IRequestHandler<GetByIdRequest<int, Component>, Component>>();
        var component = await requestHandler.Handle(new GetByIdRequest<int, Component>(request.Id), context.CancellationToken)
            ?? throw new RpcException(new Status(StatusCode.NotFound,$"Component with id={request.Id} not found"));
        var reply = component.Adapt<ComponentReply>();
        reply.Id = component.Id;
        return await Task.FromResult(reply);
    }

    public override async Task<ProductReply> CreateProduct(CreateProductRequest request, ServerCallContext context)
    {
        if (string.IsNullOrEmpty(request.Name))
            throw GrpcStatuses.GetBadRequestRpcException(nameof(CreateProductRequest.Name), "Value is null or empty");
        
        if (request.Producttypeid <= 0)
            throw GrpcStatuses.GetBadRequestRpcException(nameof(CreateProductRequest.Producttypeid), "InvalidValue");

        var product = request.Adapt<Data.Product>();
        var commandHandler = context.GetHttpContext().RequestServices.GetRequiredService<ICommandHandler<CreateCommand<Data.Product>>>();
        await commandHandler.Handle(new CreateCommand<Data.Product>(product), context.CancellationToken);
        var reply = product.Adapt<ProductReply>();
        reply.Id = product.Id;
        return await Task.FromResult(reply);
    }
    public override async Task<ShopProductReply> CreateShopProduct(CreateShopProductRequest request, ServerCallContext context)
    {
        if (request.Productid <= 0)
            throw GrpcStatuses.GetBadRequestRpcException(nameof(CreateShopProductRequest.Productid), "InvalidValue");

        if (request.Shopid <= 0)
            throw GrpcStatuses.GetBadRequestRpcException(nameof(CreateShopProductRequest.Shopid), "InvalidValue");

        var shopProduct = request.Adapt<ShopProduct>();
        var commandHandler = context.GetHttpContext().RequestServices.GetRequiredService<ICommandHandler<CreateCommand<ShopProduct>>>();
        await commandHandler.Handle(new CreateCommand<ShopProduct>(shopProduct), context.CancellationToken);
        var reply = shopProduct.Adapt<ShopProductReply>();
        reply.Id = shopProduct.Id;
        return await Task.FromResult(reply);
    }

    public override async Task<ShopProductCategoryReply> AddShopProductCategory(ShopProductCategoryRequest request, ServerCallContext context)
    {
        if (request.Shopproductid <= 0)
            throw GrpcStatuses.GetBadRequestRpcException(nameof(ShopProductCategoryRequest.Shopproductid), "InvalidValue");
        
        if (request.Shopcategoryid <= 0)
            throw GrpcStatuses.GetBadRequestRpcException(nameof(ShopProductCategoryRequest.Shopcategoryid), "InvalidValue");

        var shopProductCategory = request.Adapt<ShopProductCategory>();
        var commandHandler = context.GetHttpContext().RequestServices.GetRequiredService<ICommandHandler<CreateCommand<ShopProductCategory>>>();
        await commandHandler.Handle(new CreateCommand<ShopProductCategory>(shopProductCategory), context.CancellationToken);
        var reply = shopProductCategory.Adapt<ShopProductCategoryReply>();
        reply.Id = shopProductCategory.Id;
        return await Task.FromResult(reply);
    }

    public override async Task<BoolValue> CheckShopProductCategory(ShopProductCategoryRequest request, ServerCallContext context)
    {
        if (request.Shopproductid <= 0)
            throw GrpcStatuses.GetBadRequestRpcException(nameof(ShopProductCategoryRequest.Shopproductid), "InvalidValue");

        if (request.Shopcategoryid <= 0)
            throw GrpcStatuses.GetBadRequestRpcException(nameof(ShopProductCategoryRequest.Shopcategoryid), "InvalidValue");

        var requestHandler = context.GetHttpContext().RequestServices.GetRequiredService<IRequestHandler<CheckShopProductCategoryRequest, bool>>();
        var result = await requestHandler.Handle(new CheckShopProductCategoryRequest(request.Shopproductid, request.Shopcategoryid), context.CancellationToken);
        return await Task.FromResult(new BoolValue { Value = result });
    }

    public override async Task<ShopProductCategoryListReply> GetShopProductCategories(GetByIdInt64Request request, ServerCallContext context)
    {
        if (request.Id <= 0)
            throw GrpcStatuses.GetBadRequestRpcException(nameof(GetByIdInt64Request.Id), "InvalidValue");

        var requestHandler = context.GetHttpContext().RequestServices.GetRequiredService<IRequestHandler<GetShopProductCategoriesRequest, List<ShopProductCategory>>>();
        var shopProductCategories = await requestHandler.Handle(new GetShopProductCategoriesRequest(request.Id), context.CancellationToken)
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

        var requestHandler = context.GetHttpContext().RequestServices.GetRequiredService<IRequestHandler<FindByNameRequest<Brand>, Brand>>();
        var brand = await requestHandler.Handle(new FindByNameRequest<Brand>(request.Name), context.CancellationToken)
                 ?? throw new RpcException(new Status(StatusCode.NotFound, $"Brand with name '{request.Name}' not found")); 

        return brand.Adapt<BrandReply>();
    }

    public override async Task<ComponentReply> FindComponentByName(FindByNameRequest request, ServerCallContext context)
    {
        if (string.IsNullOrEmpty(request.Name))
            throw GrpcStatuses.GetBadRequestRpcException(nameof(FindByNameRequest.Name));

        var requestHandler = context.GetHttpContext().RequestServices.GetRequiredService<IRequestHandler<FindByNameRequest<Component>, Component>>();
        var component = await requestHandler.Handle(new FindByNameRequest<Component>(request.Name), context.CancellationToken)
                ?? throw new RpcException(new Status(StatusCode.NotFound, $"Component with name '{request.Name}' not found"));        

        var reply = component.Adapt<ComponentReply>();
        reply.Id = component.Id;
        return  await Task.FromResult(reply);
    }

    public override async Task<CountryReply> FindCountryByName(FindByNameRequest request, ServerCallContext context)
    {
        if (string.IsNullOrEmpty(request.Name))
            throw GrpcStatuses.GetBadRequestRpcException(nameof(FindByNameRequest.Name));

        var requestHandler = context.GetHttpContext().RequestServices.GetRequiredService<IRequestHandler<FindByNameRequest<Country>, Country>>();
        var country = await requestHandler.Handle(new FindByNameRequest<Country>(request.Name), context.CancellationToken)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Country with name '{request.Name}' not found"));  
        
        return  await Task.FromResult(new CountryReply { Id = country.Id, Name = country.Name });
    }

    public override async Task<ProductReply> FindProductByName(FindByNameRequest request, ServerCallContext context)
    {
        if (string.IsNullOrEmpty(request.Name))
            throw GrpcStatuses.GetBadRequestRpcException(nameof(FindByNameRequest.Name));

        var requestHandler = context.GetHttpContext().RequestServices.GetRequiredService<IRequestHandler<FindByNameRequest<Data.Product>, Data.Product>>();
        var product = await requestHandler.Handle(new FindByNameRequest<Data.Product>(request.Name), context.CancellationToken)
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

        var requestHandler = context.GetHttpContext().RequestServices.GetRequiredService<IRequestHandler<DbFindProductByNameAndBrandRequest, Data.Product>>();
        var product = await requestHandler.Handle(new DbFindProductByNameAndBrandRequest(request.Name, request.Brand), context.CancellationToken)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Product with name '{request.Name}' and brand {request.Brand} not found"));
        var reply = product.Adapt<ProductReply>();
        reply.Id = product.Id;
        return await Task.FromResult(reply);
    }

    public override async Task<ProductTypeReply> FindProductTypeByName(FindByNameRequest request, ServerCallContext context)
    {
        if (string.IsNullOrEmpty(request.Name))
            throw GrpcStatuses.GetBadRequestRpcException(nameof(FindByNameRequest.Name));

        var requestHandler = context.GetHttpContext().RequestServices.GetRequiredService<IRequestHandler<FindByNameRequest<Data.ProductType>, Data.ProductType>>();
        var productType = await requestHandler.Handle(new FindByNameRequest<ProductType>(request.Name), context.CancellationToken)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Product type with name '{request.Name}' not found"));
        return  await Task.FromResult(new ProductTypeReply { Id = productType.Id, Name = productType.Name });
    }

    public override async Task<PurposeTypeReply> FindPurposeTypeByName(FindByNameRequest request, ServerCallContext context)
    {
        if (string.IsNullOrEmpty(request.Name))
            throw GrpcStatuses.GetBadRequestRpcException(nameof(FindByNameRequest.Name));

        var requestHandler = context.GetHttpContext().RequestServices.GetRequiredService<IRequestHandler<FindByNameRequest<PurposeType>, PurposeType>>();
        var purposeType = await requestHandler.Handle(new FindByNameRequest<PurposeType>(request.Name), context.CancellationToken)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Purpose type with name '{request.Name}' not found"));
        return  await Task.FromResult(new PurposeTypeReply { Id = purposeType.Id, Name = purposeType.Name });
    }

    public override async Task<ShopProductReply> GetShopProductByShopIdAndApiUrl(GetShopProductByShopIdAndApiUrlRequest request, ServerCallContext context)
    {
        if (string.IsNullOrEmpty(request.Apiurl))
            throw GrpcStatuses.GetBadRequestRpcException(nameof(GetShopProductByShopIdAndApiUrlRequest.Apiurl), "Value is null or empty");

        if (request.Shopid <= 0)
            throw GrpcStatuses.GetBadRequestRpcException(nameof(GetShopProductByShopIdAndApiUrlRequest.Shopid), "Invalid value");

        var requestHandler = context.GetHttpContext().RequestServices.GetRequiredService<IRequestHandler<GetShopProductByShopAndItemUrlRequest, ShopProduct>>();
        var shopProduct = await requestHandler.Handle(new GetShopProductByShopAndItemUrlRequest(request.Shopid, request.Apiurl), context.CancellationToken)
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
            throw GrpcStatuses.GetBadRequestRpcException(nameof(GetShopProductByShopIdAndItemIdRequest.Itemid), "Value is null or empty");

        var requestHandler = context.GetHttpContext().RequestServices.GetRequiredService<IRequestHandler<GetShopProductByShopAndItemIdRequest, ShopProduct>>();
        var shopProduct = await requestHandler.Handle(new GetShopProductByShopAndItemIdRequest(request.Shopid, request.Itemid), context.CancellationToken)
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

        var requestHandler = context.GetHttpContext().RequestServices.GetRequiredService<IRequestHandler<GetShopProductByShopAndProductIdRequest, ShopProduct>>();
        var shopProduct = await requestHandler.Handle(new GetShopProductByShopAndProductIdRequest(request.Shopid, request.Productid), context.CancellationToken)
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
        var commandHandler = context.GetHttpContext().RequestServices.GetRequiredService<ICommandHandler<SetProductComponentCommand>>();
        await commandHandler.Handle(new SetProductComponentCommand(productComponent), context.CancellationToken);
        var reply = productComponent.Adapt<ProductComponentReply>();
        return await Task.FromResult(reply);
    }

    public override async Task<ShopProductReply> UpdateShopProduct(UpdateShopProductRequest request, ServerCallContext context)
    {
        if (request.Productid <= 0)
            throw GrpcStatuses.GetBadRequestRpcException(nameof(UpdateShopProductRequest.Productid), "Invalid value");
        
        if (request.Shopid <= 0)
            throw GrpcStatuses.GetBadRequestRpcException(nameof(UpdateShopProductRequest.Shopid), "Invalid value");

        if (string.IsNullOrEmpty(request.Itemid))
            throw GrpcStatuses.GetBadRequestRpcException(nameof(UpdateShopProductRequest.Itemid), "Value is null or empty");
        
        if (string.IsNullOrEmpty(request.Apiurl))
            throw GrpcStatuses.GetBadRequestRpcException(nameof(UpdateShopProductRequest.Apiurl), "Value is null or empty");

        var shopProduct = request.Adapt<ShopProduct>();
        shopProduct.Id = request.Id;
        var commandHandler = context.GetHttpContext().RequestServices.GetRequiredService<ICommandHandler<UpdateCommand<ShopProduct>>>();
        await commandHandler.Handle(new UpdateCommand<ShopProduct>(shopProduct), context.CancellationToken);
        var reply = shopProduct.Adapt<ShopProductReply>();
        reply.Id = shopProduct.Id;
        return await Task.FromResult(reply);
    }

    public override async Task<CurrencyReply> GetCurrencyByName(FindByNameRequest request, ServerCallContext context)
    {
        if (string.IsNullOrEmpty(request.Name))
            throw GrpcStatuses.GetBadRequestRpcException(nameof(FindByNameRequest.Name));

        var requestHandler = context.GetHttpContext().RequestServices.GetRequiredService<IRequestHandler<FindByNameRequest<Currency>, Currency>>();
        var currency = await requestHandler.Handle(new FindByNameRequest<Currency>(request.Name), context.CancellationToken)
             ?? throw new RpcException(new Status(StatusCode.NotFound, $"Currency type with name '{request.Name}' not found"));
        var reply = currency.Adapt<CurrencyReply>();
        reply.Id = currency.Id;
        return await Task.FromResult(reply);
    }

    public override async Task<CurrencyReply> GetCurrencyByCode(GetCurrencyByCodeRequest request, ServerCallContext context)
    {
        if (request.Code <= 0)
            throw GrpcStatuses.GetBadRequestRpcException(nameof(GetCurrencyByCodeRequest.Code), "Invalid value");

        var requestHandler = context.GetHttpContext().RequestServices.GetRequiredService<IRequestHandler<DbGetCurrencyByCodeRequest, Currency>>();
        var currency = await requestHandler.Handle(new DbGetCurrencyByCodeRequest((short)request.Code), context.CancellationToken)
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
        var commandHandler = context.GetHttpContext().RequestServices.GetRequiredService<ICommandHandler<CreateCommand<Currency>>>();
        await commandHandler.Handle(new CreateCommand<Currency>(currency), context.CancellationToken);
        var reply = currency.Adapt<CurrencyReply>();
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
        var commandHandler = context.GetHttpContext().RequestServices.GetRequiredService<ICommandHandler<CreateCommand<ShopProductPrice>>>();
        await commandHandler.Handle(new CreateCommand<ShopProductPrice>(shopProductPrice), context.CancellationToken);
        var reply = shopProductPrice.Adapt<ShopProductPriceReply>();
        reply.Id = shopProductPrice.Id; 
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
        var commandHandler = context.GetHttpContext().RequestServices.GetRequiredService<ICommandHandler<UpdateCommand<ShopProductPrice>>>();
        await commandHandler.Handle(new UpdateCommand<ShopProductPrice>(shopProductPrice), context.CancellationToken);
        return await Task.FromResult(new ShopProductPriceReply 
            { Id = shopProductPrice.Id, Currencyid = shopProductPrice.CurrencyId, Price = shopProductPrice.Price, Shopproductid = shopProductPrice.ShopProductId });
    }

    public override async Task<ShopProductPriceReply> GetShopProductPrice(GetByIdInt64Request request, ServerCallContext context)
    {
        if (request.Id <= 0)
            throw GrpcStatuses.GetBadRequestRpcException(nameof(GetByIdInt64Request.Id), "Invalid value");

        var requestHandler = context.GetHttpContext().RequestServices.GetRequiredService<IRequestHandler<GetByIdRequest<long,ShopProductPrice>, ShopProductPrice>>();
        var shopProductPrice = await requestHandler.Handle(new GetByIdRequest<long, ShopProductPrice>(request.Id), context.CancellationToken)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Shop product price id={request.Id}' not found")); ;
        var reply = shopProductPrice.Adapt<ShopProductPriceReply>();
        reply.Id = shopProductPrice.Id;
        return await Task.FromResult(reply);
    }

    public override async Task<PurposeTypeListReply> GetProductPurposes(GetProductPurposesRequest request, ServerCallContext context)
    {
        if (request.Productid <= 0)
            throw GrpcStatuses.GetBadRequestRpcException(nameof(GetProductPurposesRequest.Productid), "Invalid value");

        var requestHandler = context.GetHttpContext().RequestServices.GetRequiredService<IRequestHandler<GetProductPurposeTypesRequest, List<PurposeType>>>();
        var purposeTypes = await requestHandler.Handle(new GetProductPurposeTypesRequest(request.Productid), context.CancellationToken);
        
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
        var commandHandler = context.GetHttpContext().RequestServices.GetRequiredService<ICommandHandler<SetProductPurposeCommand>>();
        await commandHandler.Handle(new SetProductPurposeCommand(productPurpose), context.CancellationToken);
        var reply = new ProductPurposeReply { Productid = productPurpose.ProductId, Purposetypeid = productPurpose.PurposeTypeId };

        return await Task.FromResult(reply);
    }
}