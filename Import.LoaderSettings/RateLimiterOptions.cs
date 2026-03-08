namespace Import.LoaderSettings;

public class RateLimiterOptions
{
    public int? WindowMilliseconds { get; set;  }

    public int? QueueLimit { get; set; }
}