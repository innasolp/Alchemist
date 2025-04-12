using Alchemist.Product.Data;

namespace Alchemist.Product.Import.WebApp.Test;

public class SettingsAPIWebAppFactory(string dbConnectionString, bool ensureDeleted)
    : AlchemyDbAPIWebAppFactory<SettingsAPIProgram>(dbConnectionString, ensureDeleted)
{
    protected override void FillTestData(AlchemyContext dbContext)
    {
        //todo
    }
}
