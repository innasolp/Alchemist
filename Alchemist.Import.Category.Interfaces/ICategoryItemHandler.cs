using Import.Interfaces;

namespace Alchemist.Import.Category.Interfaces;

public interface IImportCategory
{
    ICategory Category { get; }

    string CategorySourceUrl { get; }

    string SourceName { get; }

    string SourceUrl { get; }
}

public interface ICategoryItemHandler : IItemHandler<IImportCategory, ResultStatus>
{
}
