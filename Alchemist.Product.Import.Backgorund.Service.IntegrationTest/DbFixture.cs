using Test.PostresqlTestContainer;

namespace Alchemist.Product.Import.Backgorund.Service.IntegrationTest;

public class DbFixture : IAsyncLifetime
{
    public PostgresqlTestDbContainer Container { get; } = new PostgresqlTestDbContainer();

    public async Task InitializeAsync()
    {
        Container.Build(Guid.NewGuid().ToString());
        await (Container as IAsyncLifetime).InitializeAsync();
    }

    public async Task DisposeAsync() => await (Container as IAsyncLifetime).DisposeAsync();
}

// 2. Связываем коллекцию тестов с фикстурой
[CollectionDefinition("DbCollection")]
public class DbCollection : ICollectionFixture<DbFixture> { }
