using Mediator.Messages.Events;

namespace Shop.Infrastructure;

public class CreateShopEvent(Alchemist.Product.Data.Shop entity, DateTime creationDate) 
    : CreationEvent<Alchemist.Product.Data.Shop>(Messages.ShopCreated, entity, creationDate)
{
}

public class CreateShopCategoryEvent(Alchemist.Product.Data.ShopCategory entity, DateTime creationDate) 
    : CreationEvent<Alchemist.Product.Data.ShopCategory>(Messages.CategoryAdded, entity, creationDate)
{
}