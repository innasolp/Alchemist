using Alchemist.Product.Data;
using Db.Infrastructure.Commands;

namespace Product.Data.Infrastructure;

public class SetProductComponentCommand(ProductComponent entity) : Command<ProductComponent>(entity);

public class SetProductPurposeCommand(ProductPurpose entity) : Command<ProductPurpose>(entity);