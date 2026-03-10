using Alchemist.Common;
using CustomConfigurationProvider;
using CustomJsonConfigurationProvider;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Configuration.Extensions;
using Serilog.Core;

namespace Alchemist.Log.Extensions;

public static class SerilogConfigurationExtensions
{ 
    public static LoggerConfiguration AddServiceBaseConfigs(this LoggerConfiguration loggerConfiguration, 
        string logContextFile, 
        string logPath, 
        string serviceName)
    {
        var configurationBuilder = new ConfigurationBuilder();

        loggerConfiguration.ReadFrom.Configuration(configurationBuilder.AddBaseSourceContextRules(logContextFile, $"{logPath}/{serviceName}", serviceName));

        loggerConfiguration.ReadFrom.Configuration(configurationBuilder.AddBaseSourceContextRules(logContextFile, $"{logPath}/system", "Microsoft"));

        return loggerConfiguration.ReadFrom.Configuration(configurationBuilder.AddBaseSourceContextRules(logContextFile, $"{logPath}/net.http", "System.Net.Http"));
    }

    public static LoggerConfiguration AddSourceContextConfig(this LoggerConfiguration loggerConfiguration, string logContextFile, string logPath, string serviceName)
    {
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddBaseSourceContextRules(logContextFile, logPath, serviceName);

        var configuration = configurationBuilder.Build();
        return loggerConfiguration.ReadFrom.Configuration(configuration);
    }

    public static LoggerConfiguration AddContextConfig(this LoggerConfiguration loggerConfiguration, string logContextFile)
    {
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddJsonFile(logContextFile);
        var configuration = configurationBuilder.Build();
        return loggerConfiguration.ReadFrom.Configuration(configuration);
    }

    public static LoggerConfiguration AddContextPropertiesConfig(this LoggerConfiguration loggerConfiguration, string logContextFile,
        IDictionary<string, string[]> configurationProperties)
    {
        var configurationBuilder = new ConfigurationBuilder();

        var source = configurationBuilder.AddCustomJsonConfigurationProvider(logContextFile);

        foreach(var configurationProperty in configurationProperties)
        {
            var configPropertiesString = $" {string.Join(" , ", configurationProperty.Value)} ";
            var rule = new SerilogContextPropertyConfigurationRule(configurationProperty.Key, configPropertiesString);
            source.AddCustomConfigurationRule(rule);
        }    

        var configuration = configurationBuilder.Build();
        return loggerConfiguration.ReadFrom.Configuration(configuration);
    }

    public static LoggerConfiguration AddContextPropertyConfig(this LoggerConfiguration loggerConfiguration, 
        string logContextFile,
        string logPath, 
        string contextPropertyName,
        string? sourceContext = null,
        PropertyExpression[]? propertyExpressions = null,
        IEnumerable<string>? outputProperties = null)
    {
        var configurationBuilder = new ConfigurationBuilder();
        var source = configurationBuilder.AddCustomJsonConfigurationProvider(logContextFile);

        source.AddLogPathRule(logPath, [ "path", "pathFormat" ]);
        source.AddContextPropertyNameRule(contextPropertyName);
        if(!string.IsNullOrEmpty(sourceContext)) source.AddSourceContextFilterRule(sourceContext);
        propertyExpressions?.ToList().ForEach(source.AddExpressionFilterRule);
       if(outputProperties != null) source.AddOutputTemplatePropertyRule(outputProperties);

        var configuration = configurationBuilder.Build();
        return loggerConfiguration.ReadFrom.Configuration(configuration);
    }

    private static IConfiguration AddBaseSourceContextRules(this IConfigurationBuilder configurationBuilder, string logContextFile, string logPath, string serviceName)
    {
        var source = configurationBuilder.AddCustomJsonConfigurationProvider(logContextFile);

        source.AddLogPathRule(logPath);
        source.AddContextPropertyNameRule(SerilogExpressions.SourceContext.Name);
        source.AddSourceContextFilterRule(serviceName);

        return configurationBuilder.Build();
    }

    [Obsolete]
    public static LoggerConfiguration AddPerfomanceCounter(this LoggerConfiguration loggerConfiguration,
        string logContextFile,
        string logPath,
        string url, 
        int eventId,
        string serviceName)
    {
        var configurationBuilder = new ConfigurationBuilder();

        var source = configurationBuilder.AddCustomJsonConfigurationProvider(logContextFile);

        source.AddLogPathRule(Utils.CombinePath(logPath, $"Perfomance/{serviceName}"));
        source.AddExpressionFilterRule(new PropertyExpression(SerilogFunc.Contains, [new ContextProperty("Url"), url]));
        source.AddExpressionFilterRule(new PropertyExpression("=", [SerilogExpressions.EventId, eventId]));
        source.AddExpressionFilterRule(new PropertyExpression(SerilogFunc.Contains, [SerilogExpressions.SourceContext, "Perfomance"]));
        source.AddOutputTemplatePropertyRule(["Url"]);

        var configuration = configurationBuilder.Build();
        return loggerConfiguration.ReadFrom.Configuration(configuration);
    }

    public static Serilog.ILogger SetSerilog(this LoggerConfiguration loggerConfiguration, ILoggingBuilder loggingBuilder)
    {
        loggingBuilder.ClearProviders();
        Logger logger = loggerConfiguration.CreateLogger();
        loggingBuilder.AddSerilog(logger);
        return logger;
    }
}