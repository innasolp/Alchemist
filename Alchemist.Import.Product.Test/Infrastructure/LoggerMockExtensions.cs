using Microsoft.Extensions.Logging;
using Moq;

namespace Alchemist.Import.Product.Test.Infrastructure;

internal static class LoggerMockExtensions
{
    private static void VerifyMessage(this Mock<ILogger> loggerMock, LogLevel logLevel, Exception? e, string messageFormat, params object[] args)
    {
        var message = string.Format(messageFormat, args ?? []);

        loggerMock.Verify(l => l.Log(
           It.Is<LogLevel>(v => v == logLevel),
           It.IsAny<EventId>(),
           It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
           It.Is<Exception?>(v=> v == null && e == null || v == e),
           It.IsAny<Func<It.IsAnyType, Exception?, string>>()));
    }

    public static void VerifyInfo(this Mock<ILogger> loggerMock, string messageFormat, params object[] args)
    {
        loggerMock.VerifyMessage(LogLevel.Information, null, messageFormat, args);
    }

    public static void VerifyInfo(this Mock<ILogger> loggerMock, string message)
    {
        loggerMock.VerifyInfo(message, null);
    }

    public static void VerifyWarning(this Mock<ILogger> loggerMock, string messageFormat, params object[] args)
    {
        loggerMock.VerifyMessage(LogLevel.Warning, null, messageFormat, args);
    }

    public static void VerifyWarning(this Mock<ILogger> loggerMock, Exception? exception, string messageFormat, params object[] args)
    {
        loggerMock.VerifyMessage(LogLevel.Warning, exception, messageFormat,args);
    }

    public static void VerifyError(this Mock<ILogger> loggerMock, Exception? exception, string messageFormat, params object[] args)
    {
        loggerMock.VerifyMessage(LogLevel.Error, exception, messageFormat, args);
    }
    
    public static void VerifyError(this Mock<ILogger> loggerMock, string messageFormat, params object[] args)
    {
        loggerMock.VerifyMessage(LogLevel.Error, null, messageFormat, args);
    }
}
