using Alchemist.Product.Data;
using Alchemist.Product.Data.Postgresql;
using Alchemist.Test.DBApiWebAppFactory;
using Alchemist.Test.Server.Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.Product.Import.DBService.Test;

public class GrpcServiceWebAppFactory(string connectionString) : DbAPIWebAppFactory<GrpcServiceProgramm, AlchemyContext>(false)
{
    private readonly string _connectionString = connectionString;

    public event Action<IServiceCollection> ConfigureServices;

    protected override IServiceCollection AddDbContext(IServiceCollection services)
    {
        return services.AddDbContextFactory<AlchemyContext, AlchemyContextPostgresFactory>(optionsBuilder => optionsBuilder.UseNpgsql(_connectionString));
    }

    protected override void FillTestData(AlchemyContext dbContext)
    {
        //todo
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureServices((context, services) =>
        {
            context.SetKestrelLocalhostPortsConfig(8070, 8071);

            ConfigureServices?.Invoke(services);
        });
    }
}