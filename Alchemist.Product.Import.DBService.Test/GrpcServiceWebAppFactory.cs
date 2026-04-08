using Alchemist.Product.Data;
using Alchemist.Test.DBApiWebAppFactory.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Test.PostresqlTestContainer;

namespace Alchemist.Product.Import.DBService.Test;

public class GrpcServiceWebAppFactory(string database, int httpPort, int httpsPort) 
    : DbApiAPIKestrelConfigurationContainerWebAppFactory<GrpcServiceProgramm, AlchemyContext, PostgresqlTestDbContainer>
    ("ConnectionStrings:DbContext2", database, 5432, "postgres", "P@ssw0rd", httpPort, httpsPort)
{  
    public event Action<IServiceCollection>? ConfigureServices;

    protected override void FillTestData(AlchemyContext dbContext)
    {}    

    protected override void ConfigureWebHostBuilderContext(WebHostBuilderContext context, IServiceCollection services)
    {
        base.ConfigureWebHostBuilderContext(context, services); 

        ConfigureServices?.Invoke(services);
    }
}