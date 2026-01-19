using Alchemist.Product.Data;
using Mediator.Infrastructure.Command;

namespace Alchemist.Product.Infrastructure;

public class SetProductComponentCommand(ProductComponent entity) : Command<ProductComponent>(entity);

public class SetProductPurposeCommand(ProductPurpose entity) : Command<ProductPurpose>(entity);