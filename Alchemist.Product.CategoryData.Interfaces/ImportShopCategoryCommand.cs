using Alchemist.Common;
using MediatR;

namespace Alchemist.Product.CategoryData;

public record ImportShopCategoryCommand(CategoryData Category) : IRequest<ItemProcessStatus>;