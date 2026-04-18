using System.ComponentModel.DataAnnotations;

namespace Db.Infrastructure.EF.Outbox;

internal class MessageEntry
{
    [Key]
    public Guid Id { get; set; }

    public required string Category { get; set; }

    public required string EventType { get; set; }

    public required string Payload { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? ProcessedAt { get; set; }

    public string State { get; set; } = "Created";

    public string? Error { get; set; }
}