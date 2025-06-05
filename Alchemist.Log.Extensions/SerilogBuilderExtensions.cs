using Alchemist.Common;
using Serilog;
using Serilog.Configuration.Extensions;

namespace Alchemist.Log.Extensions;

public static class SerilogBuilderExtensions
{
    public static void AddServiceBaseConfigs(this SerilogConfigurationBuilder serilogConfigurationBuilder, 
        string logContextPath, 
        string logPath, 
        string serviceName)
    {
        serilogConfigurationBuilder.AddSourceContextContainsLogConfig(logContextPath, $"{logPath}/{serviceName}", serviceName);
        serilogConfigurationBuilder.AddSourceContextContainsLogConfig(logContextPath, $"{logPath}/system", "Microsoft");
        serilogConfigurationBuilder.AddSourceContextContainsLogConfig(logContextPath, $"{logPath}/net.http", "System.Net.Http");
    }

    public static LoggerConfiguration AddPerfomanceCounter(this SerilogConfigurationBuilder serilogConfigurationBuilder,
        string logContextPath,
        string logPath,
        string url, 
        int eventId,
        string serviceName)
    {
        var expressions = new List<SerilogPropertyExpression>
            {
                {new SerilogPropertyExpression(SerilogFunc.Contains, [new ContextProperty("Url"), url]) },
                {new SerilogPropertyExpression("=", [SerilogExpressions.EventId, eventId]) },
                {new SerilogPropertyExpression(SerilogFunc.Contains, [SerilogExpressions.SourceContext, "Perfomance"]) },
            };

        return serilogConfigurationBuilder.AddPropertiesLogConfig(
            logContextPath,
            Utils.CombinePath(logPath, $"Perfomance/{serviceName}"),            
            outputs: ["Url"],
            expressions:expressions);        
    }
}
