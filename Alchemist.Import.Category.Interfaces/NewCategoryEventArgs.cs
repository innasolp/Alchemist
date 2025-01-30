namespace Alchemist.Import.Category.Interfaces;

public class NewCategoryEventArgs(ICategory category) : EventArgs
{
    public ICategory NewCategory { get; } = category;
}
