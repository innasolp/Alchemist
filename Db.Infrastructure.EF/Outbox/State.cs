namespace Db.Infrastructure.EF.Outbox;

internal enum State
{
    Created,
    Processing,
    Confirmed,
    Failed
}