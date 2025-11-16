using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Alchgemist.Product.ImportSettingsWebApp.UnitTests.Infrastructure;

public abstract class ControllerTest<TController> where TController :  Controller
{
    protected readonly Mock<HttpContext> HttpContextMock = new();

    private readonly Mock<ISession> SessionMock = new();

    private readonly Dictionary<string, byte[]> _sessionData = [];

    public ControllerTest()
    {
        HttpContextMock.Setup(c => c.Session).Returns(SessionMock.Object);

        byte[]? bytes;
        SessionMock.Setup(s => s.TryGetValue(It.IsAny<string>(), out bytes)).Returns((string key, out byte[]? result) =>
        {
            return _sessionData.TryGetValue(key, out result);
        });
        SessionMock.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>())).Callback((string key, byte[] value) =>
        {
            if (!_sessionData.ContainsKey(key)) _sessionData.TryAdd(key, value);
            else _sessionData[key] = value;
        });    
        SessionMock.Setup(s=>s.Keys).Returns(_sessionData.Keys);
    }   

    protected abstract TController CreateController();
}
