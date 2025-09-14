using Alchemist.Test.Log;
using Http.Info;
using Http.RequestHandling.PerfomanceCounter;

namespace Alchemist.Product.RestAPI.Test.Infrastructure;

public class PerfomanceCounterMiddlewareFixtureLogContext<TController>
    : FixtureLoggerContext<PerfomanceCounter<InfoLogMiddleware<TController>>>
{
}
