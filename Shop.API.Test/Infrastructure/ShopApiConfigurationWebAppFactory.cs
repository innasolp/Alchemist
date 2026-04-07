using Alchemist.Product.Data;
using Alchemist.Test.DBApiWebAppFactory.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Test.PostresqlTestContainer;

namespace Shop.API.Test.Infrastructure;

public abstract class ShopApiConfigurationWebAppFactory(string connectionStringSection, string database, Action<IServiceCollection>? configureServices = null) 
    : DbConfigurationContainerWebAppFactory<ShopAPIProgram, AlchemyContext, PostgresqlTestDbContainer>(connectionStringSection, database, 5432, "postgres", "P@ssw0rd")
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        if(configureServices != null)
            builder.ConfigureTestServices(configureServices);
    }

    protected override void FillTestData(AlchemyContext dbContext)
    {
        dbContext.Shops.Add(new Alchemist.Product.Data.Shop { Name = "TestShop", Url = "https://testshop1" });
        dbContext.SaveChanges();
    }
}