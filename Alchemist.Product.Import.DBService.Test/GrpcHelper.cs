using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Alchemist.Product.Import.DBService.Test;

internal static class GrpcHelper
{
    public static GrpcChannel CreateChannel<TEntryPoint>(this WebApplicationFactory<TEntryPoint> webApplicationFactory, string url)
        where TEntryPoint : class
    {
        try
        {
            var httpMessageHandler = webApplicationFactory.Server.CreateHandler();

            return GrpcChannel.ForAddress(url, new GrpcChannelOptions
            {
                HttpHandler = httpMessageHandler
            });
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}
