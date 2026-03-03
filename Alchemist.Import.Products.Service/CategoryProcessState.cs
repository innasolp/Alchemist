namespace Alchemist.Import.Products.Service;

public class CategoryProcessState
{
    public int SuccessProductCount { get; private set; }

    public int UnsuccessProductCount { get; private set; }

    public int AttemptsCount { get; private set; }

    public int Page { get; private set; } = 1;

    public int AllProductCount => SuccessProductCount + UnsuccessProductCount;

    public string? CategoryPagePath { get; set; }

    public string? CategoryPath { get; private set; } 

    private CategoryProcessState(int successProductCount, int unsuccessProductCount, int attemptsCount, int page)
    {
        SuccessProductCount = successProductCount;
        UnsuccessProductCount = unsuccessProductCount;
        AttemptsCount = attemptsCount;
        Page = page;
    }    

    public static CategoryProcessState Start()
    {
        return new CategoryProcessState(0, 0, 0, 1);
    }

    public static CategoryProcessState Start(string categoryPath)
    {
        return new CategoryProcessState(0, 0, 0, 1) { CategoryPath = categoryPath };
    }

    public void Reset()
    {
        SuccessProductCount = 0;
        UnsuccessProductCount = 0;
        AttemptsCount = 0;
        Page = 0;
        CategoryPagePath = null;
    }

    public void ResetAttempts()
    {
        AttemptsCount = 0;
    }

    public void IncrementAttempts()
    {
        AttemptsCount++;
    }

    public void ApplyPageIterationResult(int? iterationSuccessCount, int? iterationFullCount)
    {
        SuccessProductCount += iterationSuccessCount ?? 0;
        UnsuccessProductCount += iterationSuccessCount != null && iterationFullCount != null 
            ? iterationFullCount.Value - iterationSuccessCount.Value 
            : 0;
    }

    public void IncrementPage()
    {
        Page++;
    }
}