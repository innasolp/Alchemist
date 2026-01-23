using Mediator.Messages.Events;

namespace ShopSettings.Infrastructure;

public class CreateShopSettingsEvent(Alchemist.Product.Data.ShopSettings entity, DateTime creationDate)
    : CreationEvent<Alchemist.Product.Data.ShopSettings>(Messages.ShopSettingsCreated, entity, creationDate)
{
}