using Alchemist.Test.Log;
using Http.Info;

namespace Shop.API.Test.Infrastructure;

public class MiddlewareFixtureLogContext<TController>
    : FixtureLoggerContext<InfoLogMiddleware<TController>>
{
}
