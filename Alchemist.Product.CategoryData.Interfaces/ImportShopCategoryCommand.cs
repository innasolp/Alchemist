using Db.Infrastructure.Commands;

namespace Alchemist.Product.CategoryData;

public class ImportShopCategoryCommand(CategoryData entity) : Command<CategoryData>(entity) { }