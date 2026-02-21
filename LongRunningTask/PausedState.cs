using Hangfire.States;

namespace LongRunningTask;

public class PausedState : IState
{
    //todo add queue property

    private readonly static string StateName = "Paused";

    public string Name => StateName;

    public string Reason { get; set; }

    public bool IsFinal => false;

    public bool IgnoreJobLoadException => false;

    public Dictionary<string, string> SerializeData() => [];
}