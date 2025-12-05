namespace Alchemist.Import.Products.Service;


public record CountResult(int SuccessCount, int UnsuccessCount)
{
    public bool IsFailed() => SuccessCount == 0;

    public bool IsAnyUnsuccess() => UnsuccessCount == 0;

    public bool IsNotCompletelySuccessful() => UnsuccessCount > 0 && SuccessCount > 0;
}