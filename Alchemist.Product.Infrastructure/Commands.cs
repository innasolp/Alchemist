using Alchemist.Product.Data;
using Mediator.Infrastructure;

namespace Alchemist.Product.Infrastructure;

public class SetProductComponentCommand(ProductComponent entity) : Command<ProductComponent>(entity);

public class SetProductPurposeCommand(ProductPurpose entity) : Command<ProductPurpose>(entity);