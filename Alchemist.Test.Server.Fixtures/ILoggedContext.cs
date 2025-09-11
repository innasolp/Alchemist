using Alchemist.Test.Log;

namespace Alchemist.Test.Server.Fixtures;

public interface ILoggedContext
{
    FixtureLogContext FixtureLoggingContext { get; }
}
