using Alchemist.Product.Data;
using Db.Infrastructure;
using Microsoft.EntityFrameworkCore;
using ShopSettings.Data.Infrastructure;

namespace ShopSettings.Data.infrastructure.EF;

public sealed class SaveShopSettingsWithChildrenCommandHandler(AlchemyContext context, IUnitOfWork unitOfWork) 
    : TransactionalCommandHandler<SaveShopSettingsWithChildrenCommand>(unitOfWork)
{
    private readonly AlchemyContext _context = context;
    
    protected override async Task HandleCommand(SaveShopSettingsWithChildrenCommand command, CancellationToken cancellationToken)
    {
        var parent = command.ParentShopSettings;
        var children = command.ChildrenSettings;

        var shopSettingsId = parent.Id;

        // set existing parent settings not actual
        await _context.ShopSettings.Where(s => s.ShopId == parent.ShopId && s.Id != parent.Id && s.Type == parent.Type && s.Type != ShopSettingType.Service && s.IsActual != false)
            .ExecuteUpdateAsync(settings => settings.SetProperty(s => s.IsActual, s => false), cancellationToken);

        parent.IsActual = true;

        var result = await _context.ShopSettings.Where(s => s.Id == parent.Id)
            .ExecuteUpdateAsync(settings => settings
                .SetProperty(s => s.IsActual, s => parent.IsActual)
                .SetProperty(s => s.JsonValue, s => parent.JsonValue)
                .SetProperty(s => s.ParentSettingsId, s => parent.ParentSettingsId)
                .SetProperty(s => s.Type, s => parent.Type)
                .SetProperty(s => s.ShopId, s => parent.ShopId)
                .SetProperty(s => s.Name, s => parent.Name), cancellationToken);

        Alchemist.Product.Data.ShopSettings shopSettings;
        if (result > 0)
        {
            shopSettings = parent;
        }
        else
        {
            _context.Add(parent);
            await _context.SaveChangesAsync(cancellationToken);
            shopSettings = parent;
        }

        var handledServices = new List<Alchemist.Product.Data.ShopSettings>();
        foreach (var service in children)
        {
            service.ParentSettingsId = shopSettings.Id;
            service.Type = ShopSettingType.Service;

            var r = await _context.ShopSettings.Where(s => s.Id == service.Id)
                .ExecuteUpdateAsync(settings => settings
                    .SetProperty(s => s.IsActual, s => service.IsActual)
                    .SetProperty(s => s.JsonValue, s => service.JsonValue)
                    .SetProperty(s => s.ParentSettingsId, s => service.ParentSettingsId)
                    .SetProperty(s => s.Type, s => service.Type)
                    .SetProperty(s => s.ShopId, s => service.ShopId)
                    .SetProperty(s => s.Name, s => service.Name), cancellationToken);

            if (r > 0)
                handledServices.Add(service);
            else
            {
                _context.Add(service);
                handledServices.Add(service);
            }
        }
    }
}