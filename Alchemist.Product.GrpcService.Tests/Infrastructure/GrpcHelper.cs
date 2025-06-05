using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Alchemist.Product.GrpcService.Tests.Infrastructure;

internal static class GrpcHelper
{
    public static GrpcChannel CreateChannel<TEntryPoint>(this WebApplicationFactory<TEntryPoint> webApplicationFactory, string url)
        where TEntryPoint : class
    {
        var httpMessageHandler = webApplicationFactory.Server.CreateHandler();

        return GrpcChannel.ForAddress(url, new GrpcChannelOptions
        {
            HttpHandler = httpMessageHandler
        });
    }
}
