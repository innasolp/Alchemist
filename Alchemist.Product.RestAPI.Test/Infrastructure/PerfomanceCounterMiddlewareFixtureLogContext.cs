using Alchemist.Test.Server.Fixtures;
using Http.Info;
using Http.RequestHandling.PerfomanceCounter;

namespace Alchemist.Product.RestAPI.Test.Infrastructure;

public class PerfomanceCounterMiddlewareFixtureLogContext<TController>
    : FixtureLoggerContext<PerfomanceCounter<InfoLogMiddleware<TController>>>
{
}
