namespace ShopImport.ServiceState;

public interface IServiceStateRepositoryFactory
{
    IServiceStateRepository Create(params object[] parameters);
}
