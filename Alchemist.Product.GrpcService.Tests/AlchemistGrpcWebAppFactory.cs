using Alchemist.DataService.Interfaces;
using Alchemist.Product.Data;
using Alchemist.Product.Data.Repository;
using Alchemist.Test.Server.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace Alchemist.Product.GrpcService.Tests;

public class AlchemistGrpcWebAppFactory : AlchemistWebAppFactory<Program, AlchemyContext>
{
    protected override void FillTestData(AlchemyContext dbContext)
    {
        //todo
        throw new NotImplementedException();
    }

    protected override void RegisterServices(IServiceCollection services)
    {
        services.AddScoped<IAlchemyRepository, AlchemyRepository>();       
    }
}
