using Alchemist.Product.Data;
using Db.Infrastructure;
using Microsoft.EntityFrameworkCore;
using ShopSettings.Data.Infrastructure;

namespace ShopSettings.Data.infrastructure.EF;

public sealed class SaveShopSettingsCommandHandler(AlchemyContext context, IUnitOfWork unitOfWork) : TransactionalCommandHandler<SaveShopSettingsCommand>(unitOfWork)
{
    private readonly AlchemyContext _context = context;

    protected override async Task HandleCommand(SaveShopSettingsCommand command, CancellationToken cancellationToken)
    {
        var entity = command.ShopSettings;
        var shopSettingsId = entity.Id;

        // set existing settings not actual
        await _context.ShopSettings.Where(s => s.ShopId == entity.ShopId && s.Id != entity.Id && s.Type == entity.Type && s.Type != ShopSettingType.Service && s.IsActual != false)
            .ExecuteUpdateAsync(settings => settings.SetProperty(s => s.IsActual, s => false), cancellationToken);

        entity.IsActual = true;

        var result = await _context.ShopSettings.Where(s => s.Id == entity.Id)
            .ExecuteUpdateAsync(settings => settings
                .SetProperty(s => s.IsActual, s => entity.IsActual)
                .SetProperty(s => s.JsonValue, s => entity.JsonValue)
                .SetProperty(s => s.ParentSettingsId, s => entity.ParentSettingsId)
                .SetProperty(s => s.Type, s => entity.Type)
                .SetProperty(s => s.ShopId, s => entity.ShopId)
                .SetProperty(s => s.Name, s => entity.Name), cancellationToken);

        Alchemist.Product.Data.ShopSettings saved;
        if (result > 0)
        {
            saved = entity;
        }
        else
        {
            await _context.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            saved = entity;
        }
    }
}