namespace Alchemist.Product.GrpcService.Tests;

public class AlchemyGrpcServiceIntegrationTest(AlchemistGrpcWebAppFactory webAppFactory) 
    : AlchemistGrpcTestFixture(webAppFactory)
{
    [Fact]
    public async Task GetBrandTest()
    {
        var client = CreateAlchemistGrpcClient();

        var brandName = "Elizavecca";        
        var response = await client.FindBrandByNameAsync(new FindByNameRequest { Name = brandName});

        Assert.NotNull(response);
        Assert.Equal(brandName, response.Name);
    }
}
