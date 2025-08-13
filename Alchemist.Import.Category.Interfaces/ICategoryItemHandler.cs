using Alchemist.Common;

namespace Alchemist.Import.Category.Interfaces;

public interface IImportCategory
{
    ICategory Category { get; }

    ICategoryShopModel CategoryShopModel { get; }
}

public interface ICategoryItemHandler : IItemHandler<IImportCategory, ResultStatus>
{
}
