using Log.Interceptors;
using Microsoft.Extensions.Logging;

namespace Alchemist.Import.Logging.Factory;

internal class LogEmptyInterceptorImpl<T>(ILogger logger) : LogInterceptor(logger), ILogger<T>
{
}
