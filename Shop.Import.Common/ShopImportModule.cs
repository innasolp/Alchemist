using Autofac;

namespace Shop.Import.Common;

public class ShopImportModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType(typeof(ShopCachedRepository)).As(typeof(IShopCachedRepository));
    }
}