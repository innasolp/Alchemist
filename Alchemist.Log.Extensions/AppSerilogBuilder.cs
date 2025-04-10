using Alchemist.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Configuration.Extensions;

namespace Alchemist.Log.Serilog;

public class AppSerilogBuilder
{
    private readonly IConfigurationBuilder _configurationBuilder = new ConfigurationBuilder();

    public LoggerConfiguration LoggerConfiguration { get; }

    private const string LogPropertyFile = "log.property.json";

    private readonly string _logPath;

    private const string SourceContextParam = "SourceContext";

    private const string ContainsFunc = "Contains";

    private readonly string _logContextRootPath;

    public AppSerilogBuilder(IHostApplicationBuilder builder)
        : this(builder, Utils.GetAppPath(), builder.Environment.ContentRootPath)
    {
    }

    public AppSerilogBuilder(IHostApplicationBuilder builder, string logContextRootPath)
        : this(builder, Utils.GetAppPath(), logContextRootPath)
    {
    }


    public AppSerilogBuilder(IHostApplicationBuilder builder, string appPath, string logContextRootPath)
        : this(builder.Configuration, appPath, logContextRootPath)
    {
    }

    public AppSerilogBuilder(IConfiguration configuration, string appPath, string logContextRootPath)
    {
        LoggerConfiguration = new LoggerConfiguration().ReadFrom.Configuration(configuration);
        _logPath = Utils.CombinePath($"{appPath}/Logs");
        _logContextRootPath = logContextRootPath;
    }

    public AppSerilogBuilder(IConfiguration configuration, IHostEnvironment env)
        : this(configuration, Utils.GetAppPath(), env.ContentRootPath)
    {
    }

    public LoggerConfiguration AddSystemLogConfig(string logPath, string logContextPath)
    {
        return LoggerConfiguration.SetSystemsSerilogConfig(_configurationBuilder, logContextPath, logPath);
    }

    public LoggerConfiguration AddPropertyLogConfig(string logPath, string logContextPath, string? func, string? propertyName, string? propertyValue)
    {
        return LoggerConfiguration.SetSerilogConfigForServiceByFunc(_configurationBuilder, logContextPath, logPath, func, propertyName, propertyValue);
    }

    public LoggerConfiguration AddPropertyLogConfig(string logPath, string? func, string? propertyName, string? propertyValue)
    {
        var logContextPath = Utils.CombinePath(_logContextRootPath, LogPropertyFile);
        return LoggerConfiguration.SetSerilogConfigForServiceByFunc(_configurationBuilder, logContextPath, logPath, func, propertyName, propertyValue);
    }

    public LoggerConfiguration AddPropertiesLogConfig(string logPath, IEnumerable<SerilogPropertyExpression> expressions)
    {
        var logContextPath = Utils.CombinePath(_logContextRootPath, LogPropertyFile);
        return LoggerConfiguration.SetSerilogConfigProperties(_configurationBuilder, logContextPath, logPath, expressions);
    }

    public LoggerConfiguration AddSourceContextLogConfig(string logContextPath, string logPath, string? propertyValue)
    {
        return LoggerConfiguration.SetSerilogConfigForServiceByFunc(_configurationBuilder, logContextPath, logPath, ContainsFunc, SourceContextParam, propertyValue);
    }
    public LoggerConfiguration AddSourceContextLogConfig(string logPath, string? propertyValue)
    {
        var logContextPath = Utils.CombinePath(_logContextRootPath, LogPropertyFile);
        return LoggerConfiguration.SetSerilogConfigForServiceByFunc(_configurationBuilder, logContextPath, logPath, ContainsFunc, SourceContextParam, propertyValue);
    }

    public LoggerConfiguration AddSourceContextLogConfig(string? propertyValue)
    {
        var logContextPath = Utils.CombinePath(_logContextRootPath, LogPropertyFile);
        return LoggerConfiguration.SetSerilogConfigForServiceByFunc(_configurationBuilder, logContextPath, _logPath, ContainsFunc, SourceContextParam, propertyValue);
    }

    public LoggerConfiguration AddClassNameLogConfig(string logContextPath, string logPath, string classNameValue)
    {
        return LoggerConfiguration.SetSerilogConfigForServiceByFunc(_configurationBuilder, logContextPath, logPath, ContainsFunc, "ClassName", classNameValue);
    }
    public LoggerConfiguration AddClassNameLogConfig(string logPath, string classNameValue)
    {
        var logContextPath = Utils.CombinePath(_logContextRootPath, LogPropertyFile);
        return AddClassNameLogConfig(logContextPath, logPath, classNameValue);
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
}
