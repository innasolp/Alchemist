using Alchemist.Common;
using Alchemist.Log.Serilog;
using Alchemist.Product.SignalR;
using Http.ErrorHandling;
using Http.Info;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("https");

builder.Services.AddSignalR();      

builder.Services.AddTransient<ErrorHandlerMiddleware<EventHub>>();
builder.Services.AddSingleton<InfoLogMiddleware<EventHub>>();

var logPath = $"{Utils.GetAppPath()}/Logs";
var serviceName = "SignalR";
var appSerilogBuilder = new AppSerilogBuilder(builder);
appSerilogBuilder.AddServiceBaseConfigs(serviceName);
appSerilogBuilder.AddSourceContextLogConfig($"{logPath}/{serviceName}", typeof(InfoLogMiddleware<>).GetNameWithoutGenericArity());
appSerilogBuilder.AddSourceContextLogConfig($"{logPath}/{serviceName}", typeof(GlobalExceptionHandler<>).GetNameWithoutGenericArity());
appSerilogBuilder.SetSerilog();

var app = builder.Build();

app.UseMiddleware<ErrorHandlerMiddleware<EventHub>>();
app.UseMiddleware<InfoLogMiddleware<EventHub>>();

app.UseAuthentication();

app.MapGet("/", () => "Hello World!");

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapHub<EventHub>("/events");
app.MapHub<ImportHub>("/import");

app.UseHttpsRedirection();

app.UseAuthorization();

app.Run();
