using Alchemist.Product.Data;
using Alchemist.Product.Interfaces;
using Alchemist.Test.SettingsAPIFactory;
using Microsoft.AspNetCore.TestHost;

namespace Alchemist.Product.ImportSettingsWebApp.Test.Infrastructure;

internal class TestSettingsApiFactory(string connectionString, TestServer signalRServer, int httpPort, int httpsPort, int[] shopIds, bool ensureDeleted = true)
    : SettingsAPIWebAppFactory(connectionString, signalRServer, httpPort, httpsPort, ensureDeleted)
{
    private readonly int[] _shopIds = shopIds;

    protected override void FillTestData(AlchemyContext dbContext)
    {
        base.FillTestData(dbContext);

        var shopIds = _shopIds.ToList();

        for (var i=0;i< shopIds.Count - 1;i++) 
        {
            var shopId = shopIds[i];
            var productSettings = SettingsTestRepository.CreateProductShopSettings(shopId);
            var productSettingsEntry = dbContext.ShopSettings.Add(productSettings.To<ShopSettings>());

            var categorySettings = SettingsTestRepository.CreateCategoryShopSettings(shopId);
            var categorySettingsEntry = dbContext.ShopSettings.Add(categorySettings.To<ShopSettings>());

            dbContext.SaveChanges();

            var productServices = SettingsTestRepository.CreateShopSettingsServicesTestData(productSettingsEntry.Entity);
            productServices.ForEach(s=>dbContext.ShopSettings.Add(s.To<ShopSettings>()));

            var categoryServices = SettingsTestRepository.CreateShopSettingsServicesTestData(categorySettingsEntry.Entity);
            categoryServices.ForEach(s => dbContext.ShopSettings.Add(s.To<ShopSettings>()));

            dbContext.SaveChanges();
        }
    }
}
