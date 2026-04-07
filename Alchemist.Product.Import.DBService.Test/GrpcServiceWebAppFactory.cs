using Alchemist.Product.Data;
using Alchemist.Product.Data.Postgresql;
using Alchemist.Test.DBApiWebAppFactory.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.Product.Import.DBService.Test;

public class GrpcServiceWebAppFactory(string connectionString) : DbApiAPIKestrelContextContainerWebAppFactory<GrpcServiceProgramm, AlchemyContext>(false, 8070, 8071)
{  
    private readonly string _connectionString = connectionString;

    public event Action<IServiceCollection> ConfigureServices;

    protected override IServiceCollection AddDbContext(IServiceCollection services)
    {
        return services.AddAlchemyPostgresContextFactory(optionsBuilder =>
                optionsBuilder.UseNpgsql(_connectionString));
    }

    protected override void FillTestData(AlchemyContext dbContext)
    {
        //todo
    }    

    protected override void ConfigureWebHostBuilderContext(WebHostBuilderContext context, IServiceCollection services)
    {
        base.ConfigureWebHostBuilderContext(context, services); 

        ConfigureServices?.Invoke(services);
    }
}