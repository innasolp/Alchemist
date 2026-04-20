using System.ComponentModel.DataAnnotations;

namespace Db.Infrastructure.EF.Outbox;

internal class MessageHandlerEntry
{
    [Key]
    public Guid Id { get; set; }

    public Guid MessageId { get; set; }

    public required string HandlerType { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? ProcessedAt { get; set; }

    public string State { get; set; } = Outbox.State.Created.ToString();

    public string? Error { get; set; }
}