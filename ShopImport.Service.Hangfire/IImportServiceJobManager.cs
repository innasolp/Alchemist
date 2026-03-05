namespace ShopImport.Service.Hangfire;

public interface IImportServiceJobManager
{
    Task Execute(Guid guid, CancellationToken cancellationToken);
}