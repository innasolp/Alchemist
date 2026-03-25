using Import.Factory.Interfaces;
using Import.Settings.Interfaces;

namespace ShopImport.Service.Hangfire.Infrastructure;

internal interface IImportServiceJobFactory
{
    IImportServiceJob CreateServiceJob<TImportSource, TImportSourceItem>(IImportServiceFactory importServiceFactory,
         IImportSettings importSettings,
         string name,
         TImportSource source,
         bool isAggregate = false,
         Guid? parentId = null)
          where TImportSource :
         IImportSource,
         ISplittableSource<TImportSource>,
         IIdentificableSource,
         ISourceItemCollection<TImportSource, TImportSourceItem>;
}