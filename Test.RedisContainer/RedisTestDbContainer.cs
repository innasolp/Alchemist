using DotNet.Testcontainers.Containers;
using Test.DbContainer.Abstractions;
using Testcontainers.Redis;

namespace Test.RedisTestContainer;

public class RedisTestDbContainer : TestDbContainer
{
    private RedisContainer? _redisContainer;

    protected override DockerContainer? DockerContainer => _redisContainer;

    public override void Build(string host, int port = 6479, string user = "default", string password = "p@ssw0rd")
    {
        _redisContainer = new RedisBuilder("redis:7.4-alpine").Build();
    }

    public override string BuildConnectionString(string dataBase, int port)
    {
        if (_redisContainer == null)
            throw new InvalidOperationException("RedisTestDbContainer not built yet.");

        return $"{ _redisContainer.GetConnectionString()},defaultDatabase={dataBase}";
    }
}