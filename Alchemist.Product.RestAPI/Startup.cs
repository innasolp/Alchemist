using Alchemist.DataService.Interfaces;
using Alchemist.Product.Data.Repository;
using Alchemist.Product.RestAPI.Controllers;
using Http.ErrorHandling;
using Http.Info;
using Http.RequestHandling.PerfomanceCounter;

using Serilog.Loggers;

namespace Alchemist.Product.RestAPI;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {       
        services.AddScoped<IAlchemyRepository, AlchemyRepository>();

        services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddAuthentication("https");

        services.AddExceptionHandler<GlobalExceptionHandler<ShopController>>();
        services.AddSingleton<InfoLogMiddleware<ShopController>>();
        services.AddProblemDetails();

        services.AddPerfomanceCounter<InfoLogMiddleware<ShopController>>((logger) => new SerilogUrlLogger<PerfomanceCounter<InfoLogMiddleware<ShopController>>>(logger));

    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseExceptionHandler();
        app.UseMiddleware<InfoLogMiddleware<ShopController>>();

        app.UseRouting();
        app.UseAuthorization();
        app.UseEndpoints(ep => ep.MapControllers());
        
        app.UsePerfomanceCounters();

        app.UseAuthentication();

        // Configure the HTTP request pipeline.
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseSwagger(options =>
            {
                options.SerializeAsV2 = true;
            });
        }

        app.UseHsts();

        app.UseHttpsRedirection();
    }
}
