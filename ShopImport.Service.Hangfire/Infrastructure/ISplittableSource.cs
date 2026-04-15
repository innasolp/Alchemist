using Import.Settings.Interfaces;


namespace ShopImport.Service.Hangfire.Infrastructure;

internal interface ISplittableSource
{
    IEnumerable<IImportSource> Split();
}

internal interface ISplittableSource<TSource>
    where TSource : IImportSource
{
    IEnumerable<TSource> Split();
}