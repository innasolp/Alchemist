using Autofac;
using Db.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ShopImport.Service.Hangfire;

public class ShopImportHangfireModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterAssemblyTypes(typeof(ShopImportHangfireModule).Assembly)
            .Where(t => t.Name.EndsWith("RequestHandler") || t.Name.EndsWith("CommandHandler"))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();
    }
}