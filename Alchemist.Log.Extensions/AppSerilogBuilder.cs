using Alchemist.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Configuration.Extensions;

namespace Alchemist.Log.Serilog;

public class AppSerilogBuilder(IConfiguration configuration, string appPath, string logContextRootPath)
{
    private readonly IConfigurationBuilder _configurationBuilder = new ConfigurationBuilder();

    public LoggerConfiguration LoggerConfiguration { get; } = new LoggerConfiguration().ReadFrom.Configuration(configuration);

    private const string LogPropertyFile = "log.property.json";

    private readonly string _logPath = Utils.CombinePath($"{appPath}/Logs");

    private const string SourceContextParam = "SourceContext";

    private const string ContainsFunc = "Contains";

    private readonly string _logContextRootPath = logContextRootPath;

    public AppSerilogBuilder(IConfiguration configuration, IHostEnvironment env)
        : this(configuration, Utils.GetAppPath(), env.ContentRootPath)
    {
    }

    public LoggerConfiguration AddPropertiesLogConfig(string logPath, IEnumerable<SerilogPropertyExpression> expressions)
    {
        var logContextPath = Utils.CombinePath(_logContextRootPath, LogPropertyFile);
        return LoggerConfiguration.SetSerilogConfigProperties(_configurationBuilder, logContextPath, logPath, expressions);
    }

    public LoggerConfiguration AddSourceContextContainsLogConfig(string logPath, string? propertyValue)
    {
        var logContextPath = Utils.CombinePath(_logContextRootPath, LogPropertyFile);
        return LoggerConfiguration.SetSerilogConfigForServiceByFunc(_configurationBuilder, logContextPath, logPath, ContainsFunc, SourceContextParam, propertyValue);
    }             

    public LoggerConfiguration AddServiceBaseConfigs(string logPath, string serviceName)
    {
        var logPropertyPath = Utils.CombinePath(_logContextRootPath, LogPropertyFile);

        var logContextServicePath = Utils.CombinePath(logPath, serviceName);

        LoggerConfiguration.SetSerilogConfigForServiceByFunc(_configurationBuilder,
            logContextPath: logPropertyPath,
            logPath: logContextServicePath,
            func: ContainsFunc,
            propertyName: SourceContextParam,
            propertyValue: serviceName);


        return LoggerConfiguration.SetSystemsSerilogConfig(_configurationBuilder,
              logContextPath: logPropertyPath,
              logsPath: logContextServicePath,
              func: ContainsFunc);
    }

    public LoggerConfiguration AddServiceBaseConfigs(string serviceName)
    {
        return AddServiceBaseConfigs(_logPath, serviceName);
    }

    public ILoggingBuilder SetSerilog(ILoggingBuilder loggingBuilder)
    {
        loggingBuilder.ClearProviders();
        var logger = LoggerConfiguration.CreateLogger();
        return loggingBuilder.AddSerilog(logger);
    }

    public LoggerConfiguration AddPerfomanceCounter(string url, int eventId, string logPath, string serviceName)
    {
        var expressions = new List<SerilogPropertyExpression>
            {
                {new SerilogPropertyExpression(SerilogExpressions.Contains, "Url", url) },
                {new SerilogPropertyExpression(SerilogExpressions.EventId, eventId) },
            };
        return AddPropertiesLogConfig(Utils.CombinePath(logPath, $"Perfomance/{serviceName}"), expressions);
    }
    

    public LoggerConfiguration AddContextPropertyConfig(string logContextPath, 
        string logPath,
        string propertyName,
        string? sourceContext = null,
        string[]? outputProperties = null,
        IEnumerable<SerilogPropertyExpression>? expressions = null)
    {
        var conf = _configurationBuilder.AddJsonFile(logContextPath).Build();

        conf.SetSerilogLoggersPath(["path", "pathFormat"], logPath);

        conf.SetSerilogWriteToContextPropertyName(propertyName);

        if(outputProperties != null)
            conf.SetSerilogOutputTemplateProperties(outputProperties);

        if (!string.IsNullOrEmpty(sourceContext)) conf.SetSerilogLoggersFilterSourceContext(sourceContext);

        if (expressions != null)
            foreach (var expression in expressions)
                conf.AddSerilogExpressionFilter(expression);

        return LoggerConfiguration.ReadFrom.Configuration(conf);
    }
}
