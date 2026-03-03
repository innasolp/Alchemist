namespace ShopImport.KeyHash;

public interface IKeyHasher
{
    byte[] Hash<T>(T key);
}