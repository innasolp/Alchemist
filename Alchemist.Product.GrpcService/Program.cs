using Alchemist.Common;
using Alchemist.Log.Extensions;
using Alchemist.Product.Data;
using Alchemist.Product.Data.Postgresql;
using Alchemist.Product.GrpcService.Services;
using Alchemist.Product.Module;
using CustomConfigurationProvider;
using CustomJsonConfigurationProvider;
using Grpc.Server.Interceptors;
using Grpc.Server.RequestInterceptor;
using Mapster;
using Mediator.Module.EF;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Net;
using System.Reflection;

internal class Program
{
    private static void Main(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.IsTerminating)
            {
                var ex = e.ExceptionObject as Exception;
                Serilog.Log.Fatal(ex, ex?.Message ?? $"Fatal error. {AppDomain.CurrentDomain.FriendlyName} will be terminated.");
            }
        }

        var builder = WebApplication.CreateBuilder(args);

        builder.WebHost.UseKestrel();

        builder.Services.AddMapster();
        TypeAdapterConfig.GlobalSettings.Default.NameMatchingStrategy(NameMatchingStrategy.IgnoreCase);


        builder.Configuration.SetAppSettingsCustomJsonConfigurationProvider();
        builder.Configuration.AddCustomConfigurationRule<CustomJsonConfigurationSource, EnvironmentConfigurationRule>();

        builder.Services.AddDbContextFactory<AlchemyContext, AlchemyContextPostgresFactory>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DbContext")));

        builder.Host.AddMediatorInfrastructure<ProductModule>();

        builder.Services.AddSingleton<ServerLoggingInterceptor<AlchemyService>>();
        builder.Services.AddSingleton<ServerRequestSenderInterceptor<AlchemyService>>();
        
        builder.Services.AddGrpc(options =>
        {
            options.Interceptors.Add<ServerRequestSenderInterceptor<AlchemyService>>();
            options.Interceptors.Add<ServerLoggingInterceptor<AlchemyService>>();
        }).AddJsonTranscoding();
        
        // Register OpenAPI/Swagger support for transcoded gRPC endpoints
        //builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddGrpcSwagger();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "gRPC Product service", Version = "v1" });

            // Include XML comments for .proto and generated types if available
            var xmlFile = $"{Assembly.GetEntryAssembly()?.GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory ?? ".", xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
                // Include gRPC-specific xml comments (extension provided by Microsoft.AspNetCore.Grpc.Swagger)
                c.IncludeGrpcXmlComments(xmlPath, includeControllerXmlComments: true);
            }
        });
        builder.Services.ConfigureSwagger((options) =>
        {
            options.OpenApiVersion = OpenApiSpecVersion.OpenApi2_0;
        });

        AddLogging(builder.Configuration, builder.Logging);

        builder.Services.AddAuthentication("https");

        var app = builder.Build();

        app.UseAuthentication();

        // Configure the HTTP request pipeline.

        if (builder.Environment.IsDevelopment())
        {
            app.UseSwagger();

            app.UseDeveloperExceptionPage();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "gRPC Product service v1");
            });
        }

        app.UseHsts();

        app.UseHttpsRedirection();

        app.UseRouting();
        
        // Configure the HTTP request pipeline.
        app.MapGrpcService<AlchemyService>();
        app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
        
        app.Run();

        static void AddLogging(IConfiguration configuration, ILoggingBuilder loggingBuilder)
        {
            var logPath = $"{Utils.GetAppPath()}/Logs";
            var logContextFile = "log.property.json";
            var loggerConfiguration = new LoggerConfiguration().ReadFrom.Configuration(configuration);

            loggerConfiguration.AddServiceBaseConfigs(logContextFile, logPath, typeof(AlchemyService).Name);
            loggerConfiguration.AddSourceContextConfig(logContextFile, $"{logPath}/{typeof(AlchemyService).Name}", typeof(ServerRequestSenderInterceptor<>).GetNameWithoutGenericArity());
            loggerConfiguration.AddSourceContextConfig(logContextFile, $"{logPath}/{typeof(AlchemyService).Name}", typeof(ServerLoggingInterceptor<>).GetNameWithoutGenericArity());

            loggerConfiguration.SetSerilog(loggingBuilder);
        }
    }
}

public partial class GrpcServiceProgramm
{
}
