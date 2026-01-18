using Alchemist.Product.Data;
using Alchemist.Product.Interfaces;
using Alchemist.Test.Log;
using Alchemist.Test.SettingsAPIFactory;
using Alchemist.Test.SignalRWebAppFactory;
using Microsoft.AspNetCore.TestHost;


namespace Alchemist.Product.WebApp.Test.Infrastructure;

internal static class Common
{
    private static TestServer? _signalRTestServer;

    public static TestServer SignalRTestServer
    {
        get
        {
            _signalRTestServer ??= new SignalRLogContextWebAppFactory<FixtureLoggerFactoryContext>().Server;

            return _signalRTestServer;
        }
    }

    internal static void FillTestData(AlchemyContext dbContext, int[] shopIds)
    {
        for (var i = 0; i < shopIds.Length - 1; i++)
        {
            var shopId = shopIds[i];
            var productSettings = SettingsTestRepository.CreateProductShopSettings(shopId);
            var productSettingsEntry = dbContext.ShopSettings.Add(productSettings);

            var categorySettings = SettingsTestRepository.CreateCategoryShopSettings(shopId);
            var categorySettingsEntry = dbContext.ShopSettings.Add(categorySettings);

            dbContext.SaveChanges();

            var productServices = SettingsTestRepository.CreateShopSettingsServicesTestData(productSettingsEntry.Entity);
            productServices.ForEach(s => dbContext.ShopSettings.Add(s));

            var categoryServices = SettingsTestRepository.CreateShopSettingsServicesTestData(categorySettingsEntry.Entity);
            categoryServices.ForEach(s => dbContext.ShopSettings.Add(s));

            dbContext.SaveChanges();
        }
    }
}