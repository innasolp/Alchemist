namespace LongRunningTask;

public class CustomJobsPage : Hangfire.Dashboard.RazorPage
{
    public override void Execute()
    {
        WriteLiteral("<h1>Привет из кастомной страницы!</h1>");
        WriteLiteral("<p>Здесь можно вывести дополнительные данные о заданиях.</p>");
    }
}