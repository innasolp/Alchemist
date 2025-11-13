using Alchemist.Product.WebApp.Controllers;
using Alchemist.Product.WebApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Alchemist.Product.WebApp.UnitTests;

internal class TestSession : ISession
{
    private readonly ConcurrentDictionary<string, byte[]> _store = new();

    public IEnumerable<string> Keys => _store.Keys;

    public string Id { get; } = Guid.NewGuid().ToString();

    public bool IsAvailable { get; } = true;

    public void Clear() => _store.Clear();

    public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public void Remove(string key) => _store.TryRemove(key, out _);

    public void Set(string key, byte[] value) => _store[key] = value;

    public bool TryGetValue(string key, out byte[] value) => _store.TryGetValue(key, out value);
}

public class HomeControllerTests
{
    private static DefaultHttpContext CreateHttpContext()
    {
        var ctx = new DefaultHttpContext();
        ctx.Features.Set<ISessionFeature>(new SessionFeature { Session = new TestSession() });
        ctx.Request.Headers["Host"] = "localhost";
        return ctx;
    }

    [Fact]
    public void Index_Redirects_To_ShopRoot()
    {
        var ctx = CreateHttpContext();
        var controller = new HomeController(MockLogger());
        controller.ControllerContext.HttpContext = ctx;

        var result = controller.Index();

        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/Shop/", redirect.Url);
    }

    [Fact]
    public void IndexShops_Sets_AppUrl_And_Returns_IndexView_With_Model()
    {
        var ctx = CreateHttpContext();
        ctx.Request.Path = "/Shop/5";
        var controller = new HomeController(MockLogger());
        controller.ControllerContext.HttpContext = ctx;

        var result = controller.IndexShops(5);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("~/Views/Home/Index.cshtml", view.ViewName);
        var model = Assert.IsType<IndexModel>(view.Model);
        Assert.Equal(Tab.Shop, model.Tab);
        Assert.Equal("shopapp", model.AppName);
        Assert.NotNull(model.AppUrls);
        Assert.True(model.AppUrls.ContainsKey("shopapp"));
        Assert.Equal("/Shop/5", model.AppUrls["shopapp"] == "/Shop/5" ? "/Shop/5" : model.AppUrls["shopapp"]); // ensures key exists
        Assert.Equal(5, (int?)model.Data.GetType().GetProperty("ShopId")!.GetValue(model.Data));
    }

    [Fact]
    public void NewShop_Sets_AppUrl_And_Returns_IndexView_With_ShopIdZero()
    {
        var ctx = CreateHttpContext();
        ctx.Request.Path = "/Shop/New";
        var controller = new HomeController(MockLogger());
        controller.ControllerContext.HttpContext = ctx;

        var result = controller.NewShop();

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("~/Views/Home/Index.cshtml", view.ViewName);
        var model = Assert.IsType<IndexModel>(view.Model);
        Assert.Equal(Tab.Shop, model.Tab);
        Assert.Equal("shopapp", model.AppName);
        Assert.Equal(0, (int?)model.Data.GetType().GetProperty("ShopId")!.GetValue(model.Data));
    }

    [Fact]
    public void IndexImportSettings_Sets_AppUrl_And_Returns_IndexView_With_ImportSettingsTab()
    {
        var ctx = CreateHttpContext();
        ctx.Request.Path = "/Import/Settings";
        var controller = new HomeController(MockLogger());
        controller.ControllerContext.HttpContext = ctx;

        var result = controller.IndexImportSettings(null, null);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("~/Views/Home/Index.cshtml", view.ViewName);
        var model = Assert.IsType<IndexModel>(view.Model);
        Assert.Equal(Tab.ImportSettings, model.Tab);
        Assert.Equal("importsettingsapp", model.AppName);
    }

    [Fact]
    public void Privacy_Returns_ViewResult()
    {
        var controller = new HomeController(MockLogger());
        controller.ControllerContext.HttpContext = CreateHttpContext();

        var result = controller.Privacy();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Error_Returns_ErrorViewModel_With_TraceIdentifier()
    {
        var ctx = CreateHttpContext();
        ctx.TraceIdentifier = "trace-123";
        var controller = new HomeController(MockLogger());
        controller.ControllerContext.HttpContext = ctx;

        var result = controller.Error();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ErrorViewModel>(view.Model);
        Assert.Equal("trace-123", model.RequestId);
        Assert.True(model.ShowRequestId);
    }

    // Minimal logger factory for controller tests
    private static ILogger<HomeController> MockLogger()
    {
        return new LoggerFactory().CreateLogger<HomeController>();
    }

    // session feature helper
    private class SessionFeature : ISessionFeature
    {
        public ISession? Session { get; set; }
    }
}