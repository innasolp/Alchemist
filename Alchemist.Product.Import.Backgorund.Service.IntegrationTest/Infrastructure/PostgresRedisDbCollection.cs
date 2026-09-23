using Test.DbContainer.Abstractions;
using Test.PostresqlTestContainer;
using Test.RedisTestContainer;

namespace Alchemist.Product.Import.Backgorund.Service.IntegrationTest.Infrastructure;

[CollectionDefinition("PostgresRedisDbCollection")]
public class PostgresRedisDbCollection : ICollectionFixture<DbTestContainerFixture<PostgresqlTestDbContainer>>, 
    ICollectionFixture<DbTestContainerFixture<RedisTestDbContainer>>
{ }