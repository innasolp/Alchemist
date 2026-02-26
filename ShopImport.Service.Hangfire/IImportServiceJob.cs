namespace ShopImport.Service.Hangfire;

public interface IImportServiceJob
{
    Task Execute(Guid guid, CancellationToken cancellationToken);
}